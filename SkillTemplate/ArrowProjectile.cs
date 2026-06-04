using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    public int damage;
    public BallUnit shooterUnit;

    void Start()
    {
        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        BallUnit target = col.GetComponent<BallUnit>();
        if (target != null && target.playerIndex != shooterUnit.playerIndex)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}