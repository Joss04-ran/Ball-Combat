using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BallDatabaseWrapper
{
    public List<BallStatData> ballList;
}

[System.Serializable]
public class PotionListData
{
    public float healingPotionChance;
    public float healingAmount;
    public float strengthPotionChance;
    public float damageIncrease;
    public float swiftPotionChance;
    public float speedIncrease;
}

[System.Serializable]
public class UltimateSkillData
{
    public int damage;
    public int attackCount;
    public float chance;
    public int swordCount;
    public float spinSpeed;
    public float swordStayTime;
    public float speedIncrease;
    public float evasionChance;
    public float durationBuff;
    public float accuracy;
    public PotionListData listPotions;
    public float duration;
    public float chanceIncreasePerUnitDefeat;
    public float explosionRadius;
    public int minionCount;
    public float minionDuration;
    public float corruptionDuration;
}

[System.Serializable]
public class BallStatData
{
    public string name;
    public int hp;
    public int damage;
    public float movementSpeed;
    public float evasionChance;
    public float size;
    public float mass;
    public float blockChance;
    public float damageReduce;
    public float critChance;
    public float critDamage;
    public float attackTime;
    public float accuracy;
    public float interval;
    public float attackTimeDecrease;
    public string skillTemplate;
    public string spriteWeapon;
    public string spriteProjectile;
    public UltimateSkillData ultimateSkill;
    public int amount;
    public float explosionRadius;
    public int hpDrainAmount;
    public float lagChance;
    public float lagDuration;
}