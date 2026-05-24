using DG.Tweening;
using UnityEngine;

public class UnarmedSkill : BaseSkill
{
    protected float hitInterval;
    protected float punchDistance;
    public override void ActivateSkill()
    {
        Debug.Log("Unarmed Skill activated");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!BattleManager.isStart) return;
        BallUnit targetBall = collision.gameObject.GetComponent<BallUnit>();

        if (targetBall != null)
        {
            if (ultimateData != null && Random.value <= ultimateData.chance)
            {
                TriggerUltimateCombo(targetBall, collision.transform.position);
            }
            else
            {
                targetBall.TakeDamage(skillDamage);
            }
        }
    }

    private void TriggerUltimateCombo(BallUnit target, Vector3 contactPosition)
    {
        Debug.Log("ULTIMATE TRIGGERED!");
        if (weaponPivot == null || weaponSprite == null) return;
        Vector2 direction = contactPosition - transform.position;
        weaponPivot.right = direction.normalized;

        Time.timeScale = 0.05f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        Vector3 originalLocalPos = weaponSprite.transform.localPosition;
        if (gameObject.name == "Giant")
        {
            Camera.main.transform.DOShakePosition(0.2f, 0.4f, 10, 90, false, true);
            hitInterval = 0.16f;
            punchDistance = 1.5f;
        }
        else
        {
            hitInterval = 0.08f;
            punchDistance = 2f;
        }
        for (int i = 0; i < ultimateData.attackCount; i++)
        {
            DOVirtual.DelayedCall(i * hitInterval, () =>
            {
                if (target != null)
                {
                    target.TakeDamage(ultimateData.damage);
                    weaponSprite.transform.DOKill(); 
                    weaponSprite.enabled = true;
                    weaponSprite.transform.localPosition = originalLocalPos; 
                    weaponSprite.transform.DOLocalMoveX(originalLocalPos.x + punchDistance, hitInterval / 2f)
                        .SetEase(Ease.OutFlash)
                        .SetUpdate(true) 
                        .OnComplete(() =>
                        {
                            weaponSprite.transform.DOLocalMoveX(originalLocalPos.x, hitInterval / 2f)
                                .SetUpdate(true)
                                .OnComplete(() =>
                                {
                                    weaponSprite.enabled = false;
                                });
                        });
                }
            }, ignoreTimeScale: true);
        }
        float totalComboDuration = ultimateData.attackCount * hitInterval;
        DOVirtual.DelayedCall(totalComboDuration + 0.2f, () =>
        {
            if (weaponSprite != null)
            {
                weaponSprite.transform.DOKill();
                weaponSprite.transform.localPosition = originalLocalPos;
                weaponSprite.enabled = false;
            }
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }, ignoreTimeScale: true);
    }
}
