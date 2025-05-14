using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("General Settings:")]
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;
    [Space(5)]

    [Header("Ground Check Settings:")]
    [SerializeField] protected Transform groundCheckPoint;
    [SerializeField] protected float groundCheckY = 0.2f;
    [SerializeField] protected float groundCheckX = 0.5f;
    [SerializeField] protected LayerMask whatIsGround;
    [Space(5)]

    [SerializeField] protected PlayerController player;
    [SerializeField] protected HitFreezeDetection freezeDetector;
    [SerializeField] protected float speed;  

    [SerializeField] protected float damage;

    protected float recoilTimer;
    protected Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {

    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerController.Instance;

        freezeDetector = GetComponent<HitFreezeDetection>();
        if (freezeDetector == null)
        {
            Debug.LogError("HitFreezeDetection non trovato sul GameObject!");
            freezeDetector = gameObject.AddComponent<HitFreezeDetection>();
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        ManageDeath();
        ManageRecoil();
    }

    //Gestione controllo del terreno
    protected virtual bool Grounded()
    {
        if (groundCheckPoint == null) return false;
        
        // Utilizzio di OverlapBox
        Vector2 boxCenter = groundCheckPoint.position;
        Vector2 boxSize = new Vector2(groundCheckX * 2, groundCheckY * 0.5f);
        
        // Box sotto i piedi del personaggio
        Collider2D groundCollider = Physics2D.OverlapBox(
            boxCenter + Vector2.down * groundCheckY * 0.75f, 
            boxSize, 
            0f, 
            whatIsGround
        );
        
        // Debug visivo
        Debug.DrawLine(boxCenter + new Vector2(-boxSize.x/2, 0), 
                       boxCenter + new Vector2(boxSize.x/2, 0), 
                       groundCollider ? Color.green : Color.red);
                       
        return groundCollider != null;
    }


    void ManageRecoil()
    {
        if(isRecoiling)
        {
            if(recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
    }

    virtual public void EnemyHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        health -= damageDone;
        if(!isRecoiling)
        {
            rb.AddForce(-hitForce * recoilFactor * hitDirection);
            isRecoiling = true;
        }
    }

    // Changed to OnTriggerEnter2D and OnCollisionEnter2D for more reliability
    protected void OnTriggerEnter2D(Collider2D other)
    {
        CheckForPlayerCollision(other.gameObject);
    }
    
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        CheckForPlayerCollision(collision.gameObject);
    }
    
    // Also keep OnTriggerStay2D for continuous damage
    protected void OnTriggerStay2D(Collider2D other)
    {
        CheckForPlayerCollision(other.gameObject);
    }
    
    protected void OnCollisionStay2D(Collision2D collision)
    {
        CheckForPlayerCollision(collision.gameObject);
    }
    
    private void CheckForPlayerCollision(GameObject other)
    {
        // Debug player detection
        if (other.CompareTag("Player"))
        {

            Collider2D playerCollider = other.GetComponent<Collider2D>();
            Vector2 hitPoint = playerCollider.ClosestPoint(transform.position);
            Vector2 hitDirection = (transform.position - playerCollider.transform.position).normalized;
            
            // Direct reference to player instead of using the tag
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null && !playerController.pState.invincible)
            {
                Attack();
                freezeDetector.HitManager(hitPoint, hitDirection);
            }
        }
    }

    protected virtual void Attack()
    {
        Vector2Int hitDirection = new Vector2Int(
            (int) Mathf.Sign(player.transform.position.x - transform.position.x),
            (int) Mathf.Sign(player.transform.position.y - transform.position.y)
            );
        player.TakeDamage(damage, hitDirection);
    }

    protected virtual void ManageDeath()
    {
        if(health <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
