using DG.Tweening;
using UnityEngine;

public class SwordAndPotionsSkill : BaseSkill
{
    private Rigidbody2D rb;
    private BallUnit thisUnit;
    private bool isSwinging = false;
    private Vector3 originalWeaponScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        thisUnit = GetComponent<BallUnit>();
    }

    public override void ActivateSkill()
    {
        if (weaponSprite != null)
        {
            weaponSprite.enabled = true; // Perlihatkan tongkat
            originalWeaponScale = weaponSprite.transform.localScale;
        }
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
        if (!BattleManager.isStart || isSwinging) return;

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
        isSwinging = true;

        Sequence swingSequence = DOTween.Sequence();
        swingSequence.Append(weaponPivot.DOLocalRotate(new Vector3(0, 0, -20), 0.05f).SetRelative().SetEase(Ease.OutQuad));
        swingSequence.Append(weaponPivot.DOLocalRotate(new Vector3(0, 0, 100), 0.1f).SetRelative().SetEase(Ease.InCubic));
        swingSequence.Join(weaponSprite.transform.DOScale(originalWeaponScale * 1.2f, 0.05f).SetLoops(2, LoopType.Yoyo));

        swingSequence.InsertCallback(0.12f, () =>
        {
            if (target != null)
            {
                target.TakeDamage(skillDamage);
                transform.DOShakePosition(0.1f, 0.1f);
                if (ultimateData != null && Random.value <= ultimateData.chance)
                {
                    EvaluateAndThrowPotion();
                }
            }
        });

        swingSequence.AppendInterval(0.05f);
        swingSequence.OnComplete(() =>
        {
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

    private void EvaluateAndThrowPotion()
    {
        if (ultimateData == null || ultimateData.listPotions == null) return;
        float roll = Random.value; 
        var p = ultimateData.listPotions;
        System.Action effectCallback = null;
        Color potionColor = Color.white;
        if (roll <= p.healingPotionChance)
        {
            effectCallback = () => thisUnit.Heal(Mathf.RoundToInt(p.healingAmount));
            potionColor = Color.green; 
        }
        else if (roll <= p.healingPotionChance + p.strengthPotionChance)
        {
            effectCallback = () => thisUnit.ApplyStrengthBuff(Mathf.RoundToInt(p.damageIncrease), ultimateData.duration);
            potionColor = Color.red;
        }
        else
        {
            effectCallback = () => thisUnit.ApplySwiftnessBuff(p.speedIncrease, ultimateData.duration);
            potionColor = Color.yellow;
        }
        VisualThrowPotion(potionColor, effectCallback);
    }
    private void VisualThrowPotion(Color color, System.Action onImpact)
    {
        GameObject potion = new GameObject("ThrownPotion");
        Vector3 startPosition = transform.position; 
        potion.transform.position = startPosition;
        potion.transform.localScale = new Vector3(50f, 50f, 1f);
        SpriteRenderer sr = potion.AddComponent<SpriteRenderer>();
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        sr.color = color;
        sr.sortingOrder = 5;
        float throwDuration = 1f;
        float progress = 0f; 
        DOTween.To(() => progress, x => progress = x, 1f, throwDuration)
            .SetEase(Ease.Linear) 
            .SetUpdate(true)
            .OnUpdate(() =>
            {
                if (potion != null && thisUnit != null)
                {
                    Vector3 currentTraderPos = transform.position;
                    Vector3 basePosition = Vector3.Lerp(startPosition, currentTraderPos, progress);
                    float parabolaHeight = Mathf.Sin(progress * Mathf.PI) * 2f; 
                    potion.transform.position = new Vector3(basePosition.x, basePosition.y + parabolaHeight, basePosition.z);
                    potion.transform.Rotate(0, 0, 720f * Time.deltaTime);
                }
            })
            .OnComplete(() =>
            {
                if (onImpact != null) onImpact.Invoke();
                transform.DOShakeScale(0.15f, 0.3f);
                Destroy(potion);
            });
    }
}