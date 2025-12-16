using UnityEngine;
#if UNITY_URP
using UnityEngine.Rendering.Universal;
#endif

/// <summary>
/// Camera URP Fix - Simple fix for view frustum errors
/// Based on indie game best practices: minimal configuration, avoid complex checks
/// </summary>
[RequireComponent(typeof(Camera))]
[DefaultExecutionOrder(-100)]
public class CameraURPFix : MonoBehaviour
{
    private Camera cam;
    
    void Awake()
    {
        cam = GetComponent<Camera>();
        FixCamera();
    }
    
    void Start()
    {
        if (cam == null) cam = GetComponent<Camera>();
        FixCamera();
    }
    
    void FixCamera()
    {
        if (cam == null) return;
        
        // Simple fix: ensure rect is normalized (not pixels)
        if (cam.rect.width > 1f || cam.rect.height > 1f)
        {
            cam.rect = new Rect(0f, 0f, 1f, 1f);
        }
        
        // Basic camera setup for 2D
        cam.orthographic = true;
        if (cam.orthographicSize <= 0f)
        {
            cam.orthographicSize = 5f;
        }
        if (cam.nearClipPlane <= 0f)
        {
            cam.nearClipPlane = 0.3f;
        }
        cam.clearFlags = CameraClearFlags.SolidColor;
        if (cam.backgroundColor == Color.black)
        {
            cam.backgroundColor = new Color(0.7f, 0.85f, 1f);
        }
    }
}
