using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BattleUIManager2vs2 : BattleUIManager
{
    [Header("Team 1 UI (Combined)")]
    public TextMeshProUGUI teamNameText1;
    public TextMeshProUGUI teamHpText1;
    public SpriteRenderer teamHealthBar1;

    [Header("Team 2 UI (Combined)")]
    public TextMeshProUGUI teamNameText2;
    public TextMeshProUGUI teamHpText2;
    public SpriteRenderer teamHealthBar2;

    public override void SetPlayerName(int teamIndex, string teamName)
    {
        activeTeams[teamIndex] = teamName;

        if (teamIndex == 1 && teamNameText1 != null) teamNameText1.text = teamName;
        else if (teamIndex == 2 && teamNameText2 != null) teamNameText2.text = teamName;
    }

    public override void UpdateHealthBar(int teamIndex, int currentHp, int maxHp)
    {
        if (currentHp < 0) currentHp = 0;
        float hpPercent = maxHp > 0 ? (float)currentHp / maxHp : 0;
        Sprite currentSprite = hpEmpty;
        if (hpPercent >= 0.7f) currentSprite = hpGreen;
        else if (hpPercent >= 0.4f) currentSprite = hpYellow;
        else if (hpPercent > 0f) currentSprite = hpRed;

        if (teamIndex == 1)
        {
            if (teamHealthBar1 != null) teamHealthBar1.sprite = currentSprite;
            if (teamHpText1 != null) teamHpText1.text = currentHp + " / " + maxHp;
        }
        else if (teamIndex == 2)
        {
            if (teamHealthBar2 != null) teamHealthBar2.sprite = currentSprite;
            if (teamHpText2 != null) teamHpText2.text = currentHp + " / " + maxHp;
        }
    }
}