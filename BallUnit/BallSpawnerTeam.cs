using System.Collections.Generic;
using UnityEngine;

public class BallSpawnerTeam : MonoBehaviour
{
    [Header("Team 1 Setup")]
    public int team1Index = 1;
    public string team1Name = "Team Red";
    public List<GameObject> team1Prefabs;
    public List<Transform> team1Spawns;

    [Header("Team 2 Setup")]
    public int team2Index = 2;
    public string team2Name = "Team Blue";
    public List<GameObject> team2Prefabs;
    public List<Transform> team2Spawns;

    void Start()
    {
        List<BallUnit> team1Units = SpawnTeam(team1Prefabs, team1Spawns, team1Index);
        List<BallUnit> team2Units = SpawnTeam(team2Prefabs, team2Spawns, team2Index);
        AutoAssignWalls(team1Index, team1Units);
        AutoAssignWalls(team2Index, team2Units);
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.SetPlayerName(team1Index, team1Name);
            BattleUIManager.Instance.SetPlayerName(team2Index, team2Name);
        }
    }

    List<BallUnit> SpawnTeam(List<GameObject> prefabs, List<Transform> spawns, int teamIndex)
    {
        List<BallUnit> spawnedUnits = new List<BallUnit>();
        int count = Mathf.Min(prefabs.Count, spawns.Count);

        for (int i = 0; i < count; i++)
        {
            if (prefabs[i] != null && spawns[i] != null)
            {
                GameObject ball = Instantiate(prefabs[i], spawns[i].position, Quaternion.identity);
                BallUnit unit = ball.GetComponent<BallUnit>();
                if (unit != null)
                {
                    unit.playerIndex = teamIndex;
                    spawnedUnits.Add(unit); 
                }
            }
        }
        return spawnedUnits;
    }
    void AutoAssignWalls(int teamIndex, List<BallUnit> teamUnits)
    {
        if (teamUnits.Count < 2) return; 
        ContainmentWall[] walls = FindObjectsByType<ContainmentWall>(FindObjectsSortMode.None);
        foreach (ContainmentWall wall in walls)
        {
            if (wall.teamIndex == teamIndex)
            {
                wall.InitializeWall(teamUnits[0], teamUnits[1]);
                break;
            }
        }
    }
}