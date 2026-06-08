using UnityEngine;
using DG.Tweening;

public class SwordCursedAndDrainHealth : BaseSkill
{
    private Rigidbody2D rb;
    private BallUnit thisUnit;
    private int hpDrain;
    private bool isSwinging = false;
    private bool ultimateTriggered = false;
    private Tween swingTween;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        thisUnit = GetComponent<BallUnit>();
    }
    void Update()
    {
        if (!BattleManager.isStart) return;
        if (!isSwinging && rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            weaponPivot.right = rb.linearVelocity.normalized;
        }
    }

    private void PerformSwingAnimation()
    {
        if (swingTween != null && swingTween.active)
            swingTween.Kill();

        isSwinging = true;
        float swingDuration = 0.4f;

        swingTween = weaponPivot.DORotate(
            weaponPivot.eulerAngles + new Vector3(0, 0, 120f),
            swingDuration * 0.5f,
            RotateMode.FastBeyond360
        ).OnComplete(() =>
        {
            if (this != null)
            {
                weaponPivot.DORotate(
                    weaponPivot.eulerAngles - new Vector3(0, 0, 120f),
                    swingDuration * 0.5f,
                    RotateMode.FastBeyond360
                ).SetLink(gameObject).OnComplete(() =>
                {
                    if (this != null) isSwinging = false;
                });
            }
        }).SetLink(gameObject);
    }

    public override void InitializeWeapon(Transform pivot, SpriteRenderer sprite)
    {
        base.InitializeWeapon(pivot, sprite);
        hpDrain = thisUnit.currentStat.hpDrainAmount > 0 ? thisUnit.currentStat.hpDrainAmount : 15;

        if (weaponSprite != null)
        {
            weaponSprite.enabled = true;
            weaponSprite.color = new Color(0.6f, 0f, 0.8f);
            BoxCollider2D col = weaponSprite.gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            CursedWeaponHitbox hitbox = weaponSprite.gameObject.AddComponent<CursedWeaponHitbox>();
            hitbox.skillParent = this;
            hitbox.teamIndex = thisUnit.playerIndex;
        }
    }

    public override void ActivateSkill() { }

    public void OnHitEnemy(BallUnit enemy)
    {
        enemy.TakeDamage(skillDamage);
        thisUnit.PlayAttackSound();
        DrainMaxHealth(enemy);
        CheckAndTriggerUltimate(enemy.transform.position);
        PerformSwingAnimation();
    }

    private void DrainMaxHealth(BallUnit enemy)
    {
        if (enemy == null || enemy.currentStat.hp <= 0) return;

        if (enemy.currentStat.hp > enemy.maxHp) enemy.currentStat.hp = enemy.maxHp;
        thisUnit.currentStat.hp += hpDrain;

        thisUnit.UpdateTeamHealthBar();
        enemy.UpdateTeamHealthBar();

        if (enemy.transform.localScale.x > 0.5f)
            enemy.transform.DOScale(enemy.transform.localScale - new Vector3(0.1f, 0.1f, 0f), 0.3f).SetLink(enemy.gameObject);

        if (thisUnit.transform.localScale.x < 2.5f)
            thisUnit.transform.DOScale(thisUnit.transform.localScale + new Vector3(0.1f, 0.1f, 0f), 0.3f).SetLink(thisUnit.gameObject);

        if (thisUnit.damagePopup != null)
            Instantiate(thisUnit.damagePopup, 
                transform.position, Quaternion.identity)
                .GetComponent<DamagePopup>().Setup($"+{hpDrain} HP");
    }

    private void CheckAndTriggerUltimate(Vector2 spawnPoint)
    {
        if (ultimateTriggered) return;

        float hpPercent = (float)thisUnit.currentStat.hp / thisUnit.maxHp;
        if (hpPercent <= 0.3f && Random.value <= ultimateData.chance)
        {
            ultimateTriggered = true;
            SpawnMinions(spawnPoint);
        }
    }

    private void SpawnMinions(Vector2 spawnPosition)
    {
        int count = ultimateData.minionCount > 0 ? ultimateData.minionCount : 3;
        float duration = ultimateData.minionDuration > 0 ? ultimateData.minionDuration : 3f;

        if (thisUnit.buffParticle != null) thisUnit.buffParticle.Play();
        for (int i = 0; i < count; i++)
        {
            if (thisUnit.minionPrefab != null)
            {
                GameObject minion = Instantiate(thisUnit.minionPrefab, spawnPosition + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)), Quaternion.identity);

                SkullMinion logic = minion.GetComponent<SkullMinion>();
                if (logic != null)
                {
                    logic.teamIndex = thisUnit.playerIndex;
                    logic.damage = ultimateData.damage;
                    logic.summonerId = thisUnit.gameObject;
                }

                Collider2D minionCol = minion.GetComponent<Collider2D>();
                Collider2D necromancerCol = thisUnit.GetComponent<Collider2D>();
                if (minionCol != null && necromancerCol != null)
                {
                    Physics2D.IgnoreCollision(necromancerCol, minionCol, true);
                }

                // Ignore collision with all other allied units
                BallUnit[] allBalls = FindObjectsByType<BallUnit>(FindObjectsSortMode.None);
                foreach (BallUnit ally in allBalls)
                {
                    if (ally.playerIndex == thisUnit.playerIndex)
                    {
                        Collider2D allyCol = ally.GetComponent<Collider2D>();
                        if (allyCol != null && minionCol != null)
                        {
                            Physics2D.IgnoreCollision(minionCol, allyCol, true);
                        }
                    }
                }

                Destroy(minion, duration);
            }
        }
    }
}