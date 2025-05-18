using UnityEngine;
using UnityEngine.UI;

public class HeartController : MonoBehaviour
{
    PlayerController player;

    private GameObject[] heartContainers;
    private Image[] heartFills;
    public Transform heartsParent;
    public GameObject heartContainerPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (heartsParent == null)
        {
            Debug.LogError("Hearts Parent non è stato assegnato! Assegnalo nell'Inspector.");
            heartsParent = transform; // Fallback temporaneo
        }
        
        // Verifica che il prefab sia assegnato
        if (heartContainerPrefab == null)
        {
            Debug.LogError("Heart Container Prefab non è stato assegnato! Assegnalo nell'Inspector.");
            return;
        }
        
        player = PlayerController.Instance;
        if (player == null)
        {
            Debug.LogError("PlayerController.Instance è null! Assicurati che il player sia presente nella scena.");
            return;
        }

        player = PlayerController.Instance;
        if (player == null)
        {
            Debug.LogError("Player non trovato");
        }
        heartContainers = new GameObject[PlayerController.Instance.maxHealth];
        heartFills = new Image[PlayerController.Instance.maxHealth];

        PlayerController.Instance.onHealthChangedCallBack += UpdateHeartsHUD;
        InstantiateHeartContainers();
        UpdateHeartsHUD();

        // Debug per verificare che i cuori siano stati creati
        Debug.Log($"Creati {heartContainers.Length} contenitori di cuori");
    }

    void SetHeartContainers()
    {
        for(int i = 0; i < heartContainers.Length; i++)
        {
            if(i < PlayerController.Instance.maxHealth)
            {
                heartContainers[i].SetActive(true);
            }
            else
            {
                heartContainers[i].SetActive(false);
            }
        }
    }

    void SetFilledHearts()
    {
        for(int i = 0; i < heartFills.Length; i++)
        {
            if(i < PlayerController.Instance.Health)
            {
                heartFills[i].fillAmount = 1;
            }
            else
            {
                heartFills[i].fillAmount = 0;
            }
        }
    }

    void InstantiateHeartContainers()
    {
        for(int i = 0; i < PlayerController.Instance.maxHealth; i++)
        {
            GameObject temp = Instantiate(heartContainerPrefab);
            temp.transform.SetParent(heartsParent, false);

            // Posizionamento esplicito
            RectTransform rectTransform = temp.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(i * 45, 0); // Spaziatura orizzontale
            
            heartContainers[i] = temp;

            // Verifica che il riferimento a HeartFill esista
            Transform heartFillTransform = temp.transform.Find("HeartFill");
            if (heartFillTransform != null)
            {
                heartFills[i] = heartFillTransform.GetComponent<Image>();
                if (heartFills[i] == null)
                {
                    Debug.LogError($"Impossibile trovare il componente Image in HeartFill nel cuore {i}");
                }
            }
            else
            {
                Debug.LogError($"Impossibile trovare HeartFill nel cuore {i}");
            }
        }
    }

    void UpdateHeartsHUD()
    {
        SetHeartContainers();
        SetFilledHearts();
        Debug.Log($"HUD aggiornato: {player.Health}/{player.maxHealth} cuori");
    }
}
