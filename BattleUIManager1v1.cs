using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BattleUIManager1v1 : BattleUIManager
{
    [Header("Player 1 UI")]
    public TextMeshProUGUI nameText1;
    public TextMeshProUGUI hpText1;
    public SpriteRenderer healthBar1;
    public TextMeshProUGUI descText1;

    [Header("Player 2 UI")]
    public TextMeshProUGUI nameText2;
    public TextMeshProUGUI hpText2;
    public SpriteRenderer healthBar2;
    public TextMeshProUGUI descText2;

    public override void SetPlayerName(int playerIndex, string ballName)
    {
        activeTeams[playerIndex] = ballName;

        string foundDesc = "";
        UnitDesc match = allDescriptions.Find(d => d.unitName == ballName);
        if (match != null) foundDesc = match.desc;

        if (playerIndex == 1 && nameText1 != null)
        {
            nameText1.text = ballName;
            if (descText1 != null) descText1.text = foundDesc;
        }
        else if (playerIndex == 2 && nameText2 != null)
        {
            nameText2.text = ballName;
            if (descText2 != null) descText2.text = foundDesc;
        }
    }

    public override void UpdateHealthBar(int playerIndex, int currentHp, int maxHp)
    {
        if (currentHp < 0) currentHp = 0;
        float hpPercent = maxHp > 0 ? (float)currentHp / maxHp : 0;
        Sprite currentSprite = hpEmpty;
        if (hpPercent >= 0.7f) currentSprite = hpGreen;
        else if (hpPercent >= 0.4f) currentSprite = hpYellow;
        else if (hpPercent > 0f) currentSprite = hpRed;

        if (playerIndex == 1)
        {
            if (healthBar1 != null) healthBar1.sprite = currentSprite;
            if (hpText1 == null)
                return;
            if (currentHp > 999)
            {
                hpText1.text = "Inf / Inf";
            }
            else
            {
                hpText1.text = currentHp + " / " + maxHp;
            }
        }
        else if (playerIndex == 2)
        {
            if (healthBar2 != null) healthBar2.sprite = currentSprite;
            if (hpText2 == null)
                return;
            if (currentHp > 999)
            {
                hpText2.text = "Inf / Inf";
            }
            else
            {
                hpText2.text = currentHp + " / " + maxHp;
            }
        }
    }
}