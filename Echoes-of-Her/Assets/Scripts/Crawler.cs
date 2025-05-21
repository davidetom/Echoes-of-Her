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

    private void ChasePlayer()
    {
        if (!isRecoiling)
        {
            // Calculate direction to player (only on X axis)
            float direction = Mathf.Sign(PlayerController.Instance.transform.position.x - transform.position.x);

            // Apply velocity instead of directly moving transform
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

            // Optional: Face the player
            transform.localScale = new Vector3(direction * transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    protected override void UpdateEnemyStates()
    {
        Death(0.05f);
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
