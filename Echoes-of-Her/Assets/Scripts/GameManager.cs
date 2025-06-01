using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string transitionFromScene;

    public Vector2 platformingRespawnPoint;
    public Vector2 respawnPoint;
    [SerializeField] private Bench bench;

    [Header("DeathStone Settings:")]
    [SerializeField] private GameObject deathStonePrefab; // Assegna il prefab DeathStone nell'Inspector
    private GameObject currentDeathStone; // Riferimento alla DeathStone attualmente presente
    private bool airDeath;

    public static GameManager Instance { get; private set; }

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
    }

    private void Start()
    {
        // Inizializza il respawn point con la posizione iniziale del player
        if (PlayerController.Instance != null)
        {
            platformingRespawnPoint = PlayerController.Instance.transform.position;
            respawnPoint = PlayerController.Instance.transform.position;
        }

        airDeath = false;
    }

    public IEnumerator Respawn()
    {
        PlayerController player = PlayerController.Instance;
        if (player.pState.cutscene) yield break;

        player.pState.cutscene = true;
        player.pState.invincible = true;
        player.pState.beenHit = false;
        player.rb.gravityScale = 0f;

        airDeath = player.Grounded() ? false : true;

        bool isDeathRespawn = !player.pState.alive;

        // DEBUG: Aggiungi log per capire il tipo di respawn
        Debug.Log($"Respawn chiamato - isDeathRespawn: {isDeathRespawn}, player.pState.alive: {player.pState.alive}");
        Debug.Log($"LastPosition: {player.lastPosition}, LastFacingWasRight: {player.lastFacingWasRight}");

        Time.timeScale = 1f;

        StartCoroutine(UIManager.Instance.sceneFader.Fade(SceneFader.FadeDirection.In));
        yield return new WaitForSecondsRealtime(0.8f);

        // Se il respawn è dovuto alla morte, ripristina la salute al massimo
        if (isDeathRespawn)
        {
            Debug.Log("Handling death respawn - creating DeathStone");
            HandleDeathStone(player.lastPosition, player.lastFacingWasRight);

            player.transform.position = GameManager.Instance.respawnPoint;
            player.Health = player.maxHealth;
            player.halfMana = true;
            UIManager.Instance.SwitchMana(UIManager.ManaState.HalfMana);
            player.Mana = 0f;
        }
        else
        {
            Debug.Log("Handling platforming respawn - no DeathStone");
            player.transform.position = GameManager.Instance.platformingRespawnPoint;
        }

        player.pState.alive = true;
        player.ResetAllStates();
        StartCoroutine(UIManager.Instance.sceneFader.Fade(SceneFader.FadeDirection.Out));
        yield return new WaitForSecondsRealtime(UIManager.Instance.sceneFader.fadeTime);

        player.pState.cutscene = false;

        yield return new WaitForSeconds(0.5f);
        player.pState.invincible = false;
        airDeath = false;
    }

    private void HandleDeathStone(Vector3 deathPosition, bool lastFacingWasRight)
    {
        Debug.Log($"HandleDeathStone chiamato - Position: {deathPosition}, FacingRight: {lastFacingWasRight}");

        // Rimuovi la DeathStone precedente se esiste ancora
        if (currentDeathStone != null)
        {
            Debug.Log("Distruggendo DeathStone precedente");
            Destroy(currentDeathStone);
            currentDeathStone = null;
        }

        // Crea una nuova DeathStone nella posizione di morte
        if (deathStonePrefab != null)
        {
            Debug.Log("Creando nuova DeathStone");
            currentDeathStone = Instantiate(deathStonePrefab, deathPosition, Quaternion.identity);

            currentDeathStone.transform.position += Vector3.down * 0.5f;

            if (airDeath)
            {
                if (lastFacingWasRight)
                {
                    currentDeathStone.transform.position += Vector3.left * 0.8f;
                }
                else
                {
                    currentDeathStone.transform.position += Vector3.right * 0.8f;
                }
            }
            
            Debug.Log($"DeathStone creata alla posizione: {deathPosition}");
        }
        else
        {
            Debug.LogWarning("DeathStone prefab non è stato assegnato nel GameManager!");
        }
    }

    public void RemoveCurrentDeathStone()
    {
        if (currentDeathStone != null)
        {
            Destroy(currentDeathStone);
            currentDeathStone = null;
        }
    }

    public bool HasActiveDeathStone()
    {
        return currentDeathStone != null;
    }
}
