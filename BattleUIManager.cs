using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;
    public TextMeshProUGUI nameText1;
    public TextMeshProUGUI hpText1;
    public SpriteRenderer healthBar1; 
    public TextMeshProUGUI nameText2;
    public SpriteRenderer healthBar2;
    public TextMeshProUGUI hpText2;
    public Sprite hpGreen;
    public Sprite hpYellow;
    public Sprite hpRed;
    public Sprite hpEmpty;
    public TextMeshProUGUI victoryText; 
    private string namePlayer0;
    private string namePlayer1;
    void Start()
    {
        if (victoryText != null)
        {
            victoryText.gameObject.SetActive(false);
        }
    }
    void Awake()
    {
        Instance = this;
    }

    public void SetPlayerName(int playerIndex, string ballName)
    {
        if (playerIndex == 1 && nameText1 != null)
        {
            nameText1.text = ballName;
            namePlayer0 = nameText1.text;
        }
        else if (playerIndex == 2 && nameText2 != null)
        {
            nameText2.text = ballName;
            namePlayer1 = nameText2.text;
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
            if (hpText1 != null) hpText1.text = currentHp + " / " + maxHp;
        }
        else if (playerIndex == 2)
        {
            if (healthBar2 != null) healthBar2.sprite = currentSprite;
            if (hpText2 != null) hpText2.text = currentHp + " / " + maxHp;
        }
    }

    public void DeclareWinner(int loserIndex)
    {
        if (victoryText == null) return;

        string winnerName = "";
        if (loserIndex == 1) winnerName = namePlayer1;
        else if (loserIndex == 2) winnerName = namePlayer0;

        victoryText.text = "WINNER : " + winnerName;
        victoryText.gameObject.SetActive(true);
    }
}
