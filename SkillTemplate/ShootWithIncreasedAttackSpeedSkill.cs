using DG.Tweening;
using UnityEngine;

public class ShootWithIncreasedAttackSpeedSkill : BaseSkill
{
    public float baseAccuracy;
    public float currentAccuracy;
    public float attackTime;
    public float interval;
    public float attackTimeDecrease;
    public string projectileSpriteName;

    private float shootTimer;
    private float speedUpTimer;
    private Rigidbody2D rb;
    private BallUnit thisUnit;
    public AudioClip shootClip;

    private Sprite loadedBulletSprite;
    private static Sprite fallbackBulletSprite;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        thisUnit = GetComponent<BallUnit>();
    }

    public override void ActivateSkill()
    {
        currentAccuracy = baseAccuracy; 
        if (weaponSprite != null) weaponSprite.enabled = true;

        shootTimer = attackTime;
        speedUpTimer = interval;

        if (!string.IsNullOrEmpty(projectileSpriteName))
        {
            loadedBulletSprite = Resources.Load<Sprite>(projectileSpriteName);
        }

        if (fallbackBulletSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.black);
            tex.Apply();
            fallbackBulletSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }
    }

    void Update()
    {
        if (!BattleManager.isStart) return;

        shootTimer -= Time.deltaTime;
        speedUpTimer -= Time.deltaTime;
        if (speedUpTimer <= 0)
        {
            attackTime = Mathf.Max(0.1f, attackTime - attackTimeDecrease);
            speedUpTimer = interval;
            Debug.Log($"Attack Speed Increased! Current Delay: {attackTime}");
        }
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
            if (shootTimer <= 0)
            {
                Shoot(aimDirection);
                if (shootClip != null)
                {
                    AudioSource.PlayClipAtPoint(shootClip, transform.position);
                }
                shootTimer = attackTime;
            }
        }
    }

    private void Shoot(Vector2 baseAimDirection)
    {
        rb.linearVelocity *= 0.5f;
        weaponSprite.transform.DOLocalMoveX(-0.3f, 0.1f).SetLoops(2, LoopType.Yoyo);
        float maxSpreadAngle = 15f; 
        float spread = (1f - currentAccuracy) * maxSpreadAngle;
        float randomAngle = Random.Range(-spread, spread);
        Vector2 finalShootDirection = Quaternion.Euler(0, 0, randomAngle) * baseAimDirection;
        CreateBullet(finalShootDirection, skillDamage);
        if (ultimateData != null && Random.value <= ultimateData.chance)
        {
            thisUnit.ApplySheriffUltimateBuff(ultimateData.accuracy, ultimateData.durationBuff);
        }
    }

    private void CreateBullet(Vector2 dir, int damage)
    {
        GameObject bullet = new GameObject("SheriffBullet");
        bullet.transform.position = weaponPivot.position + (Vector3)(dir * 0.8f);
        bullet.transform.right = dir;
        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = loadedBulletSprite != null ? loadedBulletSprite : fallbackBulletSprite;
        if (loadedBulletSprite == null) bullet.transform.localScale = new Vector3(2f, 2f, 1f);
        PolygonCollider2D col = bullet.AddComponent<PolygonCollider2D>();
        col.isTrigger = true;
        Rigidbody2D bulletRb = bullet.AddComponent<Rigidbody2D>();
        bulletRb.bodyType = RigidbodyType2D.Kinematic;
        bulletRb.linearVelocity = dir * 25f;
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
        ParticleSystem ps = bullet.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.2f;  
        main.startSpeed = 0f;       
        main.startSize = 0.15f;     
        main.startColor = Color.yellow; 
        main.simulationSpace = ParticleSystemSimulationSpace.World; 
        var emission = ps.emission;
        emission.rateOverTime = 40;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.05f;
        var renderer = bullet.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        ArrowProjectile logic = bullet.AddComponent<ArrowProjectile>();
        logic.damage = damage;
        logic.shooterUnit = thisUnit;
    }
}