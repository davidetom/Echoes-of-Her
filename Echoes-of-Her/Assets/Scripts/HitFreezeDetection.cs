using UnityEngine;

public class HitFreezeDetection : MonoBehaviour
{
    [Header("Freeze Frame Settings")]
    [Tooltip("Durata esatta del freeze frame in secondi")]
    [SerializeField] private float freezeDuration = 0.05f;  // 1/20 di secondo
    
    [Header("Visual Feedback")]
    [Tooltip("Effetto particellare da istanziare al punto di impatto")]
    public GameObject hitParticlePrefab;
    
    [Header("Feedback aggiuntivi")]
    [Tooltip("Velocità di shake della camera durante l'impatto")]
    [Range(0, 1)]
    public float cameraShakeIntensity = 0.2f;
    
    [SerializeField]
    private AudioClip hitSoundEffect;
    
    public void HitManager(Vector2 hitPoint, Vector2 hitDirection)
    {
        // Applica il freeze frame con durata esatta di 1/20 di secondo
        FreezeFrameManager.Instance.DoFreezeFrame(freezeDuration);
        
        // Gestisci feedback visivi e audio
        SpawnHitEffects(hitPoint, hitDirection);
    }
    
    private void SpawnHitEffects(Vector2 position, Vector2 normal)
    {
        // Spawna effetti particellari
        if (hitParticlePrefab != null)
        {
            Quaternion rotation = Quaternion.LookRotation(normal);
            Instantiate(hitParticlePrefab, position, rotation);
        }
        
        // Riproduci suono di impatto
        if (hitSoundEffect != null)
        {
            AudioSource.PlayClipAtPoint(hitSoundEffect, position);
        }
        
        /* Camera shake (richiede un componente CameraShake sul camera object)
        if (cameraShakeIntensity > 0 && Camera.main != null)
        {
            CameraShake shaker = Camera.main.GetComponent<CameraShake>();
            if (shaker != null)
            {
                shaker.ShakeCamera(cameraShakeIntensity, 0.1f);
            }
        }
        */
    }
}
