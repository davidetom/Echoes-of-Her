using UnityEngine;

public class Charger : Enemy
{
    [SerializeField] private float ledgeChechX;
    [SerializeField] private float ledgeChechY;
    [SerializeField] private float chargeSpeedMultiplier;
    [SerializeField] private float chargeDuration;
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask whatIsGround;

    private bool beenHit = false;
    private bool playerHit = false;
    RaycastHit2D hit;
    Vector3 direzione;

    float timer;

    protected override void Start()
    {
        base.Start();
        ChangeState(EnemyStates.Charger_Idle);
        rb.gravityScale = 12f;
    }

    protected override void UpdateEnemyStates()
    {
        if (health <= 0)
        {
            Death(0.05f);
        }
        Vector3 ledgeCheckStart = transform.localScale.x > 0 ? new Vector3(ledgeChechX, 0) : new Vector3(-ledgeChechX, 0);
        Vector2 wallChechDir = transform.localScale.x > 0 ? transform.right : -transform.right;
        direzione = wallChechDir;

        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Charger_Idle:
                if (Grounded())
                {
                    // Flip a causa di un ostacolo o di una sporgenza
                    if (!Physics2D.Raycast(transform.position + ledgeCheckStart, Vector2.down, ledgeChechY, whatIsGround)
                    || Physics2D.Raycast(transform.position, wallChechDir, ledgeChechX, whatIsGround))
                    {
                        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                        Flip();
                    }

                    hit = Physics2D.Raycast(transform.position + ledgeCheckStart, wallChechDir, ledgeChechX * 10);
                    if ((hit.collider != null && hit.collider.gameObject.CompareTag("Player")) || beenHit || playerHit)
                    {
                        ChangeState(EnemyStates.Charger_Surprised);
                    }

                    //Flip se viene colpito da dietro
                    if (beenHit || playerHit)
                    {
                        float direction = (PlayerController.Instance.transform.position.x - transform.position.x);
                        if (transform.localScale.x != direction)
                        {
                            Flip();
                        }
                    }

                    //Movimento a destra/sinistra
                    if (transform.localScale.x > 0)
                    {
                        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
                    }
                    else
                    {
                        rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
                    }
                }
                beenHit = false;
                playerHit = false;
                break;

            case EnemyStates.Charger_Surprised:
                rb.linearVelocity = new Vector2(0, jumpForce);

                ChangeState(EnemyStates.Charger_Charge);
                break;

            case EnemyStates.Charger_Charge:
                timer += Time.deltaTime;
                bool blocked = Physics2D.Raycast(transform.position, wallChechDir, ledgeChechX, whatIsGround);
                if (timer < chargeDuration)
                {
                    if (Grounded() && !blocked)
                    {
                        if (transform.localScale.x > 0)
                        {
                            rb.linearVelocity = new Vector2(speed * chargeSpeedMultiplier, rb.linearVelocity.y);
                        }
                        else
                        {
                            rb.linearVelocity = new Vector2(-speed * chargeSpeedMultiplier, rb.linearVelocity.y);
                        }
                    }
                    else
                    {
                        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

                        if (blocked)
                        {
                            ChangeState(EnemyStates.Charger_Idle);
                        }
                    }
                }
                else
                {
                    timer = 0;
                    ChangeState(EnemyStates.Charger_Idle);
                }
                break;
        }
    }

    public override void EnemyHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        base.EnemyHit(damageDone, hitDirection, hitForce);
        beenHit = true;
    }

    protected override void Attack()
    {
        base.Attack();
        playerHit = true;
    }

    protected override void ChangeCurrentAnimation()
    {
        if (GetCurrentEnemyState == EnemyStates.Charger_Idle)
        {
            anim.speed = 1;
        }

        if (GetCurrentEnemyState == EnemyStates.Charger_Charge)
        {
            anim.speed = chargeSpeedMultiplier;
        }
    }

    private void Flip()
    {
        transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
    }

    private bool Grounded()
    {
        return rb.linearVelocity.y == 0;
    }
    
    void OnDrawGizmos()
    {
        Vector3 origine = transform.position;
        Vector3 fine;

        if (hit.collider != null)
        {
            Gizmos.color = Color.red;
            fine = hit.point;
        }
        else
        {
            Gizmos.color = Color.green;
            fine = origine + (Vector3)(direzione * ledgeChechX);
        }

        Gizmos.DrawLine(origine, fine);
        Gizmos.DrawSphere(fine, 0.1f); // opzionale: punto finale visibile
    }
}
