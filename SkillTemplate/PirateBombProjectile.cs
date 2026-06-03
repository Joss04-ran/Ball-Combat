using UnityEngine;
using DG.Tweening;

public class PirateBombProjectile : MonoBehaviour
{
    public int damage;
    public float radius;
    public BallUnit shooter;
    public GameObject explosionVFX;

    void Start()
    {
        Destroy(gameObject, 4f); 
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject == shooter.gameObject || col.name == "PirateBomb") return;
        Explode();
    }

    private void Explode()
    {
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D col in hitObjects)
        {
            BallUnit victim = col.GetComponent<BallUnit>();
            if (victim != null && victim.playerIndex != shooter.playerIndex)
            {
                float distance = Vector2.Distance(transform.position, victim.transform.position);
                float damageFactor = 1f - Mathf.Clamp01(distance / radius);
                int finalCalculatedDamage = Mathf.RoundToInt(damage * damageFactor);

                if (finalCalculatedDamage > 0)
                {
                    victim.TakeDamage(finalCalculatedDamage);
                }
            }
        }
        if (explosionVFX != null)
        {
            GameObject fx = Instantiate(explosionVFX, transform.position, Quaternion.identity);
            fx.transform.localScale = new Vector3(radius, radius, 1f) * 0.5f;
            Destroy(fx, 2f);
        }
        Camera.main.transform.DOShakePosition(0.1f, 0.15f);
        Destroy(gameObject); 
    }
}