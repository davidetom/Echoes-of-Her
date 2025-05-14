using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float followSpeed = 2.0f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -20);
    
    [Header("Smoothing")]
    [SerializeField] private bool useSmoothing = true;
    [SerializeField] private float smoothDampTime = 0.15f;
    private Vector3 velocity = Vector3.zero;
    
    [Header("Boundaries")]
    [SerializeField] private bool useBoundaries = false;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -10f;
    [SerializeField] private float maxY = 10f;
    
    private Transform target;
    private Vector3 targetPosition;

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            target = PlayerController.Instance.transform;
        }
        else
        {
            Debug.LogError("PlayerController.Instance is null. Make sure PlayerController is in the scene.");
        }
    }

    // LateUpdate è chiamato dopo tutti gli Update, ideale per la camera
    void LateUpdate()
    {
        if (target == null) return;
        
        // Calcola la posizione target con offset
        targetPosition = target.position + offset;
        
        // Applica limiti se necessario
        if (useBoundaries)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        }
        
        if (useSmoothing)
        {
            // SmoothDamp è migliore di Lerp per seguire oggetti in movimento
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                targetPosition, 
                ref velocity, 
                smoothDampTime
            );
        }
        else
        {
            // Interpolazione lineare per un movimento più diretto
            transform.position = Vector3.Lerp(
                transform.position, 
                targetPosition, 
                followSpeed * Time.deltaTime
            );
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (useBoundaries)
        {
            // Disegna i confini della camera
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector3(minX, minY, 0), new Vector3(maxX, minY, 0));
            Gizmos.DrawLine(new Vector3(maxX, minY, 0), new Vector3(maxX, maxY, 0));
            Gizmos.DrawLine(new Vector3(maxX, maxY, 0), new Vector3(minX, maxY, 0));
            Gizmos.DrawLine(new Vector3(minX, maxY, 0), new Vector3(minX, minY, 0));
        }
    }
}