using UnityEngine;

public class BallMovement : MonoBehaviour
{
    public float initSpeed = 10f;
    private Rigidbody2D rb;
    public AudioSource bounceSound;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (BattleManager.isStart)
        {
            FirstThrow();
            Debug.Log("button has pressed");
        }
    }

    void FirstThrow()
    {
        float dirX = Random.Range(-0.2f, 0.2f);
        float dirY = Random.Range(-0.2f, 0.2f);
        Vector2 dir = new Vector2(dirX, dirY);

        rb.linearVelocity += dir;

        rb.linearVelocity = rb.linearVelocity.normalized * initSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bounceSound.Play();
        float curveAngle = Random.Range(-5f, 5f);

        rb.linearVelocity = Quaternion.Euler(0, 0, curveAngle) * rb.linearVelocity;

        rb.linearVelocity = rb.linearVelocity.normalized * initSpeed;
    }
}
