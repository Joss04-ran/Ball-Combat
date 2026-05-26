using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public static bool isStart = false;
    void Start()
    {
        isStart = false;
        Invoke("StartBattle",2.0f);
    }

    void StartBattle()
    {
        isStart = true;
        Debug.Log("Start The Sim");
    }

    public void EndGame(int loserIndex)
    {
        isStart = false; 

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.DeclareWinner(loserIndex);
        }
        Invoke("RestartScene", 3.0f);
    }

    void RestartScene()
    {
        Application.Quit();
    }
}