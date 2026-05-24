using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class UnarmedWithIncreasedChancePerDefeatSkill : BaseSkill
{
    public float chanceIncreasePerUnitDefeat;
    private static Dictionary<int, int> teamDefeatCounts = new Dictionary<int, int>();
    private BallUnit thisUnit;

    public static void IncrementDefeatCount(int playerIndex)
    {
        if (!teamDefeatCounts.ContainsKey(playerIndex)) teamDefeatCounts[playerIndex] = 0;
        teamDefeatCounts[playerIndex]++;
    }

    void Awake()
    {
        thisUnit = GetComponent<BallUnit>();
    }

    public override void ActivateSkill()
    {
        Debug.Log("Brawlings Skill template activated");
        if (teamDefeatCounts.ContainsKey(thisUnit.playerIndex))
        {
            teamDefeatCounts[thisUnit.playerIndex] = 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!BattleManager.isStart) return;

        BallUnit targetBall = collision.gameObject.GetComponent<BallUnit>();

        if (targetBall != null)
        {
            if (targetBall.playerIndex == thisUnit.playerIndex) return;
            int deathCount = teamDefeatCounts.ContainsKey(thisUnit.playerIndex) ? teamDefeatCounts[thisUnit.playerIndex] : 0;
            float finalChance = ultimateData.chance + (deathCount * chanceIncreasePerUnitDefeat);

            if (ultimateData != null && Random.value <= finalChance)
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
        Debug.Log("BRAWLINGS ULTIMATE FLURRY!");

        if (weaponPivot == null || weaponSprite == null) return;
        Vector2 direction = contactPosition - transform.position;
        weaponPivot.right = direction.normalized;

        Time.timeScale = 0.05f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        Vector3 originalLocalPos = weaponSprite.transform.localPosition;
        float hitInterval = 0.08f;
        float punchDistance = 2f;

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
                                .OnComplete(() => weaponSprite.enabled = false);
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