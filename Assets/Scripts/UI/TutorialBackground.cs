using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Tutorial background that shows controls and hides after player starts moving
/// </summary>
public class TutorialBackground : MonoBehaviour
{
    [Header("Settings")]
    public float displayDuration = 5f; // Show for 5 seconds
    public bool hideOnFirstInput = true; // Hide when player presses any key
    
    private float timer = 0f;
    private bool hasHidden = false;
    
    void Update()
    {
        timer += Time.deltaTime;
        
        // Hide after duration
        if (timer >= displayDuration && !hasHidden)
        {
            HideTutorial();
        }
        
        // Hide on first input
        if (hideOnFirstInput && !hasHidden && Keyboard.current != null)
        {
            // Check for any movement or jump input
            bool anyInput = Keyboard.current.aKey.isPressed ||
                           Keyboard.current.dKey.isPressed ||
                           Keyboard.current.leftArrowKey.isPressed ||
                           Keyboard.current.rightArrowKey.isPressed ||
                           Keyboard.current.spaceKey.wasPressedThisFrame ||
                           Keyboard.current.upArrowKey.wasPressedThisFrame;
            
            if (anyInput)
            {
                HideTutorial();
            }
        }
    }
    
    void HideTutorial()
    {
        if (hasHidden) return;
        hasHidden = true;
        
        // Fade out smoothly
        StartCoroutine(FadeOut());
    }
    
    System.Collections.IEnumerator FadeOut()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        float fadeTime = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - (elapsed / fadeTime);
            yield return null;
        }
        
        gameObject.SetActive(false);
    }
}
