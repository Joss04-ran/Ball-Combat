using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    public int damage;
    private float nextHitTime = 0f;

    void OnTriggerStay2D(Collider2D col)
    {
        if (Time.time < nextHitTime) return;
        BallUnit target = col.GetComponent<BallUnit>();
        if (target != null && target.gameObject != transform.root.gameObject)
        {
            target.TakeDamage(damage);
            nextHitTime = Time.time + 0.5f;
        }
    }
}