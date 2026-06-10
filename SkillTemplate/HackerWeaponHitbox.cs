using UnityEngine;

public class HackerWeaponHitbox : MonoBehaviour
{
    public SwordWithLagAttack skillParent;
    public int teamIndex;

    private float nextHitTime = 0f;
    private const float HIT_COOLDOWN = 0.4f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Time.time < nextHitTime) return;

        BallUnit enemy = collision.GetComponent<BallUnit>();
        if (enemy != null && enemy.playerIndex != teamIndex)
        {
            nextHitTime = Time.time + HIT_COOLDOWN;
            //skillParent.OnHitEnemy(enemy);
        }
    }
}