using DG.Tweening;
using UnityEngine;

public class ShootExplosiveBombSkill : BaseSkill
{
    public float attackTime;
    public float explosionRadius;
    public string projectileSpriteName;

    private float timer;
    private Rigidbody2D rb;
    private BallUnit thisUnit;
    private Sprite loadedBombSprite;
    private static Sprite fallbackBombSprite;

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
            loadedBombSprite = Resources.Load<Sprite>(projectileSpriteName);
        }

        if (fallbackBombSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.black);
            tex.Apply();
            fallbackBombSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
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
            if (b.playerIndex != thisUnit.playerIndex && b.currentStat.hp > 0)
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
                Shoot(aimDirection);
                timer = attackTime;
            }
        }
    }

    private void Shoot(Vector2 shootDirection)
    {
        rb.linearVelocity *= 0.4f; 
        weaponSprite.transform.DOLocalMoveX(-0.4f, 0.1f).SetLoops(2, LoopType.Yoyo); 
        GameObject bomb = new GameObject("PirateBomb");
        bomb.transform.position = weaponPivot.position + (Vector3)(shootDirection * 0.8f);
        bomb.transform.right = shootDirection;
        bomb.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        SpriteRenderer sr = bomb.AddComponent<SpriteRenderer>();
        sr.sprite = loadedBombSprite != null ? loadedBombSprite : fallbackBombSprite;
        if (loadedBombSprite == null) bomb.transform.localScale = new Vector3(0.2f, 0.2f, 1f);

        CircleCollider2D col = bomb.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.2f;

        Rigidbody2D bombRb = bomb.AddComponent<Rigidbody2D>();
        bombRb.bodyType = RigidbodyType2D.Kinematic;
        bombRb.linearVelocity = shootDirection * 12f;
        PirateBombProjectile logic = bomb.AddComponent<PirateBombProjectile>();
        logic.damage = skillDamage;
        logic.radius = explosionRadius;
        logic.shooter = thisUnit;
        logic.explosionVFX = thisUnit.explosionPrefab;
    }
    public void TriggerOnDeathUltimate()
    {
        if (ultimateData == null) return;

        Debug.Log("ULTIMATE TRIGGERED ON DEATH!");
        Camera.main.transform.DOShakePosition(0.3f, 0.5f);
        if (thisUnit.explosionPrefab != null)
        {
            GameObject ultimateFx = Instantiate(thisUnit.explosionPrefab, transform.position, Quaternion.identity);
            ultimateFx.transform.localScale = new Vector3(ultimateData.explosionRadius, ultimateData.explosionRadius, 1f) * 0.5f;
            Destroy(ultimateFx, 3f);
        }
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, ultimateData.explosionRadius);
        foreach (Collider2D col in targets)
        {
            BallUnit victim = col.GetComponent<BallUnit>();
            if (victim != null && victim.playerIndex != thisUnit.playerIndex)
            {
                float distance = Vector2.Distance(transform.position, victim.transform.position);
                float factor = 1f - Mathf.Clamp01(distance / ultimateData.explosionRadius);
                int finalDamage = Mathf.RoundToInt(ultimateData.damage * factor);

                if (finalDamage > 0)
                {
                    victim.TakeDamage(finalDamage);
                }
            }
        }
    }
}