using UnityEditor.SearchService;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
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

    public SceneFader sceneFader;

    private void Start()
    {
        sceneFader = GetComponentInChildren<SceneFader>();
    }

}
