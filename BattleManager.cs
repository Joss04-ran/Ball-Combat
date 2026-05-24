using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static bool isStart = false;
    void Start()
    {
        isStart = false;
        Invoke("StartBattle",0.0f);
    }

    void Update()
    {
        isStart = true;
        Debug.Log("Start the sim!");
    }
}