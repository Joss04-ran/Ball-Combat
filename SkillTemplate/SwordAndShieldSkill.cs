using DG.Tweening;
using UnityEngine;

public class SwordAndShieldSkill : BaseSkill
{
    private Rigidbody2D rb;
    private Transform ultimatePivot;
    private bool isUltimateActive = false;
    private bool isSwinging = false;

    private TrailRenderer weaponTrail;
    private Vector3 originalWeaponScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (weaponSprite != null)
        {
            weaponTrail = weaponSprite.GetComponent<TrailRenderer>();
            if (weaponTrail != null) weaponTrail.emitting = false;

            originalWeaponScale = weaponSprite.transform.localScale;
        }
    }

    public override void ActivateSkill()
    {
        if (weaponSprite != null) weaponSprite.enabled = true;
    }

    void Update()
    {
        if (!BattleManager.isStart) return;
        if (!isSwinging && rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            weaponPivot.right = rb.linearVelocity.normalized;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!BattleManager.isStart) return;
        if (isSwinging) return;

        BallUnit targetBall = collision.gameObject.GetComponent<BallUnit>();

        if (targetBall != null)
        {
            Vector2 contactDir = (collision.GetContact(0).point - (Vector2)transform.position).normalized;
            float hitAngle = Vector2.Dot(contactDir, weaponPivot.right);

            if (hitAngle > 0.4f)
            {
                PlaySwingAnimation(targetBall);
            }
        }
    }
    private void PlaySwingAnimation(BallUnit target)
    {
        if (isSwinging || weaponSprite == null) return;
        isSwinging = true;

        if (weaponTrail != null) weaponTrail.emitting = true;

        Sequence swingSequence = DOTween.Sequence();
        swingSequence.Append(weaponPivot.DOLocalRotate(new Vector3(0, 0, -20), 0.05f).SetRelative().SetEase(Ease.OutQuad));
        swingSequence.Append(weaponPivot.DOLocalRotate(new Vector3(0, 0, 100), 0.1f).SetRelative().SetEase(Ease.InCubic));
        swingSequence.Join(weaponSprite.transform.DOScale(originalWeaponScale * 1.3f, 0.05f).SetLoops(2, LoopType.Yoyo));
        swingSequence.InsertCallback(0.12f, () =>
        {
            if (target != null)
            {
                if (ultimateData != null && Random.value <= ultimateData.chance)
                {
                    TriggerUltimateCombo();
                }
                else
                {
                    target.TakeDamage(skillDamage);
                    transform.DOShakePosition(0.1f, 0.2f);
                }
            }
        });
        swingSequence.AppendInterval(0.05f);
        swingSequence.OnComplete(() =>
        {
            if (weaponTrail != null) weaponTrail.emitting = false;
            if (rb != null)
            {
                weaponPivot.DORotateQuaternion(Quaternion.LookRotation(Vector3.forward, Vector3.Cross(Vector3.forward, rb.linearVelocity.normalized)), 0.1f)
                .OnComplete(() => isSwinging = false);
            }
            else
            {
                isSwinging = false;
            }
        });
    }
    private void TriggerUltimateCombo()
    {
        if (isUltimateActive || weaponSprite == null) return;
        isUltimateActive = true;
        Debug.Log("KNIGHT ULTIMATE!");
        ultimatePivot = new GameObject("UltimatePivot").transform;
        ultimatePivot.SetParent(transform);
        ultimatePivot.localPosition = Vector3.zero;
        float angleStep = 360f / ultimateData.swordCount;
        for (int i = 0; i < ultimateData.swordCount; i++)
        {
            GameObject sword = Instantiate(weaponSprite.gameObject, ultimatePivot);
            if (sword.GetComponent<TrailRenderer>()) sword.GetComponent<TrailRenderer>().emitting = true;
            sword.transform.localPosition = Quaternion.Euler(0, 0, angleStep * i) * Vector3.right * 1.5f;
            sword.transform.right = sword.transform.localPosition.normalized;
            BoxCollider2D col = sword.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            SwordHitbox hitbox = sword.AddComponent<SwordHitbox>();
            hitbox.damage = ultimateData.damage;
        }
        ultimatePivot.DORotate(new Vector3(0, 0, -360), 10f / ultimateData.spinSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);
        DOVirtual.DelayedCall(ultimateData.swordStayTime, () =>
        {
            if (ultimatePivot != null) Destroy(ultimatePivot.gameObject);
            isUltimateActive = false;
        });
    }
}