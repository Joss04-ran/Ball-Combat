using DG.Tweening;
using UnityEngine;

public class SimpleShootProjectileSkill : BaseSkill
{
    public float critChance;
    public float critDamage;
    public float attackTime;

    private float timer;
    private Rigidbody2D rb;
    private BallUnit thisUnit;
    public AudioClip shootClip;

    public string projectileSpriteName;
    private Sprite loadedArrowSprite; 
    private static Sprite fallbackArrowSprite;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        thisUnit = GetComponent<BallUnit>();
    }

    public override void ActivateSkill()
    {
        if (weaponSprite != null) weaponSprite.enabled = true;
        timer = attackTime;
        if (!string.IsNullOrEmpty(projectileSpriteName))
        {
            loadedArrowSprite = Resources.Load<Sprite>(projectileSpriteName);

            if (loadedArrowSprite == null)
            {
                Debug.LogWarning(projectileSpriteName + " not available!");
            }
        }
        if (fallbackArrowSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            fallbackArrowSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }
    }

    void Update()
    {
        if (!BattleManager.isStart) return;
        timer -= Time.deltaTime;
        BallUnit[] allBalls = FindObjectsByType<BallUnit>(FindObjectsSortMode.None);
        BallUnit target = null;
        float closestDist = Mathf.Infinity;
        foreach (BallUnit b in allBalls)
        {
            if (b.playerIndex != thisUnit.playerIndex)
            {
                float dist = Vector2.Distance(transform.position, b.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    target = b;
                }
            }
        }
        if (target != null)
        {
            Vector2 aimDirection = (target.transform.position - transform.position).normalized;
            weaponPivot.right = aimDirection;
            if (timer <= 0)
            {
                Shoot(target, aimDirection);
                if (shootClip != null)
                {
                    AudioSource.PlayClipAtPoint(shootClip, transform.position);
                }
                timer = attackTime;
            }
        }
    }

    private void Shoot(BallUnit target, Vector2 shootDirection)
    {
        rb.linearVelocity *= 0.2f;
        weaponSprite.transform.DOLocalMoveX(-0.5f, 0.2f).SetLoops(2, LoopType.Yoyo);
        int finalDamage = skillDamage;
        bool isCrit = Random.value <= critChance;
        if (isCrit)
        {
            finalDamage = Mathf.RoundToInt(skillDamage + critDamage);
            Debug.Log("CRITICAL HIT READY!");
        }
        CreateArrow(shootDirection, finalDamage, isCrit);
        if (ultimateData != null && Random.value <= ultimateData.chance)
        {
            thisUnit.ApplyUltimateBuff(ultimateData.speedIncrease,
    ultimateData.evasionChance, ultimateData.durationBuff);
        }
    }

    private void CreateArrow(Vector2 dir, int damage, bool isCrit)
    {
        GameObject arrow = new GameObject("ArrowProjectile");
        arrow.transform.position = weaponPivot.position + (Vector3)(dir * 0.8f);
        arrow.transform.right = dir;
        arrow.transform.Rotate(0, 0, -45f);
        arrow.transform.localScale = new Vector3(0.6f, 0.08f, 1f);

        SpriteRenderer sr = arrow.AddComponent<SpriteRenderer>();
        sr.sprite = loadedArrowSprite != null ? loadedArrowSprite : fallbackArrowSprite;        
        if (loadedArrowSprite != null) arrow.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
        else arrow.transform.localScale = new Vector3(0.6f, 0.08f, 1f);
        sr.color = isCrit ? Color.red : Color.white; 

        PolygonCollider2D col = arrow.AddComponent<PolygonCollider2D>();
        col.isTrigger = true;

        Rigidbody2D arrowRb = arrow.AddComponent<Rigidbody2D>();
        arrowRb.bodyType = RigidbodyType2D.Kinematic;
        arrowRb.linearVelocity = dir * 15f;

        Collider2D shooterCol = GetComponent<Collider2D>();
        if (shooterCol != null)
        {
            Physics2D.IgnoreCollision(shooterCol, col);
        }
        BallUnit myUnit = GetComponent<BallUnit>();
        if (myUnit != null)
        {
            myUnit.PlayAttackSound();
        }
        ArrowProjectile logic = arrow.AddComponent<ArrowProjectile>();
        logic.damage = damage;
        logic.shooterUnit = thisUnit;
    }
}