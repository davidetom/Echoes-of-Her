using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{

    [Header("Horizontal Movement Setting:")]
    [SerializeField] private float walkSpeed = 10;
    [Space(5)]

    [Header("Vertical Movement Settings:")]
    [SerializeField] private float jumpForce = 30; 
    private float jumpBufferCounter = 0;
    [SerializeField] private float jumpBufferTime = 0.2f; 
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime = 0.2f;
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps = 1;
    [Space(5)]

    [Header("Ground Check Settings:")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;
    [Space(5)]

    [Header("Dash Settings:")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashCoolDown = 1f;
    [SerializeField] GameObject dashEffect;
    private bool canDash = true;
    private bool dashed = false;
    private float dashTimeCounter = 0f;
    private bool isDashing = false;
    [Space(5)]

    [Header("Attack Settings:")]
    [SerializeField] private float timeBetweenAttacks = 0.5f;
    bool attack = false;
    float timeSinceAttack = 0f;
    [SerializeField] private Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] private Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] private LayerMask attackableLayer;
    [SerializeField] float damage = 2f;
    [SerializeField] private GameObject slashEffect;
    [Space(5)]

    [Header("Recoil Settings:")]
    [SerializeField] float recoilDuration = 0.2f;
    [SerializeField] float recoilXSpeed = 100;
    [SerializeField] float recoilYSpeed = 100;
    private float recoilTimerX = 0f;
    private float recoilTimerY = 0f;
    private float recoilDirectionX = 0f;
    private float recoilDirectionY = 0f;
    [Space(5)]

    [Header("Health Settings:")]
    [SerializeField] public int maxHealth = 5;
    public int health;
    [SerializeField] GameObject bloodSpurt;
    [SerializeField] float hitFlashSpeed;
    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate onHealthChangedCallBack;
    float healTimer;
    [SerializeField] float timeToHeal;
    [Space(5)]
    
    [Header("Mana Settings:")]
    [SerializeField] UnityEngine.UI.Image manaStorage;
    [SerializeField] float mana;
    [SerializeField] float manaDrainSpeed;
    [SerializeField] float manaGain;
    public bool halfMana = false;
    [Space(5)]

    [Header("Spell Casting Settings:")]
    [SerializeField] float manaSpellCost = 0.33f;
    [SerializeField] float timeBetweenCast = 0.5f;
    float timeSinceCast;
    private float castOrHealTimer;
    [SerializeField] float castHoldThreshold = 0.15f;
    private bool isButtonHeld = false;
    [SerializeField] float spellDamage;
    [SerializeField] float downSpellForce;
    [SerializeField] GameObject sideSpellFireball;
    [SerializeField] GameObject upSpellExplosion;
    [SerializeField] GameObject downSpellFireball;
    [SerializeField] private float spellInvincibilityDuration = 0.5f; // Nuovo parametro per la durata dell'invincibilità
    [SerializeField] private float downSpellMaxVelocity = 15f; // Velocità massima per l'incantesimo verso il basso
    private bool isDownSpellActive = false; // Flag per tracciare se l'incantesimo verso il basso è attivo

    [Space(5)]

    [HideInInspector] public PlayerStateList pState;
    private HitFreezeDetection freezeDetector;
    private Animator anim;
    public Rigidbody2D rb;
    private SpriteRenderer sr;
    private float xAxis, yAxis;
    public Vector3 lastPosition;
    public bool lastFacingWasRight;
    bool openMap;
    private float gravity;

    // Aggiungiamo una lista per tenere traccia dei collider dei nemici che stiamo ignorando
    private List<Collider2D> ignoredEnemyColliders = new List<Collider2D>();
    private Collider2D playerCollider;
    
    public static PlayerController Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);

        // Inizializzazione playerStateList anticipata
        pState = GetComponent<PlayerStateList>();
        if (pState == null)
        {
            Debug.LogWarning("PlayerStateList non trovato, verrà aggiunto automaticamente");
            pState = gameObject.AddComponent<PlayerStateList>();
        }

        // Ottieni il collider del giocatore
        playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            Debug.LogError("Collider2D non trovato sul Player!");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pState = GetComponent<PlayerStateList>();
        if (pState == null)
        {
            Debug.LogError("PlayerStateList non trovato sul GameObject!");
            pState = gameObject.AddComponent<PlayerStateList>();
        }

        freezeDetector = GetComponent<HitFreezeDetection>();
        if (freezeDetector == null)
        {
            Debug.LogError("HitFreezeDetection non trovato sul GameObject!");
            freezeDetector = gameObject.AddComponent<HitFreezeDetection>();
        }

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D non trovato sul GameObject!");
        }

        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("SpriteRenderer non trovato sul GameObject!");
        }

        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogWarning("Animator non trovato sul GameObject!");
        }

        gravity = rb.gravityScale;

        Mana = mana;
        manaStorage.fillAmount = Mana;

        Health = maxHealth;

        lastPosition = transform.position;
        lastFacingWasRight = true;
    }

    //Per visualizzare le hitbox degli attacchi
    private void OnDrawGizmos()
    {
        //Per visualizzare le hitbox degli attacchi
        Gizmos.color = Color.red;
        if (SideAttackTransform != null) Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        if (UpAttackTransform != null) Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        if (DownAttackTransform != null) Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);

        // Visualizza i raycast di ground check
        Gizmos.color = Color.green;
        if (groundCheckPoint != null)
        {
            Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position + Vector3.down * groundCheckY);
            Gizmos.DrawLine(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), 
                           groundCheckPoint.position + new Vector3(groundCheckX, 0, 0) + Vector3.down * groundCheckY);
            Gizmos.DrawLine(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), 
                           groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0) + Vector3.down * groundCheckY);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (pState.cutscene) return;

        if (pState.alive)
        {
            GetInputs();
            ToggleMap();

            UpdateJumpVariables();
            ManageDashState();

            UpdateLastGroundPosition();

            if (pState.dashing) return;

            Flip();
            Move();
            Jump();
            StartDash();
            Attack();
            FlashWhileInvincible();
            Heal();
            CastSpell();
        }
    }

    private void UpdateLastGroundPosition()
    {
        if (Grounded())
        {
            lastPosition = transform.position;
            lastFacingWasRight = pState.lookingRight ? true : false;
        }
    }

    //Per incantesimi verso l'alto e il basso
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Enemy>() != null && pState.casting)
        {
            other.GetComponent<Enemy>().EnemyHit(spellDamage, (other.transform.position - transform.position).normalized, -recoilYSpeed);
        }
    }

    private void FixedUpdate()
    {
        if (pState.cutscene) return;
        
        if (pState.dashing) return;
        Recoil();
    }

    //Ottenimento input da tastiera e mouse
    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetButtonDown("Attack");
        openMap = Input.GetButton("Map");

        if (Input.GetButtonDown("Cast/Heal"))
        {
            isButtonHeld = false;
        }

        if (Input.GetButton("Cast/Heal"))
        {
            castOrHealTimer += Time.deltaTime;
            if (castOrHealTimer >= castHoldThreshold && !isButtonHeld)
            {
                isButtonHeld = true;
            }
        }
        else
        {
            castOrHealTimer = 0;
        }

        if (Input.GetButtonDown("Reset"))
        {
            ResetSalvataggio();
        }
    }

    private void ResetSalvataggio()
    {
        SaveData.Instance.ResetToDefault();
        Debug.Log("Dati cancellati");
    }

    private void ToggleMap()
    {
        if (openMap)
        {
            UIManager.Instance.mapHandler.SetActive(true);
        }
        else
        {
            UIManager.Instance.mapHandler.SetActive(false);
        }
    }

    //Gestione della rotazione della sprite 
    void Flip()
    {
        if(xAxis < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
            pState.lookingRight = false;
        }
        else if(xAxis > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
            pState.lookingRight = true;
        }
    }

    //Gestione movimento
    private void Move()
    {
        //Il giocatore può muoversi solo se non sta subendo rinculo
        if(!pState.recoilingX)
        {
            rb.linearVelocity = new Vector2(walkSpeed * xAxis, rb.linearVelocity.y);
        }
        if(anim != null)
        {
            anim.SetBool("Walking", rb.linearVelocity.x != 0 && Grounded());
        }
    }

    //GESTIONE LOGICA DEL DASH:
    //Gestione inizio e fine del dash 
    void ManageDashState()
    {
        if (isDashing)
        {
            //Incremento durata attuale del dash
            dashTimeCounter += Time.deltaTime;
            
            // Manteniamo la velocità orizzontale costante e verticale a zero durante il dash
            rb.linearVelocity = new Vector2(transform.localScale.x * dashSpeed, 0);
            
            // Fine del dash dopo il tempo stabilito
            if (dashTimeCounter >= dashTime)
            {
                isDashing = false;
                rb.gravityScale = gravity;
                pState.dashing = false;
                dashTimeCounter = 0f;
                
                // Avvia il cooldown
                StartCoroutine(DashCooldown());
            }
        }
    }

    //Attivatore del dash
    void StartDash()
    {
        if(Input.GetButtonDown("Dash") && canDash && !dashed)
        {
            Dash();
            dashed = true;
        }

        if(Grounded())
        {
            dashed = false;
        }
    }

    //Gestione dash
    void Dash()
    {
        canDash = false;
        isDashing= true;
        pState.dashing = true;
        dashTimeCounter = 0f;

        if(anim != null)
        {
            anim.SetTrigger("Dashing");
        }

        //Durante il dash la gravità non ha effetto
        rb.gravityScale = 0;
        int dir = pState.lookingRight ? 1 : -1;
        rb.linearVelocity = new Vector2(dir * dashSpeed, 0);
        
        //Effetto del dash
        if(Grounded() && dashEffect != null) 
        {
            GameObject effect = Instantiate(dashEffect, transform);
            //effect.transform.localScale = new Vector3(transform.localScale.x, 1, 1);
        }
    }

    //Cooldown prima del prossimo dash
    IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCoolDown);
        canDash = true;
    }

    //Gestion del cambio scena
    public IEnumerator WalkIntoNewScene(Vector2 exitDir, float delay)
    {
        pState.invincible = true;
        //Se la direzione di uscia è verso l'alto
        if (exitDir.y > 0)
        {
            rb.linearVelocity = jumpForce * exitDir;
        }

        //Se la direzione di uscita richiede movimento orizzontale
        if (exitDir.x != 0)
        {
            xAxis = exitDir.x > 0 ? 1 : -1;

            Move();
        }

        Flip();
        yield return new WaitForSeconds(delay);
        pState.invincible = false;
        pState.cutscene = false;
    }

    //GESTIONE LOGICA DELL'ATTACCO
    //Funzione d'attacco principale
    void Attack()
    {
        timeSinceAttack += Time.deltaTime;

        //Se è passato abbastanza tempo dall'attacco precedente
        if(attack && timeSinceAttack >= timeBetweenAttacks)
        {
            timeSinceAttack = 0;

            if(anim != null)
            {
                anim.SetTrigger("Attacking");
            }

            //Attacco laterale
            if(yAxis == 0 || (yAxis < 0 && Grounded()))
            {
                if(SideAttackTransform != null)
                {
                    int recoilLeftOrRight = pState.lookingRight ? 1 : -1;
                    Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, Vector2.right * recoilLeftOrRight, recoilXSpeed);

                    if(slashEffect != null)
                    {
                        SlashEffectAtAngle(slashEffect, 0, SideAttackTransform);
                    }
                }
            }
            //Attacco verso l'alto
            else if(yAxis > 0)
            {
                if(UpAttackTransform != null)
                {
                    Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, Vector2.up, recoilYSpeed);
                    
                    if(slashEffect != null)
                    {
                        if(pState.lookingRight)
                        {
                            SlashEffectAtAngle(slashEffect, 80, UpAttackTransform);
                        }
                        else
                        {
                            SlashEffectAtAngle(slashEffect, -80, UpAttackTransform);
                        }
                    }
                }
            }
            //Attacco verso il basso
            else if(yAxis < 0 && !Grounded())
            {
                if(DownAttackTransform != null)
                {
                    Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, Vector2.down, recoilYSpeed);
                    if(slashEffect != null)
                    {
                        if(pState.lookingRight)
                        {
                            SlashEffectAtAngle(slashEffect, -90, DownAttackTransform);
                        }
                        else
                        {
                            SlashEffectAtAngle(slashEffect, 90, DownAttackTransform);
                        }
                    }
                }
            }
        }
    }

    //Gestione del colpo
    private void Hit(Transform attackTransform, Vector2 attackArea, ref bool recoilBool, Vector2 recoilDir, float recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(attackTransform.position, attackArea, 0, attackableLayer);
        List<Enemy> hitEnemies = new List<Enemy>();
        
        //Se il giocatore colpisce almeno un oggetto attaccabile...
        if(objectsToHit.Length > 0)
        {
            //...va attivato il rinculo
            recoilBool = true;

            // Reset dei timer di rinculo quando colpiamo qualcosa
            if (recoilBool == pState.recoilingX)
            {
                recoilTimerX = 0f;
                recoilDirectionX = -transform.localScale.x;
            }
            else if (recoilBool == pState.recoilingY)
            {
                recoilTimerY = 0f;
                recoilDirectionY = yAxis < 0 ? 1f : -1f;
            }
        }
        bool enemyHit = false;
        Vector2 hitDirection = new Vector2(0, 0);
        Vector2 hitPoint = new Vector2(0, 0);
        //Gestione dei danni ai nemici
        for(int i = 0; i < objectsToHit.Length; i++)
        {
            Enemy enemy = objectsToHit[i].GetComponent<Enemy>();
            if(enemy != null && !hitEnemies.Contains(enemy))
            {
                enemyHit = true;
                Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
                hitPoint = enemyCollider.ClosestPoint(transform.position);
                hitDirection = (transform.position - objectsToHit[i].transform.position).normalized;

                enemy.EnemyHit(damage, recoilDir, recoilStrength);
                hitEnemies.Add(enemy);

                Mana += manaGain;
            }
        }

        if(enemyHit)
        {
            freezeDetector.HitManager(hitPoint,hitDirection);
        }
    }

    //Gestione dell'effetto del colpo (angolazione)
    void SlashEffectAtAngle(GameObject effect, int effectAngle, Transform attackTransform)
    {
        if(effect == null || attackTransform == null) return;
        
        GameObject slashEffect = Instantiate(effect, attackTransform.position, Quaternion.identity);
        slashEffect.transform.eulerAngles = new Vector3(0, 0, effectAngle);
        slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
    }

    //GESTIONE LOGICA DEL RINCULO
    void Recoil()
    {
        // Rinculo orizzontale
        if(pState.recoilingX)
        {
            recoilTimerX += Time.deltaTime;
            float recoilDirection;
            //Rinculo causato da nemico
            if(pState.recoilingFromHitHorizontal != 0)
            {
                recoilDirection = pState.recoilingFromHitHorizontal;
            }
            //Rinculo causato da attacco
            else
            {
                recoilDirection = recoilDirectionX;
            }
            rb.linearVelocity = new Vector2(recoilDirection * recoilXSpeed, rb.linearVelocity.y);
            
            //Interruzione rinculo
            if (recoilTimerX >= recoilDuration)
            {
                StopRecoilX();
            }
        }
        
        // Rinculo verticale
        if(pState.recoilingY)
        {
            recoilTimerY += Time.deltaTime;
            rb.gravityScale = 0;
            float recoilDirection;
            //Rinculo causato da nemico
            if(pState.recoilingFromHitVertical != 0)
            {
                recoilDirection = pState.recoilingFromHitVertical;
            }
            //Rinculo causato da attacco
            else
            {
                recoilDirection = recoilDirectionY;
            }
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, recoilDirection * recoilYSpeed);
            airJumpCounter = 0;

            //Interruzione rinculo
            if (recoilTimerY >= recoilDuration)
            {
                StopRecoilY();
            }
        }
        else if(!pState.dashing)
        {
            rb.gravityScale = gravity;
        }

        if(Grounded() && pState.recoilingY)
        {
            StopRecoilY();
        }
    }

    //Gestione rinculo da colpo subito
    void RecoilFromHit(Vector2Int hitDirection)
    {
        //Controllo che il nemico non sia esattamente dentro il player 
        if(hitDirection.x != 0 || hitDirection.y != 0)
        {
            pState.recoilingFromHitHorizontal = hitDirection.x;
            if(!Grounded())
            {
                pState.recoilingFromHitVertical = hitDirection.y;
            }
            //Componente rinculo orizzontale
            if(hitDirection.x != 0)
            {
                pState.recoilingX = true;
            }
            //Componente rinculo verticale
            if(hitDirection.y != 0)
            {
                pState.recoilingY = true;
            }

            Recoil();
        }
    }

    //Gestione interruzione del rinculo
    void StopRecoilX()
    {
        recoilTimerX = 0f;
        pState.recoilingX = false;
        if(pState.recoilingFromHitHorizontal != 0)
        {
            pState.recoilingFromHitHorizontal = 0;
        }
    }

    //Gestione interruzione del rinculo
    void StopRecoilY()
    {
        recoilTimerY = 0f;
        pState.recoilingY = false;
        if(pState.recoilingFromHitVertical != 0)
        {
            pState.recoilingFromHitVertical = 0;
        }
    }

    //GESTIONE LOGICA DEL DANNO
    //Gestione danno
    public void TakeDamage(float damage, Vector2Int hitDirection)
    {
        if (!pState.alive || pState.invincible || pState.cutscene) return;
        pState.beenHit = true;
        Health -= Mathf.RoundToInt(damage);
        if (Health == 0)
        {
            StartCoroutine(Death());
        }
        else
        {
            RecoilFromHit(hitDirection);
            StartCoroutine(StopTakingDamage());
        }
    }

    //Gestione i-frames
    IEnumerator StopTakingDamage()
    {
        pState.invincible = true;

        // Crea l'effetto sangue
        if (bloodSpurt != null)
        {
            GameObject bloodSpurtParticles = Instantiate(bloodSpurt, transform.position, Quaternion.identity);
            bloodSpurtParticles.SetActive(true);
            Destroy(bloodSpurtParticles, 1.5f);
        }

        // Triggera l'animazione di danno
        if (anim != null)
        {
            anim.SetTrigger("TakeDamage");
        }

        // Attiva l'attraversamento nemici DOPO aver gestito l'effetto sangue
        EnableEnemyPhasing(true);

        yield return new WaitForSeconds(1f);

        if (pState.alive)
        {
            EnableEnemyPhasing(false);
            pState.invincible = false;
            pState.beenHit = false;
        }
    }

    void FlashWhileInvincible()
    {
        if (pState.beenHit && pState.alive)
        {
            sr.material.color = Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time * hitFlashSpeed, 1.0f));
        }
        else
        {
            sr.material.color = Color.white;
        }
}

    //Gestione della salute
    public int Health
    {
        get {return health; }
        set
        {
            if(health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);

                if(onHealthChangedCallBack != null)
                {
                    onHealthChangedCallBack.Invoke();
                }
            }
        }
    } 

    //Gestione della cura
    void Heal()
    {
        if(Input.GetButton("Cast/Heal") && isButtonHeld && Health < maxHealth && Mana > 0 && !pState.jumping && !pState.dashing)
        {
            pState.healing = true;
            anim.SetBool("Healing", true);

            //healing
            healTimer += Time.deltaTime;
            if(healTimer >= timeToHeal)
            {
                Health++;
                healTimer = 0;
            }

            Mana -= Time.deltaTime * manaDrainSpeed;
        }
        else
        {
            pState.healing = false;
            anim.SetBool("Healing", false);
            healTimer = 0;
        }
    }

    IEnumerator Death()
    {
        pState.alive = false;
        pState.invincible = true;
        pState.beenHit = false;

        Time.timeScale = 1f;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = gravity;

        anim.SetBool("Walking", false);
        anim.SetBool("Jumping", false);
        anim.SetBool("Dashing", false);
        anim.SetBool("Healing", false);
        anim.SetBool("Casting", false);
        anim.SetTrigger("Death");

        pState.dashing = false;
        pState.jumping = false;
        pState.healing = false;
        pState.casting = false;
        pState.recoilingX = false;
        pState.recoilingY = false;

        EnableEnemyPhasing(false);

        yield return new WaitForSeconds(0.9f);

        if (!pState.cutscene)
        {
            Respawn();
            anim.Play("player_idle");
        }
    }

    private void Respawn()
    {
        StartCoroutine(GameManager.Instance.Respawn());
    }

    // Nuovo metodo per resettare tutti gli stati (morte completa)
    public void ResetAllStates()
    {
        // Reset physics
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = gravity;

        // Reset movement states
        pState.dashing = false;
        pState.jumping = false;
        pState.healing = false;
        pState.casting = false;
        pState.recoilingX = false;
        pState.recoilingY = false;
        pState.recoilingFromHitHorizontal = 0;
        pState.recoilingFromHitVertical = 0;

        // Reset timers
        dashTimeCounter = 0f;
        recoilTimerX = 0f;
        recoilTimerY = 0f;
        airJumpCounter = 0;

        // Reset dash
        isDashing = false;
        canDash = true;
        dashed = false;

        // Reset spell states
        if (isDownSpellActive)
        {
            downSpellFireball.SetActive(false);
            isDownSpellActive = false;
        }

        // Reset animazioni
        if (anim != null)
        {
            anim.SetBool("Walking", false);
            anim.SetBool("Jumping", false);
            anim.SetBool("Dashing", false);
            anim.SetBool("Healing", false);
            anim.SetBool("Casting", false);
        }

        // Assicurati che l'attraversamento nemici sia disattivato
        EnableEnemyPhasing(false);
    }
    
    //Gestione del mana
    public float Mana
    {
        get { return mana; }
        set
        {
            if (mana != value)
            {
                if (!halfMana)
                {
                    mana = Mathf.Clamp(value, 0, 1);
                }
                else
                {
                    mana = Mathf.Clamp(value, 0, 0.5f);
                }

                manaStorage.fillAmount = mana;
            }
        }
    }

    //Gestione incantesimi
    void CastSpell()
    {
        if (Input.GetButtonUp("Cast/Heal") && !isButtonHeld && timeSinceCast >= timeBetweenCast && Mana >= manaSpellCost)
        {
            pState.casting = true;
            timeSinceCast = 0;
            StartCoroutine(CastCoroutine());
        }
        else
        {
            timeSinceCast += Time.deltaTime;
        }

        if (Grounded() && isDownSpellActive)
        {
            downSpellFireball.SetActive(false);
            isDownSpellActive = false;
        }

        if (isDownSpellActive && downSpellFireball.activeInHierarchy)
        {
            rb.AddForce(downSpellForce * Vector2.down);
            if (rb.linearVelocity.y < -downSpellMaxVelocity)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -downSpellMaxVelocity);
            }
        }
    }

    IEnumerator CastCoroutine()
    {
        anim.SetBool("Casting", true);

        pState.invincible = true;
        EnableEnemyPhasing(true);

        yield return new WaitForSeconds(0.15f);

        //Incantesimo laterale
        if (yAxis == 0 || (yAxis < 0 && Grounded()))
        {
            GameObject fireball = Instantiate(sideSpellFireball, SideAttackTransform.position, Quaternion.identity);
            if (pState.lookingRight)
            {
                fireball.transform.eulerAngles = Vector3.zero;
            }
            else
            {
                fireball.transform.eulerAngles = new Vector2(fireball.transform.eulerAngles.x, 180);
            }
            pState.recoilingX = true;
        }

        //Incantesimo verso l'alto
        else if (yAxis > 0)
        {
            Instantiate(upSpellExplosion, transform);
            rb.linearVelocity = Vector2.zero;
        }

        //Incantesimo verso il basso
        else if (yAxis < 0 && !Grounded())
        {
            downSpellFireball.SetActive(true);
            isDownSpellActive = true;
        }

        Mana -= manaSpellCost;

        yield return new WaitForSeconds(0.35f);
        anim.SetBool("Casting", false);

        yield return new WaitForSeconds(spellInvincibilityDuration);

        EnableEnemyPhasing(false);
        pState.invincible = false;
        pState.casting = false;
    }

    // Durante l'incantesimo verso il basso, assicura che le collisioni con il terreno vengano rilevate
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDownSpellActive && (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || 
            (whatIsGround == (whatIsGround | (1 << collision.gameObject.layer)))))
        {
            downSpellFireball.SetActive(false);
            isDownSpellActive = false;
        }
    }

    // Nuovo metodo per gestire l'attraversamento dei nemici
    void EnableEnemyPhasing(bool enable)
    {
        if (playerCollider == null) return;
        
        // Trova tutti i nemici nella scena
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        
        // Verifica che abbiamo trovato nemici
        if (enemies == null || enemies.Length == 0)
        {
            Debug.Log("Nessun nemico trovato nella scena");
            return;
        }
        
        foreach (Enemy enemy in enemies)
        {
            if(enemy == null) continue;

            Collider2D[] enemyColliders = enemy.GetComponents<Collider2D>();
            
            if (enemyColliders == null || enemyColliders.Length == 0) continue;

            foreach (Collider2D enemyCollider in enemyColliders)
            {
                if (enable)
                {
                    // Ignora collisioni con questo nemico
                    Physics2D.IgnoreCollision(playerCollider, enemyCollider, true);
                    
                    // Aggiungi alla lista solo se non c'è già
                    if (!ignoredEnemyColliders.Contains(enemyCollider))
                    {
                        ignoredEnemyColliders.Add(enemyCollider);
                    }
                }
                else
                {
                    // Ripristina collisioni con questo nemico
                    Physics2D.IgnoreCollision(playerCollider, enemyCollider, false);
                }
            }
        }
        
        // Se disabilitiamo l'attraversamento, puliamo la lista
        if (!enable)
        {
            ignoredEnemyColliders.Clear();
        }
    }
    
    // Metodo per gestire nuovi nemici che potrebbero apparire durante l'invincibilità
    void OnEnable()
    {
        Enemy.OnEnemySpawned -= HandleNewEnemySpawned;
        Enemy.OnEnemySpawned += HandleNewEnemySpawned;
    }
    
    void OnDisable()
    {
        Enemy.OnEnemySpawned -= HandleNewEnemySpawned;

        // Pulizia delle referenze ai collider ignorati
        if (ignoredEnemyColliders != null)
        {
            ignoredEnemyColliders.Clear();
        }
    }
    
    void HandleNewEnemySpawned(Enemy newEnemy)
    {
        // Controlla sia che siamo in stato di invincibilità sia che playerCollider non sia null
        if (pState != null && pState.invincible && playerCollider != null && newEnemy != null)
        {
            Collider2D[] enemyColliders = newEnemy.GetComponents<Collider2D>();
            if (enemyColliders != null)
            {
                foreach (Collider2D enemyCollider in enemyColliders)
                {
                    if (enemyCollider != null)
                    {
                        Physics2D.IgnoreCollision(playerCollider, enemyCollider, true);
                        ignoredEnemyColliders.Add(enemyCollider);
                    }
                }
            }
        }
    }

    //Gestione controllo del terreno
    public bool Grounded()
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
        Debug.DrawLine(boxCenter + new Vector2(-boxSize.x / 2, 0),
                       boxCenter + new Vector2(boxSize.x / 2, 0),
                       groundCollider ? Color.green : Color.red);

        return groundCollider != null;
    }

    // Metodo per visualizzare il box di groundCheck
    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Vector2 boxCenter = groundCheckPoint.position;
            Vector2 boxSize = new Vector2(groundCheckX * 2, groundCheckY * 0.5f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(
                boxCenter + Vector2.down * groundCheckY * 0.75f,
                boxSize
            );
        }
    }

    //GESTIONE LOGICA DEL SALTO
    //Gestione del salto
    void Jump()
    {
        // Gestione del salto iniziale (da terra)
        if(jumpBufferCounter > 0 && coyoteTimeCounter > 0 && !pState.jumping)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Azzera velocità Y prima del salto
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            pState.jumping = true;
            airJumpCounter = 0;
            jumpBufferCounter = 0; // Reset jump buffer dopo il salto
            coyoteTimeCounter = 0; // Reset anche del coyote time
        }
        //Gestione del salto aereo
        else if(!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
        {
            // Anche per il salto aereo, azzera la velocità Y prima di saltare
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            pState.jumping = true;
            airJumpCounter++; //Incremento il contatore dei salti aerei
        }

        //Gestione del salto regolabile
        if(Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            // Limita la velocità a un valore minimo per evitare salti troppo corti
            float minimumJumpVelocity = jumpForce * 0.4f;
            if (rb.linearVelocity.y > minimumJumpVelocity)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, minimumJumpVelocity);
            }
            pState.jumping = false;
        }

        if (anim != null)
        {
            anim.SetBool("Jumping", !Grounded());
        }
    }

    //Gestione degli aggiornamenti delle variabili di salto
    void UpdateJumpVariables()
    {
        //Se il giocatore è a terra vengono resettati il coyote time 
        // e il contatore di salti aerei
        if(Grounded())
        {
           pState.jumping = false;
           coyoteTimeCounter = coyoteTime;
           airJumpCounter = 0;
        }
        //Altrimenti viene decrementato il coyote time
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        //Se si vuole saltare viene azionato il jump buffer
        if(Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        //Altrimenti viene decrementato il jump buffer
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }
}
