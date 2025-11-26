using UnityEngine;
using System.Collections;

/// <summary>
/// Controls background color based on day/night cycle
/// Changes camera background color instead of block colors
/// Smoothly transitions between day and night through blues
/// </summary>
public class BackgroundController : MonoBehaviour
{
    [Header("Background Colors")]
    public Color dayBackgroundColor = new Color(0.5f, 0.7f, 1f); // Light blue sky
    public Color nightBackgroundColor = new Color(0.05f, 0.05f, 0.15f); // Dark blue/night sky
    
    [Header("Transition Settings")]
    [Tooltip("Duration of the smooth transition between day and night")]
    public float transitionDuration = 1.5f;
    
    [Header("References")]
    public Camera targetCamera;
    public GameManager gameManager;
    
    private Color currentColor;
    private Color targetColor;
    private Coroutine transitionCoroutine;
    
    private void Start()
    {
        // Find camera if not assigned
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        
        // Find GameManager if not assigned
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
        
        // Initialize current color
        if (targetCamera != null)
        {
            currentColor = targetCamera.backgroundColor;
        }
        
        // Subscribe to day/night changes
        if (gameManager != null)
        {
            gameManager.OnTimeOfDayChanged += UpdateBackground;
            // Set initial background color
            UpdateBackground(gameManager.isDayTime);
        }
    }
    
    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnTimeOfDayChanged -= UpdateBackground;
        }
        
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
    }
    
    /// <summary>
    /// Update background color based on time of day with smooth transition
    /// </summary>
    public void UpdateBackground(bool isDay)
    {
        if (targetCamera == null) return;
        
        // Stop any existing transition
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        
        // Set target color
        targetColor = isDay ? dayBackgroundColor : nightBackgroundColor;
        
        // Start smooth transition
        transitionCoroutine = StartCoroutine(SmoothTransition());
    }
    
    /// <summary>
    /// Smoothly transition background color through blues
    /// </summary>
    IEnumerator SmoothTransition()
    {
        float elapsedTime = 0f;
        Color startColor = currentColor;
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            
            // Smooth interpolation (ease in-out)
            t = t * t * (3f - 2f * t);
            
            // Lerp through blues
            currentColor = Color.Lerp(startColor, targetColor, t);
            
            if (targetCamera != null)
            {
                targetCamera.backgroundColor = currentColor;
            }
            
            yield return null;
        }
        
        // Ensure we end at exact target color
        currentColor = targetColor;
        if (targetCamera != null)
        {
            targetCamera.backgroundColor = currentColor;
        }
        
        transitionCoroutine = null;
    }
}

