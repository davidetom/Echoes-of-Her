using UnityEngine;

public class Crawler : Enemy
{
    [SerializeField] private float flipWaitTime;
    [SerializeField] private float ledgeChechX;
    [SerializeField] private float ledgeChechY;
    [SerializeField] private LayerMask whatIsGround;

    float timer;

    protected override void Start()
    {
        base.Start();
        ChangeState(EnemyStates.Crawler_Idle);
        rb.gravityScale = 12f;
    }
    
    // AGGIUNGI QUESTI METODI PER DEBUG E COLLISIONI
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Crawler OnTriggerEnter2D with: {other.name}, Tag: {other.tag}");
        base.OnTriggerEnter2D(other);
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Crawler OnCollisionEnter2D with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        base.OnCollisionEnter2D(collision);
    }

    protected override void Attack()
    {
        Debug.Log("Crawler is attacking player!");
        base.Attack();
    }

    protected override void UpdateEnemyStates()
    {
        if (health <= 0)
        {
            Death(0.05f);
        }

        if (PlayerController.Instance == null) return;

        if (!PlayerController.Instance.pState.alive) return;

        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Crawler_Idle:
                Vector3 ledgeCheckStart = transform.localScale.x > 0 ? new Vector3(ledgeChechX, 0) : new Vector3(-ledgeChechX, 0);
                Vector2 wallChechDir = transform.localScale.x > 0 ? transform.right : -transform.right;

                if (!Physics2D.Raycast(transform.position + ledgeCheckStart, Vector2.down, ledgeChechY, whatIsGround)
                    || Physics2D.Raycast(transform.position, wallChechDir, ledgeChechX, whatIsGround))
                {
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                    ChangeState(EnemyStates.Crawler_Flip);
                }
                else
                {
                    if (transform.localScale.x > 0)
                    {
                        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
                    }
                    else
                    {
                        rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
                    }
                }
                break;

            case EnemyStates.Crawler_Flip:
                timer += Time.deltaTime;
                if (timer > flipWaitTime)
                {
                    timer = 0;
                    transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
                    ChangeState(EnemyStates.Crawler_Idle);
                }
                break;
        }
    }
}
