using UnityEngine;

public class SkullMinion : MonoBehaviour
{
    public int teamIndex;
    public int damage;
    public GameObject summonerId;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        BallUnit enemy = collision.gameObject.GetComponent<BallUnit>();
        if (enemy != null && enemy.playerIndex != teamIndex)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (collision.gameObject == summonerId)
        {
            // Don't attack the unit that summoned us
            return;
        }
        else if (collision.gameObject.CompareTag("Projectile"))
        {
            Destroy(gameObject);
        }
    }
}
