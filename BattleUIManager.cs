using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

public abstract class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;

    [Header("Universal UI Assets")]
    public Sprite hpGreen;
    public Sprite hpYellow;
    public Sprite hpRed;
    public Sprite hpEmpty;
    public TextMeshProUGUI victoryText;
    public TextAsset descriptionJsonFile;

    protected List<UnitDesc> allDescriptions = new List<UnitDesc>();
    protected Dictionary<int, string> activeTeams = new Dictionary<int, string>();

    protected virtual void Awake()
    {
        Instance = this;
    }

    protected virtual void Start()
    {
        if (victoryText != null) victoryText.gameObject.SetActive(false);
        if (descriptionJsonFile != null)
        {
            DescDatabaseWrapper wrapper = JsonUtility.FromJson<DescDatabaseWrapper>(descriptionJsonFile.text);
            if (wrapper != null) allDescriptions = wrapper.descriptions;
        }
    }

    public abstract void SetPlayerName(int targetIndex, string targetName);
    public abstract void UpdateHealthBar(int targetIndex, int currentHp, int maxHp);

    public virtual void DeclareWinner(int loserIndex)
    {
        if (victoryText == null) return;
        string winnerName = "DRAW";

        foreach (var team in activeTeams)
        {
            if (team.Key != loserIndex)
            {
                winnerName = team.Value;
                break;
            }
        }

        victoryText.text = "WINNER : " + winnerName;
        victoryText.gameObject.SetActive(true);
    }
}