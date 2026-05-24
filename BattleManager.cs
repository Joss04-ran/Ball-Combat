using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static bool isStart = false;
    void Start()
    {
        isStart = false;
    }

    void Update()
    {
        if (!isStart && Input.GetKeyDown(KeyCode.Space))
        {
            isStart = true;
            Debug.Log("BATTLE STARTED!");
        }
    }
}