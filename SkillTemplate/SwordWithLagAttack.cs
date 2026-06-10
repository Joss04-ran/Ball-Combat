using UnityEngine;
using DG.Tweening;

public class SwordWithLagAttack : BaseSkill
{
    // ── References ────────────────────────────────────────────────────────
    private Rigidbody2D rb;
    private BallUnit thisUnit;

    // ── State ─────────────────────────────────────────────────────────────
    private bool isSwinging = false;
    private bool isUltimateActive = false;
    private Vector3 originalWeaponScale;

    // ── Lag parameters injected from JSON via SetLagParameters() ─────────
    private float lagChance;
    private float lagDuration;

    // ─────────────────────────────────────────────────────────────────────
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        thisUnit = GetComponent<BallUnit>();
    }

    public void SetLagParameters(float chance, float duration)
    {
        lagChance = chance;
        lagDuration = duration;
    }

    // ── Weapon visual setup (called after InitializeWeapon sets weaponSprite)
    public override void ActivateSkill()
    {
        if (weaponSprite == null) return;

        weaponSprite.enabled = true;
        originalWeaponScale = weaponSprite.transform.localScale;

        // Holographic cyan with looping alpha pulse
        weaponSprite.color = new Color(0f, 0.9f, 1f, 1f);
        weaponSprite.DOColor(new Color(0.2f, 1f, 1f, 0.35f), 0.45f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(weaponSprite.gameObject);
    }

    // ── Track weapon pivot toward movement direction ───────────────────
    void Update()
    {
        if (!BattleManager.isStart) return;
        if (!isSwinging && rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
            weaponPivot.right = rb.linearVelocity.normalized;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  HIT DETECTION  — follows SwordAndShieldSkill.cs exactly
    // ─────────────────────────────────────────────────────────────────────
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!BattleManager.isStart) return;
        if (isSwinging) return;

        BallUnit targetBall = collision.gameObject.GetComponent<BallUnit>();
        if (targetBall == null) return;

        // Only deal damage when the dagger faces the contact direction
        Vector2 contactDir = (collision.GetContact(0).point - (Vector2)transform.position).normalized;
        float hitAngle = Vector2.Dot(contactDir, weaponPivot.right);

        if (hitAngle > 0.4f)
            PlaySwingAnimation(targetBall);
    }
    private void PlaySwingAnimation(BallUnit target)
    {
        if (isSwinging || weaponSprite == null) return;
        isSwinging = true;

        Sequence swingSeq = DOTween.Sequence();

        // Wind-back then slash
        swingSeq.Append(weaponPivot.DOLocalRotate(new Vector3(0, 0, -20f), 0.05f)
            .SetRelative().SetEase(Ease.OutQuad));
        swingSeq.Append(weaponPivot.DOLocalRotate(new Vector3(0, 0, 110f), 0.1f)
            .SetRelative().SetEase(Ease.InCubic));

        // Scale pop on weapon sprite during swing
        swingSeq.Join(weaponSprite.transform
            .DOScale(originalWeaponScale * 1.3f, 0.05f)
            .SetLoops(2, LoopType.Yoyo));

        // Damage + effects at peak of swing (0.12 s into sequence)
        swingSeq.InsertCallback(0.12f, () =>
        {
            if (target == null) return;

            target.TakeDamage(skillDamage);
            thisUnit.PlayAttackSound();
            transform.DOShakePosition(0.1f, 0.2f);

            // 20% chance → Lag / Desync
            if (Random.value <= lagChance)
                ApplyLagEffect(target);

            // 10% chance → Data Corruption ultimate
            if (ultimateData != null && !isUltimateActive && Random.value <= ultimateData.chance)
                ApplyDataCorruption();
        });

        swingSeq.AppendInterval(0.05f);

        // Rotate weapon pivot back to movement direction, then unlock
        swingSeq.OnComplete(() =>
        {
            if (rb != null)
            {
                weaponPivot.DORotateQuaternion(
                    Quaternion.LookRotation(Vector3.forward,
                        Vector3.Cross(Vector3.forward, rb.linearVelocity.normalized)), 0.1f)
                    .OnComplete(() => isSwinging = false);
            }
            else
            {
                isSwinging = false;
            }
        });
    }
    private void ApplyLagEffect(BallUnit enemy)
    {
        if (enemy == null || enemy.currentStat.hp <= 0) return;

        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        BallMovement enemyMovement = enemy.GetComponent<BallMovement>();
        BaseSkill enemySkill = enemy.GetComponent<BaseSkill>();

        // Don't stack lag if already frozen
        if (enemyRb != null && enemyRb.bodyType == RigidbodyType2D.Kinematic) return;

        // Store state for restoration
        float originalSpeed = enemyMovement != null ? enemyMovement.initSpeed : 10f;
        Vector2 lastDir = (enemyRb != null && enemyRb.linearVelocity.sqrMagnitude > 0.01f)
                          ? enemyRb.linearVelocity.normalized : Vector2.right;

        TriggerTimeStopVisual();
        if (enemyMovement != null) enemyMovement.initSpeed = 0f;
        if (enemyRb != null)
        {
            enemyRb.linearVelocity = Vector2.zero;
            enemyRb.bodyType = RigidbodyType2D.Kinematic;
        }
        if (enemySkill != null) enemySkill.enabled = false;
        SpriteRenderer enemySr = enemy.GetComponent<SpriteRenderer>();
        Color originalColor = enemySr != null ? enemySr.color : Color.white;
        if (enemySr != null)
        {
            DOTween.Sequence()
                .Append(enemySr.DOColor(new Color(0f, 1f, 1f, 0.55f), 0.07f))
                .Append(enemySr.DOColor(originalColor, 0.07f))
                .SetLoops(14, LoopType.Yoyo)
                .SetLink(enemy.gameObject);
        }

        if (thisUnit.damagePopup != null)
            Instantiate(thisUnit.damagePopup,
                        enemy.transform.position + Vector3.up * 0.65f,
                        Quaternion.identity)
                .GetComponent<DamagePopup>().Setup("LAGGED!");
        float freezeDuration = lagDuration > 0f ? lagDuration : 3f;
        DOVirtual.DelayedCall(freezeDuration, () =>
        {
            if (enemy == null) return;
            if (enemyMovement != null) enemyMovement.initSpeed = originalSpeed;
            if (enemyRb != null)
            {
                enemyRb.bodyType = RigidbodyType2D.Dynamic;
                enemyRb.linearVelocity = lastDir * originalSpeed;
            }
            if (enemySkill != null) enemySkill.enabled = true;
            if (enemySr != null)
            {
                enemySr.DOKill(false);
                enemySr.DOColor(originalColor, 0.25f).SetLink(enemy.gameObject);
            }
        });
    }
    private void TriggerTimeStopVisual()
    {
        thisUnit.PlaySpecialSkillSound();
        Time.timeScale = 0.02f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        Camera cam = Camera.main;
        if (cam != null)
        {
            GameObject overlay = new GameObject("TimeStopOverlay");
            SpriteRenderer overlaySr = overlay.AddComponent<SpriteRenderer>();

            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            overlaySr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            overlaySr.color = new Color(0f, 0.08f, 0.4f, 0f);
            overlaySr.sortingOrder = 999;

            overlay.transform.SetParent(cam.transform);
            overlay.transform.localPosition = new Vector3(0f, 0f, 10f);

            float screenH = cam.orthographicSize * 2f;
            float screenW = screenH * cam.aspect;
            overlay.transform.localScale = new Vector3(screenW * 1.5f, screenH * 1.5f, 1f);

            DOTween.Sequence()
                .Append(overlaySr.DOColor(new Color(0f, 0.08f, 0.4f, 0.72f), 0.07f).SetUpdate(true))
                .AppendInterval(0.26f)
                .Append(overlaySr.DOColor(new Color(0f, 0.08f, 0.4f, 0f), 0.22f).SetUpdate(true))
                .SetUpdate(true)
                .OnComplete(() => {
                    Destroy(tex); 
                    Destroy(overlay);
                });

            cam.transform.DOKill();
            cam.transform
                .DOShakePosition(0.5f, 0.3f, 20, 90f, false, true)
                .SetUpdate(true);
        }

        SpriteRenderer selfSr = GetComponent<SpriteRenderer>();
        if (selfSr != null)
        {
            Color selfOrig = selfSr.color;
            DOTween.Sequence()
                .Append(selfSr.DOColor(Color.cyan, 0.06f).SetUpdate(true))
                .Append(selfSr.DOColor(selfOrig, 0.06f).SetUpdate(true))
                .Append(selfSr.DOColor(Color.cyan, 0.06f).SetUpdate(true))
                .Append(selfSr.DOColor(selfOrig, 0.05f).SetUpdate(true))
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        transform.DOPunchScale(Vector3.one * 0.3f, 0.45f, 9, 0.5f)
            .SetUpdate(true)
            .SetLink(gameObject);

        DOVirtual.DelayedCall(0.42f, () =>
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }, ignoreTimeScale: true);
    }

    private void ApplyDataCorruption()
    {
        isUltimateActive = true;

        float duration = (ultimateData != null && ultimateData.corruptionDuration > 0f)
            ? ultimateData.corruptionDuration : 3f;

        if (Camera.main != null)
            Camera.main.transform.DOShakePosition(0.3f, 0.45f, 18).SetUpdate(true);

        SpriteRenderer selfSr = GetComponent<SpriteRenderer>();
        if (selfSr != null)
        {
            Color selfOrig = selfSr.color;
            selfSr.DOColor(new Color(0.6f, 0f, 1f), 0.2f).SetLink(gameObject)
                .OnComplete(() =>
                {
                    DOVirtual.DelayedCall(duration, () =>
                    {
                        if (selfSr != null)
                            selfSr.DOColor(selfOrig, 0.3f).SetLink(gameObject);
                    }).SetLink(gameObject);
                });
        }

        BallUnit[] allBalls = FindObjectsByType<BallUnit>(FindObjectsSortMode.None);
        foreach (BallUnit target in allBalls)
        {
            if (target == null || target.playerIndex == thisUnit.playerIndex) continue;

            BaseSkill targetSkill = target.GetComponent<BaseSkill>();
            if (targetSkill == null) continue;
            targetSkill.enabled = false;

            SpriteRenderer targetSr = target.GetComponent<SpriteRenderer>();
            Color origColor = targetSr != null ? targetSr.color : Color.white;
            if (targetSr != null)
            {
                int loops = Mathf.Max(1, Mathf.RoundToInt(duration / 0.22f));
                DOTween.Sequence()
                    .Append(targetSr.DOColor(new Color(1f, 0f, 0.85f, 0.7f), 0.07f))
                    .Append(targetSr.DOColor(new Color(0f, 1f, 0.2f, 0.7f), 0.07f))
                    .Append(targetSr.DOColor(origColor, 0.08f))
                    .SetLoops(loops, LoopType.Restart)
                    .SetLink(target.gameObject);
            }

            if (thisUnit.damagePopup != null)
                Instantiate(thisUnit.damagePopup,
                            target.transform.position + Vector3.up,
                            Quaternion.identity)
                    .GetComponent<DamagePopup>().Setup("CORRUPTED!");

            BaseSkill cs = targetSkill;
            SpriteRenderer csr = targetSr;
            Color cc = origColor;
            BallUnit ct = target;

            DOVirtual.DelayedCall(duration, () =>
            {
                if (cs != null)
                {
                    cs.enabled = true;
                }
                if (csr != null && ct != null)
                {
                    csr.DOKill(false);
                    csr.DOColor(cc, 0.3f);
                }
            });
        }
        DOVirtual.DelayedCall(duration, () => { isUltimateActive = false; })
            .SetLink(gameObject);
    }
}