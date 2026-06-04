using UnityEngine;
using DG.Tweening;

public class ContainmentWall : MonoBehaviour
{
    public int teamIndex;
    private BallUnit activeAlly;       
    private BallUnit containedAlly;    
    private Collider2D wallCollider;
    private SpriteRenderer wallSprite;
    private BaseSkill containedSkill;
    private bool isOpened = false;
    [HideInInspector] public bool isInitialized = false;
    void Awake()
    {
        wallCollider = GetComponent<Collider2D>();
        wallSprite = GetComponent<SpriteRenderer>();
    }
    public void InitializeWall(BallUnit active, BallUnit contained)
    {
        activeAlly = active;
        containedAlly = contained;
        isInitialized = true;
        Invoke("FreezeContainedAlly", 0.1f);
    }
    void FreezeContainedAlly()
    {
        containedAlly.isInvincible = true;
        BaseSkill skill = containedAlly.GetComponent<BaseSkill>();
        skill.enabled = false;
    }
    void Update()
    {
        if (!isInitialized || isOpened) return;
        if (activeAlly == null || activeAlly.currentStat.hp <= 0)
        {
            ReleaseAlly();
        }
    }

    void ReleaseAlly()
    {
        isOpened = true;
        wallSprite.DOFade(0.0f, 1f);
        wallCollider.enabled = false;
        containedAlly.isInvincible = false;
        BaseSkill skill = containedAlly.GetComponent<BaseSkill>();
        if (skill != null) skill.enabled = true;

        Debug.Log("Containment Wall Opened! Backup unit entering the field.");
    }
}