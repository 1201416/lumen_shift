using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Game Manager - tracks lightning bolts collected and manages game state.
/// This is what lightning bolts notify when collected.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Lightning Bolt Tracking")]
    public int totalBoltsCollected = 0;
    public int boltsRequiredToProgress = 3; // Number needed to unlock next level/area
    
    [Header("Time of Day")]
    public bool isDayTime = true;
    public float dayNightCycleDuration = 60f; // Seconds for full cycle
    // cycleTimer removed - not used (automatic cycling is commented out)
    
    [Header("Events")]
    public System.Action<bool> OnTimeOfDayChanged;
    public System.Action<int> OnBoltCollected;
    public System.Action OnAllBoltsCollected;
    
    private List<LightningBolt> allBolts = new List<LightningBolt>();
    private List<BoxBlock> allBoxes = new List<BoxBlock>();
    private List<Monster> allMonsters = new List<Monster>();

    void Start()
    {
        RefreshAllObjects();
        
        // Notify all objects of initial time state
        UpdateTimeOfDay(isDayTime);
    }
    
    /// <summary>
    /// Refresh lists of all bolts, boxes, and monsters in the scene
    /// Call this after generating new level content
    /// </summary>
    public void RefreshAllObjects()
    {
        allBolts.Clear();
        allBoxes.Clear();
        allMonsters.Clear();
        
        // Find all collectibles, blocks, and monsters in the scene
        allBolts.AddRange(FindObjectsByType<LightningBolt>(FindObjectsSortMode.None));
        allBoxes.AddRange(FindObjectsByType<BoxBlock>(FindObjectsSortMode.None));
        allMonsters.AddRange(FindObjectsByType<Monster>(FindObjectsSortMode.None));
    }

    void Update()
    {
        // Day/night cycle (if you want automatic cycling)
        // Uncomment this if you want automatic day/night switching
        /*
        cycleTimer += Time.deltaTime;
        if (cycleTimer >= dayNightCycleDuration)
        {
            cycleTimer = 0f;
            ToggleDayNight();
        }
        */
        
        // Manual toggle with SHIFT key - using new Input System
        if (Keyboard.current != null && 
            (Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.rightShiftKey.wasPressedThisFrame))
        {
            ToggleDayNight();
        }
    }

    /// <summary>
    /// Called when a lightning bolt is collected
    /// </summary>
    public void CollectLightningBolt(int value)
    {
        totalBoltsCollected += value;
        
        OnBoltCollected?.Invoke(totalBoltsCollected);
        
        Debug.Log($"Lightning Bolt Collected! Total: {totalBoltsCollected}/{boltsRequiredToProgress}");
        
        // Check if player has collected enough bolts
        if (totalBoltsCollected >= boltsRequiredToProgress)
        {
            OnAllBoltsCollected?.Invoke();
            Debug.Log("All lightning bolts collected! You can progress!");
        }
    }

    /// <summary>
    /// Toggle between day and night
    /// </summary>
    public void ToggleDayNight()
    {
        isDayTime = !isDayTime;
        UpdateTimeOfDay(isDayTime);
    }

    /// <summary>
    /// Set time of day and notify all relevant objects
    /// </summary>
    public void UpdateTimeOfDay(bool isDay)
    {
        isDayTime = isDay;
        
        // Notify all lightning bolts
        foreach (var bolt in allBolts)
        {
            if (bolt != null)
            {
                bolt.SetTimeOfDay(isDay);
            }
        }
        
        // Notify all boxes
        foreach (var box in allBoxes)
        {
            if (box != null)
            {
                box.SetTimeOfDay(isDay);
            }
        }
        
        // Notify all monsters
        foreach (var monster in allMonsters)
        {
            if (monster != null)
            {
                monster.SetTimeOfDay(isDay);
            }
        }
        
        OnTimeOfDayChanged?.Invoke(isDay);
        
        Debug.Log($"Time of Day: {(isDay ? "DAY" : "NIGHT")}");
    }
}

