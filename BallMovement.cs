using UnityEngine;
using DG.Tweening; 

public class BallMovement : MonoBehaviour
{
    public float initSpeed = 10f;
    private Rigidbody2D rb;
    public AudioSource bounceSound;
    private Vector3 originalScale;
    private bool hasThrown = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale; 
    }

    void Update()
    {
        if (BattleManager.isStart && !hasThrown)
        {
            FirstThrow();
            hasThrown = true; 
            Debug.Log(gameObject.name + " Have moved");
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

    void FixedUpdate()
    {
        if (BattleManager.isStart && hasThrown)
        {
            if (rb.linearVelocity.sqrMagnitude > 0)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * initSpeed;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (bounceSound != null && bounceSound.clip != null)
        {
            bounceSound.PlayOneShot(bounceSound.clip);
        }
        transform.DOKill(true);
        transform.localScale = originalScale;
        transform.DOPunchScale(new Vector3(0.1f, -0.1f, 0f), 0.2f, 10, 1);
        float curveAngle = Random.Range(-5f, 5f);
        rb.linearVelocity = Quaternion.Euler(0, 0, curveAngle) * rb.linearVelocity;
        rb.linearVelocity = rb.linearVelocity.normalized * initSpeed;
    }
}