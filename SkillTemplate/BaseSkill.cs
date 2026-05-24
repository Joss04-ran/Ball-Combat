using UnityEngine;

public abstract class BaseSkill : MonoBehaviour
{
    public int skillDamage;
    protected UltimateSkillData ultimateData;
    protected Transform weaponPivot;
    protected SpriteRenderer weaponSprite;

    public virtual void InitializeWeapon(Transform pivot, SpriteRenderer sprite)
    {
        weaponPivot = pivot;
        weaponSprite = sprite;

        if (weaponSprite != null)
        {
            weaponSprite.enabled = false;
        }
    }
    public abstract void ActivateSkill();

    public void SetDamage(int damageValue)
    {
        skillDamage = damageValue;
    }

    public void SetUltimateData(UltimateSkillData ultData)
    {
        ultimateData = ultData;
    }
}