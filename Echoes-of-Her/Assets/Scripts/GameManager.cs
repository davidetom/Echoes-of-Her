using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string transitionFromScene;

    public Vector2 platformingRespawnPoint;

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
        }
    }
}
