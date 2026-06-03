using UnityEngine;

public class BallSpawnerTeam : MonoBehaviour
{
    [Header("Ball Settings")]
    public GameObject ballPrefab;
    public GameObject ballPrefab2;
    public GameObject ballPrefab3;
    public GameObject ballPrefab4;

    [Header("Spawn Points")]
    public Transform spawnPoint1; 
    public Transform spawnPoint2; 
    public Transform spawnPoint3;
    public Transform spawnPoint4;

    void Start()
    {
        SpawnBalls();
    }

    void SpawnBalls()
    {
        if (ballPrefab != null)
        {
            GameObject ball1 = Instantiate(ballPrefab, spawnPoint1.position, Quaternion.identity);
            ball1.GetComponent<BallUnit>().playerIndex = 1;

            GameObject ball2 = Instantiate(ballPrefab2, spawnPoint2.position, Quaternion.identity);
            ball2.GetComponent<BallUnit>().playerIndex = 2;
            Debug.Log("Spawned 2 balls");
        }
        else
        {
            Debug.LogError("No Balls");
        }
    }
}