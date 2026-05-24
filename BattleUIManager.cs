using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
[System.Serializable]
public class UnitDesc
{
    public string unitName;
    public string desc;
}

[System.Serializable]
public class DescDatabaseWrapper
{
    public List<UnitDesc> descriptions;
}

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;
    public TextMeshProUGUI nameText1;
    public TextMeshProUGUI hpText1;
    public SpriteRenderer healthBar1;
    public TextMeshProUGUI descText1;
    public TextMeshProUGUI nameText2;
    public SpriteRenderer healthBar2;
    public TextMeshProUGUI hpText2;
    public TextMeshProUGUI descText2;
    public Sprite hpGreen;
    public Sprite hpYellow;
    public Sprite hpRed;
    public Sprite hpEmpty;
    public TextMeshProUGUI victoryText; 
    private string namePlayer0;
    private string namePlayer1;
    public TextAsset descriptionJsonFile;
    private List<UnitDesc> allDescriptions = new List<UnitDesc>();
    void Start()
    {
        if (victoryText != null)
        {
            victoryText.gameObject.SetActive(false);
        }
        if (descriptionJsonFile != null)
        {
            DescDatabaseWrapper wrapper = JsonUtility.FromJson<DescDatabaseWrapper>(descriptionJsonFile.text);
            if (wrapper != null) allDescriptions = wrapper.descriptions;
        }
    }
    void Awake()
    {
        Instance = this;
    }

    public void SetPlayerName(int playerIndex, string ballName)
    {
        string foundDesc = "";
        UnitDesc match = allDescriptions.Find(d => d.unitName == ballName);
        if (match != null) foundDesc = match.desc;

        if (playerIndex == 1 && nameText1 != null)
        {
            nameText1.text = ballName;
            namePlayer0 = nameText1.text;
            if (descText1 != null) descText1.text = foundDesc; 
        }
        else if (playerIndex == 2 && nameText2 != null)
        {
            nameText2.text = ballName;
            namePlayer1 = nameText2.text;
            if (descText2 != null) descText2.text = foundDesc; 
        }
    }

    public void UpdateHealthBar(int playerIndex, int currentHp, int maxHp)
    {
        if (currentHp < 0) currentHp = 0;
        float hpPercent = (float)currentHp / maxHp;
        Sprite currentSprite = hpEmpty;
        if (hpPercent >= 0.7f) currentSprite = hpGreen;
        else if (hpPercent >= 0.4f) currentSprite = hpYellow;
        else if (hpPercent > 0f) currentSprite = hpRed;
        if (playerIndex == 1)
        {
            if (healthBar1 != null) healthBar1.sprite = currentSprite;
            if (hpText1 != null)
            {
                if (currentHp > 999)
                {
                    hpText1.text = "Inf / Inf";
                }
                else
                {
                    hpText1.text = currentHp + " / " + maxHp;
                }
            }
        }
        else if (playerIndex == 2)
        {
            if (healthBar2 != null) healthBar2.sprite = currentSprite;
            if (hpText2 != null)
            {
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

    public void DeclareWinner(int loserIndex)
    {
        if (victoryText == null) return;

        string winnerName = "";
        if (loserIndex == 1) winnerName = namePlayer1;
        else if (loserIndex == 2) winnerName = namePlayer0;
        else winnerName = "DRAW";
        
        if (winnerName == "DRAW") victoryText.text = winnerName;
        else victoryText.text = "WINNER : " + winnerName;
        victoryText.gameObject.SetActive(true);
    }
}
