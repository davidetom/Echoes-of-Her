using System;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static event Action<Enemy> OnEnemySpawned;

    [Header("General Settings:")]
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;
    [Space(5)]

    [SerializeField] protected HitFreezeDetection freezeDetector;

    [SerializeField] protected float speed;  

    [SerializeField] protected float damage;
    [SerializeField] protected GameObject orangeBlood;

    protected float recoilTimer;
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Animator anim;

    protected enum EnemyStates
    {
        //Crawler
        Crawler_Idle,
        Crawler_Flip,

        //Bat
        Bat_Idle,
        Bat_Chase,
        Bat_Stunned,
        Bat_Death,

        //Charger
        Charger_Idle,
        Charger_Surprised,
        Charger_Charge
    }
    protected EnemyStates currentEnemyStates;

    protected virtual EnemyStates GetCurrentEnemyState
    {
        get { return currentEnemyStates; }
        set
        {
            if (currentEnemyStates != value)
            {
                currentEnemyStates = value;

                ChangeCurrentAnimation();
            }
        }
    }

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        freezeDetector = GetComponent<HitFreezeDetection>();
        if (freezeDetector == null)
        {
            Debug.LogError("HitFreezeDetection non trovato sul GameObject!");
            freezeDetector = gameObject.AddComponent<HitFreezeDetection>();
        }
    }

    protected virtual void OnEnable()
    {
        // Notifica che un nuovo nemico è stato creato/abilitato
        if (OnEnemySpawned != null)
        {
            OnEnemySpawned(this);
        }
    }

    // Per completezza, aggiungiamo anche OnDisable per garantire la pulizia dell'evento
    protected virtual void OnDisable()
    {
        // Non necessario fare nulla qui per l'evento statico,
        // ma potrebbe essere un buon posto per la pulizia di altre risorse
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (health <= 0)
        {
            Death(0.05f);
        }
        ManageRecoil();
    }

    protected void ManageRecoil()
    {
        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
        else
        {
            UpdateEnemyStates();
        }
    }

    virtual public void EnemyHit(float damageDone, Vector2 _hitDirection, float hitForce)
    {
        health -= damageDone;
        if(!isRecoiling)
        {
            GameObject _orangeBlood = Instantiate(orangeBlood, transform.position, Quaternion.identity);
            Destroy(_orangeBlood, 3f);
            rb.linearVelocity = (hitForce * recoilFactor * _hitDirection);
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
            Vector2 _hitDirection = (transform.position - playerCollider.transform.position).normalized;
            
            // Direct reference to player instead of using the tag
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null && !playerController.pState.invincible && health > 0)
            {
                Attack();
                freezeDetector.HitManager(hitPoint, _hitDirection);
            }
        }
    }

    protected virtual void UpdateEnemyStates() { }

    protected virtual void ChangeCurrentAnimation() { }

    protected void ChangeState(EnemyStates newState)
    {
        GetCurrentEnemyState = newState;
    }
    protected virtual void Attack()
    {
        Vector2Int _hitDirection = new Vector2Int(
            (int)Mathf.Sign(PlayerController.Instance.transform.position.x - transform.position.x),
            (int)Mathf.Sign(PlayerController.Instance.transform.position.y - transform.position.y)
            );
        PlayerController.Instance.TakeDamage(damage, _hitDirection);
    }

    protected virtual void Death(float deathTime)
    {
            Destroy(gameObject, deathTime);
    }
}
