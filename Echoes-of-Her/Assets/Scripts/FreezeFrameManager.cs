using UnityEngine;
using System.Collections;

/// <summary>
/// Manager che implementa il freeze-frame (hitstop) nel gioco.
/// Blocca completamente il gioco per una breve durata quando viene attivato.
/// </summary>
public class FreezeFrameManager : MonoBehaviour
{
    public static FreezeFrameManager Instance;
    
    private bool isFrozen = false;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Attiva un freeze-frame per la durata specificata.
    /// </summary>
    /// <param name="duration">Durata del congelamento in secondi (default: 1/20 di secondo)</param>
    public void DoFreezeFrame(float duration = 0.05f)
    {
        if (!isFrozen)
        {
            StartCoroutine(FreezeTime(duration));
        }
    }
    
    private IEnumerator FreezeTime(float duration)
    {
        isFrozen = true;
        
        // Salva il timeScale corrente
        float originalTimeScale = Time.timeScale;
        
        // Blocca completamente il tempo di gioco
        Time.timeScale = 0f;
        
        // Attendi per la durata reale (non influenzata dal timeScale)
        float pauseEndTime = Time.realtimeSinceStartup + duration;
        while (Time.realtimeSinceStartup < pauseEndTime)
        {
            yield return null;
        }
        
        // Ripristina il timeScale normale
        Time.timeScale = originalTimeScale;
        
        isFrozen = false;
    }
}
