using Unity.VisualScripting;
using UnityEngine;

public class Bat : Enemy
{
    [SerializeField] private float chaseDistance;
    [SerializeField] private float stunDuration;
    [SerializeField] private float deathAnimationDuration = 2f;

    private float timer;
    private bool isDying = false;
    private bool deathAnimationStarted = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        ChangeState(EnemyStates.Bat_Idle);
    }

    protected override void Update()
    {
        ManageRecoil();
    }

    protected override void UpdateEnemyStates()
    {
        if (PlayerController.Instance == null) return;

        // Gestisce la morte prima di qualsiasi altro stato
        if (health <= 0 && !isDying)
        {
            isDying = true;
            ChangeState(EnemyStates.Bat_Death);
            return;
        }

        float dist = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);

        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Bat_Idle:
                if (dist < chaseDistance && health > 0)
                {
                    ChangeState(EnemyStates.Bat_Chase);
                }

                rb.linearVelocity = Vector2.zero;
                break;

            case EnemyStates.Bat_Chase:
                if (!isRecoiling && health > 0) // Muoviti solo se non stai subendo recoil
                {
                    // Segui il giocatore con un movimento fluido
                    Vector2 direction = (PlayerController.Instance.transform.position - transform.position).normalized;
                    rb.linearVelocity = direction * speed;

                    // Ruota il pipistrello verso il giocatore
                    FlipBat();
                }

                // Se il giocatore è troppo lontano, torna allo stato idle
                if (dist > chaseDistance * 1.5f && health > 0)
                {
                    ChangeState(EnemyStates.Bat_Idle);
                }
                break;

            case EnemyStates.Bat_Stunned:
                rb.linearVelocity = Vector2.zero;
                timer += Time.deltaTime;
                if (timer > stunDuration)
                {
                    if (health > 0)
                    {
                        ChangeState(EnemyStates.Bat_Idle);
                        rb.linearVelocity = Vector2.zero;
                    }
                    else
                    {
                        isDying = true;
                        ChangeState(EnemyStates.Bat_Death);
                    }
                    timer = 0;
                }
                break;

                case EnemyStates.Bat_Death:
                if (!deathAnimationStarted)
                {
                    // Inizializza la morte solo una volta
                    deathAnimationStarted = true;
                    rb.linearVelocity = Vector2.zero;
                    rb.gravityScale = 12;
                    timer = 0;
                    
                    Debug.Log("Death animation started for " + gameObject.name);
                }
                
                timer += Time.deltaTime;
                if (timer >= deathAnimationDuration)
                {
                    Debug.Log("Destroying " + gameObject.name + " after " + timer + " seconds");
                    Death(0f);
                }
                break;
        }
    }

    protected override void ChangeCurrentAnimation()
    {
        if (anim == null) return;

        // Reset di tutti i parametri
        anim.SetBool("Idle", false);
        anim.SetBool("Chase", false);
        anim.SetBool("Stunned", false);

        // Debug per vedere quale stato stiamo impostando
        Debug.Log("Changing animation to: " + GetCurrentEnemyState);

        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Bat_Idle:
                anim.SetBool("Idle", true);
                break;
            case EnemyStates.Bat_Chase:
                anim.SetBool("Chase", true);
                break;
            case EnemyStates.Bat_Stunned:
                anim.SetBool("Stunned", true);
                break;
            case EnemyStates.Bat_Death:
                Debug.Log("Setting Death trigger for " + gameObject.name);
                anim.SetTrigger("Death");
                break;
        }
    }

    public override void EnemyHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        base.EnemyHit(damageDone, hitDirection, hitForce);

        if (health > 0)
        {
            ChangeState(EnemyStates.Bat_Stunned);
            timer = 0;
        }
        else if (!isDying)
        {
            isDying = true;
            ChangeState(EnemyStates.Bat_Death);
        }
    }

    protected override void Death(float deathTime)
    {
        base.Death(deathTime);
    }

    void FlipBat()
    {
        sr.flipX = PlayerController.Instance.transform.position.x < transform.position.x;
    }
    
    // Utile per il debug visivo
    void OnDrawGizmosSelected()
    {
        // Visualizza il raggio di inseguimento
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
    }
}
