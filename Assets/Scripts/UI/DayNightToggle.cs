using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Toggle button for switching between day and night
/// Attach this to a UI Button in your scene
/// </summary>
[RequireComponent(typeof(Button))]
public class DayNightToggle : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    
    [Header("UI Elements")]
    public Text buttonText; // Optional: Text component to show current state
    
    private Button toggleButton;
    
    void Start()
    {
        toggleButton = GetComponent<Button>();
        
        // Find GameManager if not assigned
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
        
        // Setup button click listener
        if (toggleButton != null && gameManager != null)
        {
            toggleButton.onClick.AddListener(OnToggleClicked);
        }
        
        // Subscribe to time changes to update button text
        if (gameManager != null)
        {
            gameManager.OnTimeOfDayChanged += UpdateButtonText;
            UpdateButtonText(gameManager.isDayTime);
        }
    }
    
    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnTimeOfDayChanged -= UpdateButtonText;
        }
    }
    
    void OnToggleClicked()
    {
        if (gameManager != null)
        {
            gameManager.ToggleDayNight();
        }
    }
    
    void UpdateButtonText(bool isDay)
    {
        if (buttonText != null)
        {
            buttonText.text = isDay ? "Day" : "Night";
        }
    }
}

