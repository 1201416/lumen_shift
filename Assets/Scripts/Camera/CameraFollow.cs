using UnityEngine;

/// <summary>
/// Camera that follows the player like in Mario games
/// When player reaches the middle of the screen, camera keeps moving with them
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Player transform
    
    [Header("Follow Settings")]
    [Tooltip("Horizontal offset from center where camera starts following")]
    public float followThreshold = 0.3f; // 30% from center (like Mario)
    
    [Tooltip("How fast camera follows (0 = instant, higher = smoother)")]
    public float followSpeed = 5f;
    
    [Header("Camera Bounds (Optional)")]
    public bool useBounds = false;
    public float minX = -10f;
    public float maxX = 100f;
    public float minY = -5f;
    public float maxY = 20f;
    
    [Header("Camera Size")]
    [Tooltip("Orthographic size of the camera (bigger = see more)")]
    public float cameraSize = 10f;
    
    private Camera cam;
    private float lastTargetX;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
        
        // Set camera size
        if (cam != null)
        {
            cam.orthographicSize = cameraSize;
        }
        
        // Find player if not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        if (target != null)
        {
            lastTargetX = target.position.x;
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        float targetX = target.position.x;
        float targetY = target.position.y;
        
        // Get camera viewport bounds
        float cameraHalfWidth = cam.orthographicSize * cam.aspect;
        
        // Calculate desired camera X position
        float desiredX = targetX; // Default: keep player centered
        
        // Apply bounds if enabled
        if (useBounds)
        {
            // Try to center the player
            desiredX = targetX;
            
            // Calculate camera edges if we center on player
            float leftEdgeIfCentered = desiredX - cameraHalfWidth;
            float rightEdgeIfCentered = desiredX + cameraHalfWidth;
            
            // Check if centering would show empty space on left
            if (leftEdgeIfCentered < minX)
            {
                // Can't show left side - align left edge to minX
                desiredX = minX + cameraHalfWidth;
            }
            
            // Check if centering would show empty space on right
            if (rightEdgeIfCentered > maxX)
            {
                // Can't show right side - align right edge to maxX
                desiredX = maxX - cameraHalfWidth;
            }
            
            // Clamp to bounds (minX and maxX are camera center positions)
            desiredX = Mathf.Clamp(desiredX, minX, maxX);
            targetY = Mathf.Clamp(targetY, minY + cam.orthographicSize, maxY - cam.orthographicSize);
        }
        else
        {
            // Without bounds, keep player centered
            desiredX = targetX;
        }
        
        // Smoothly move camera
        Vector3 targetPos = new Vector3(desiredX, targetY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        
        lastTargetX = targetX;
    }
    
    /// <summary>
    /// Set the camera target (player)
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}

