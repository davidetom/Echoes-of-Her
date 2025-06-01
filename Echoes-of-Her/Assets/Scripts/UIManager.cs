using UnityEditor.SearchService;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public SceneFader sceneFader;

    public static UIManager Instance;
    [SerializeField] public GameObject mapHandler;
    [SerializeField] private GameObject halfMana, fullMana;

    public enum ManaState
    {
        FullMana,
        HalfMana
    }

    public ManaState manaState;

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
        sceneFader = GetComponentInChildren<SceneFader>();
    }

    public void SwitchMana(ManaState _manaState)
    {
        switch (_manaState)
        {
            case ManaState.FullMana:
                halfMana.SetActive(false);
                fullMana.SetActive(true);
                break;

            case ManaState.HalfMana:
                halfMana.SetActive(true);
                fullMana.SetActive(false);
                break;
        }

        manaState = _manaState;
    }
}
