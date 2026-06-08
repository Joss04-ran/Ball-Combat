using UnityEngine;

public class CursedWeaponHitbox : MonoBehaviour
{
    public SwordCursedAndDrainHealth skillParent;
    public int teamIndex;
    private SkullMinion cachedMinion;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        BallUnit enemy = collision.GetComponent<BallUnit>();
        if (enemy != null && enemy.playerIndex != teamIndex)
        {
            skillParent.OnHitEnemy(enemy);
        }

        // Don't attack minions
        cachedMinion = collision.GetComponent<SkullMinion>();
        if (cachedMinion != null)
        {
            return;
        }
    }
}