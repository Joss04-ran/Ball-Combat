using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using DG.Tweening;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class BallUnit : MonoBehaviour
{
    public TextAsset jsonFile;
    public string unitNameToLoad;
    public GameObject damagePopup;
    public SpriteRenderer weaponSprite;
    public int playerIndex;
    public Transform weaponPivot;
    public SpriteRenderer weaponSpriteRenderer;
    [HideInInspector] public int maxHp;
    [HideInInspector] public bool isClone = false;
    [HideInInspector] public BallStatData currentStat;
    private BallMovement movement;
    public GameObject explosionPrefab;
    public AudioClip hitSound;
    public AudioClip attackSound;
    private AudioSource audioSource;
    public ParticleSystem buffParticle;
    private float baseSpeed;
    private int baseAttack;
    public bool isInvincible = false;
    public static List<BallUnit> activeBalls = new List<BallUnit>();
    public GameObject minionPrefab;
    public AudioClip specialSkillSound;

    void Awake()
    {
        movement = GetComponent<BallMovement>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        LoadStatsFromJson();
        Invoke("DisableTeamCollisions", 0.1f);
    }

    public void PlayAttackSound()
    {
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    public void PlaySpecialSkillSound()
    {
        if (audioSource != null && specialSkillSound != null)
        {
            audioSource.PlayOneShot(specialSkillSound);
        }
    }

    void LoadStatsFromJson()
    {
        try
        {
            if (jsonFile != null)
            {
                BallDatabaseWrapper database = JsonUtility.FromJson<BallDatabaseWrapper>(jsonFile.text);

                currentStat = database.ballList.Find(ball => ball.name == unitNameToLoad);

                if (currentStat != null)
                {
                    gameObject.name = currentStat.name;
                    maxHp = currentStat.hp;
                    baseAttack = currentStat.damage;

                    if (movement != null)
                    {
                        movement.initSpeed = currentStat.movementSpeed;
                        baseSpeed = currentStat.movementSpeed;
                    }

                    transform.DOScale(new Vector3(currentStat.size, currentStat.size, 1f), currentStat.size - 0.2f)
                        .SetEase(Ease.OutBack)
                        .SetLink(gameObject);

                    if (!isClone && currentStat.amount > 1)
                    {
                        for (int i = 0; i < currentStat.amount - 1; i++)
                        {
                            GameObject clone = Instantiate(gameObject, transform.position, Quaternion.identity);
                            BallUnit cloneUnit = clone.GetComponent<BallUnit>();
                            cloneUnit.isClone = true;
                            cloneUnit.playerIndex = this.playerIndex;
                            cloneUnit.unitNameToLoad = this.unitNameToLoad;
                            cloneUnit.jsonFile = this.jsonFile;
                            clone.transform.localScale = Vector3.zero;
                        }
                    }

                    ApplySkillTemplate(currentStat.skillTemplate);
                }

                if (BattleUIManager.Instance != null && BattleUIManager.Instance is BattleUIManager1v1)
                {
                    BattleUIManager.Instance.SetPlayerName(playerIndex, currentStat.name);
                }
                BattleUIManager.Instance.UpdateHealthBar(playerIndex, currentStat.hp, maxHp);

                Debug.Log($"Data Loaded. HP: {currentStat.hp}, Skill: {currentStat.skillTemplate}");
            }
            else
            {
                throw new Exception("JSON File not found! Attach in the inspector!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    void ApplySkillTemplate(string skillName)
    {
        BaseSkill skill = null;

        if (skillName == "Unarmed")
        {
            skill = gameObject.AddComponent<UnarmedSkill>();
        }
        else if (skillName == "SwordAndShield")
        {
            skill = gameObject.AddComponent<SwordAndShieldSkill>();
        }
        else if (skillName == "SimpleShootProjectile")
        {
            skill = gameObject.AddComponent<SimpleShootProjectileSkill>();
            var archer = skill as SimpleShootProjectileSkill;
            archer.critChance = currentStat.critChance;
            archer.critDamage = currentStat.critDamage;
            archer.attackTime = currentStat.attackTime;
            archer.projectileSpriteName = currentStat.spriteProjectile;
        }
        else if (skillName == "ShootWithIncreasedAttackSpeed")
        {
            skill = gameObject.AddComponent<ShootWithIncreasedAttackSpeedSkill>();
            var sheriff = skill as ShootWithIncreasedAttackSpeedSkill;
            sheriff.baseAccuracy = currentStat.accuracy;
            sheriff.attackTime = currentStat.attackTime;
            sheriff.interval = currentStat.interval;
            sheriff.attackTimeDecrease = currentStat.attackTimeDecrease;
            sheriff.projectileSpriteName = currentStat.spriteProjectile;
        }
        else if (skillName == "SwordAndPotions")
        {
            skill = gameObject.AddComponent<SwordAndPotionsSkill>();
        }
        else if (skillName == "UnarmedWithIncreasedChancePerDefeat")
        {
            skill = gameObject.AddComponent<UnarmedWithIncreasedChancePerDefeatSkill>();
            var brawlingsSkill = skill as UnarmedWithIncreasedChancePerDefeatSkill;
            if (brawlingsSkill != null)
            {
                brawlingsSkill.chanceIncreasePerUnitDefeat = currentStat.ultimateSkill.chanceIncreasePerUnitDefeat;
            }
        }
        else if (skillName == "ShootExplosiveBomb")
        {
            skill = gameObject.AddComponent<ShootExplosiveBombSkill>();
            var pirate = skill as ShootExplosiveBombSkill;
            if (pirate != null)
            {
                pirate.attackTime = currentStat.attackTime > 0 ? currentStat.attackTime : 1.5f;
                pirate.explosionRadius = currentStat.explosionRadius > 0 ? currentStat.explosionRadius : 2.5f;
                pirate.projectileSpriteName = currentStat.spriteProjectile;
            }
        }
        else if (skillName == "SwordCursedAndDrainHealth")
        {
            skill = gameObject.AddComponent<SwordCursedAndDrainHealth>();
        }

        // ── Hacker Ball ───────────────────────────────────────────────────
        else if (skillName == "SwordWithLagAttack")
        {
            skill = gameObject.AddComponent<SwordWithLagAttack>();
            var hacker = skill as SwordWithLagAttack;
            if (hacker != null)
            {
                // Inject JSON-driven Lag parameters
                hacker.SetLagParameters(currentStat.lagChance, currentStat.lagDuration);
            }
        }

        if (skill != null)
        {
            skill.SetDamage(currentStat.damage);
            skill.SetUltimateData(currentStat.ultimateSkill);
            skill.InitializeWeapon(weaponPivot, weaponSpriteRenderer);
            skill.ActivateSkill();
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;
        if (currentStat.evasionChance > 0 && UnityEngine.Random.value < currentStat.evasionChance)
        {
            Debug.Log(currentStat.name + " Dodge the attack");
            if (damagePopup != null)
            {
                GameObject popup = Instantiate(damagePopup, transform.position, Quaternion.identity);
                popup.GetComponent<DamagePopup>().Setup("Miss!");
                return;
            }
        }
        if (currentStat.blockChance > 0 && UnityEngine.Random.value < currentStat.blockChance)
        {
            float reducedDamage = damage * (1f - currentStat.damageReduce);
            damage = Mathf.RoundToInt(reducedDamage);
            Debug.Log(currentStat.name + " Blocked! Damage reduced.");
            if (damagePopup != null)
                Instantiate(damagePopup, transform.position, Quaternion.identity)
                    .GetComponent<DamagePopup>().Setup(damage.ToString());
        }
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        currentStat.hp -= damage;
        UpdateTeamHealthBar();
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.UpdateHealthBar(playerIndex, currentStat.hp, maxHp);
        }
        Debug.Log(gameObject.name + " Receive " + damage + " HP Left : " + currentStat.hp);

        if (damagePopup != null)
        {
            GameObject popup = Instantiate(damagePopup, transform.position, Quaternion.identity);
            popup.GetComponent<DamagePopup>().Setup(damage.ToString());
        }

        if (currentStat.hp <= 0)
        {
            Defeat();
        }
    }

    public void ApplyUltimateBuff(float extraSpeed, float extraEvasion, float duration)
    {
        Debug.Log($"{gameObject.name} ULTIMATE ACTIVE!");
        float originalEvasion = currentStat.evasionChance;
        movement.initSpeed = extraSpeed;
        currentStat.evasionChance = extraEvasion;
        buffParticle.Play();
        SpriteRenderer ballSprite = GetComponent<SpriteRenderer>();
        Color originalColor = ballSprite != null ? ballSprite.color : Color.white;
        if (ballSprite != null) ballSprite.color = Color.yellow;
        DG.Tweening.DOVirtual.DelayedCall(duration, () =>
        {
            if (this != null && movement != null)
            {
                movement.initSpeed = baseSpeed;
                currentStat.evasionChance = originalEvasion;
                if (ballSprite != null) ballSprite.color = originalColor;
                Debug.Log($"{gameObject.name} Buff Ended.");
                buffParticle.Stop();
            }
        }).SetLink(gameObject);
    }

    public void ApplySheriffUltimateBuff(float ultimateAccuracy, float duration)
    {
        Debug.Log($"{gameObject.name} SHERIFF ULTIMATE! High Noon Accuracy!");
        var skill = GetComponent<ShootWithIncreasedAttackSpeedSkill>();
        if (skill != null)
        {
            float originalAccuracy = skill.currentAccuracy;
            skill.currentAccuracy = ultimateAccuracy;
            if (buffParticle != null) buffParticle.Play();
            DG.Tweening.DOVirtual.DelayedCall(duration, () =>
            {
                if (this != null && skill != null)
                {
                    skill.currentAccuracy = originalAccuracy;
                    if (buffParticle != null) buffParticle.Stop();
                    Debug.Log($"{gameObject.name} Sheriff Buff Ended.");
                }
            }).SetLink(gameObject);
        }
    }

    public void Heal(int amount)
    {
        currentStat.hp = Mathf.Min(maxHp, currentStat.hp + amount);
        if (BattleUIManager.Instance != null) BattleUIManager.Instance.UpdateHealthBar(playerIndex, currentStat.hp, maxHp);
        Debug.Log($"{gameObject.name} healed for {amount}. HP: {currentStat.hp}");
        if (damagePopup != null)
            Instantiate(damagePopup, transform.position, Quaternion.identity)
                .GetComponent<DamagePopup>().Setup($"+{amount} HP");
    }

    public void ApplyStrengthBuff(int extraDamage, float duration)
    {
        var skill = GetComponent<SwordAndPotionsSkill>();
        if (skill != null)
        {
            skill.skillDamage = extraDamage;
            if (damagePopup != null)
                Instantiate(damagePopup, transform.position, Quaternion.identity)
                    .GetComponent<DamagePopup>().Setup("+ATK!");
            if (weaponSpriteRenderer != null) weaponSpriteRenderer.color = Color.red;
            DG.Tweening.DOVirtual.DelayedCall(duration, () =>
            {
                if (skill != null)
                {
                    skill.skillDamage = baseAttack;
                    if (weaponSpriteRenderer != null) weaponSpriteRenderer.color = Color.white;
                }
            }).SetLink(gameObject);
        }
    }

    public void ApplySwiftnessBuff(float extraSpeed, float duration)
    {
        if (movement != null)
        {
            movement.initSpeed = extraSpeed;
            if (damagePopup != null)
                Instantiate(damagePopup, transform.position, Quaternion.identity)
                    .GetComponent<DamagePopup>().Setup("+SPEED!");
            if (buffParticle != null) buffParticle.Play();
            DG.Tweening.DOVirtual.DelayedCall(duration, () =>
            {
                if (movement != null)
                {
                    movement.initSpeed = baseSpeed;
                    if (buffParticle != null) buffParticle.Stop();
                }
            }).SetLink(gameObject);
        }
    }

    public void UpdateTeamHealthBar()
    {
        if (BattleUIManager.Instance == null) return;
        int totalCurrentHp = 0;
        int totalMaxHp = 0;
        BallUnit[] allBalls = FindObjectsByType<BallUnit>(FindObjectsSortMode.None);
        foreach (var ball in allBalls)
        {
            if (ball.playerIndex == this.playerIndex && ball.currentStat != null)
            {
                if (ball.currentStat.hp > 0) totalCurrentHp += ball.currentStat.hp;
                totalMaxHp = ball.maxHp;
            }
        }
        BattleUIManager.Instance.UpdateHealthBar(playerIndex, totalCurrentHp, totalMaxHp);
    }

    void DisableTeamCollisions()
    {
        Collider2D myCol = GetComponent<Collider2D>();
        if (myCol == null) return;
        BallUnit[] allBalls = FindObjectsByType<BallUnit>(FindObjectsSortMode.None);
        foreach (BallUnit ally in allBalls)
        {
            if (ally != null && ally != this && ally.playerIndex == this.playerIndex)
            {
                Collider2D allyCol = ally.GetComponent<Collider2D>();
                if (allyCol != null)
                    Physics2D.IgnoreCollision(myCol, allyCol, true);
            }
        }
    }

    private void Defeat()
    {
        Debug.Log(gameObject.name + " Has defeated");
        if (gameObject.name == "Pirate")
        {
            var pirateSkill = GetComponent<ShootExplosiveBombSkill>();
            if (pirateSkill != null) pirateSkill.TriggerOnDeathUltimate();
        }
        if (gameObject.name == "Brawlings")
        {
            UnarmedWithIncreasedChancePerDefeatSkill.IncrementDefeatCount(playerIndex);
        }
        activeBalls.Remove(this);
        bool anyTeamAlive = false;
        BallUnit[] allBalls = FindObjectsByType<BallUnit>(FindObjectsSortMode.None);
        foreach (var ball in allBalls)
        {
            if (ball != this && ball.playerIndex == this.playerIndex
                && ball.currentStat != null && ball.currentStat.hp > 0)
            {
                anyTeamAlive = true;
                break;
            }
        }
        if (!anyTeamAlive && BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.DeclareWinner(playerIndex);
        }
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        foreach (var ball in allBalls)
        {
            if (ball != this && ball.playerIndex == this.playerIndex
                && ball.currentStat != null && ball.currentStat.hp > 0)
            {
                ball.UpdateTeamHealthBar();
                break;
            }
        }
        Destroy(gameObject);
    }
}