using UnityEngine;

/// <summary>
/// TimeOfDayController - manages time of day state and notifies other systems
/// </summary>
public class TimeOfDayController : MonoBehaviour
{
    [Header("Time of Day Settings")]
    public bool isDayTime = true;
    
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            // Sync with GameManager
            isDayTime = gameManager.isDayTime;
            gameManager.OnTimeOfDayChanged += OnTimeOfDayChanged;
        }
    }
    
    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnTimeOfDayChanged -= OnTimeOfDayChanged;
        }
    }
    
    /// <summary>
    /// Called when time of day changes in GameManager
    /// </summary>
    void OnTimeOfDayChanged(bool isDay)
    {
        isDayTime = isDay;
    }
    
    /// <summary>
    /// Manually set time of day
    /// </summary>
    public void SetTimeOfDay(bool isDay)
    {
        isDayTime = isDay;
        if (gameManager != null)
        {
            gameManager.UpdateTimeOfDay(isDay);
        }
    }
}

