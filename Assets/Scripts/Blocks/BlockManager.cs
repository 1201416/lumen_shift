using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// BlockManager - manages all blocks in the level and handles time-of-day changes
/// </summary>
public class BlockManager : MonoBehaviour
{
    [Header("Block Management")]
    public bool autoFindBlocks = true;
    
    private List<BoxBlock> allBoxBlocks = new List<BoxBlock>();
    private List<FloorBlock> allFloorBlocks = new List<FloorBlock>();
    
    void Start()
    {
        if (autoFindBlocks)
        {
            RefreshBlocks();
        }
        
        // Subscribe to time of day changes
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnTimeOfDayChanged += OnTimeOfDayChanged;
        }
    }
    
    void OnDestroy()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnTimeOfDayChanged -= OnTimeOfDayChanged;
        }
    }
    
    /// <summary>
    /// Refresh the list of all blocks in the scene
    /// </summary>
    public void RefreshBlocks()
    {
        allBoxBlocks.Clear();
        allFloorBlocks.Clear();
        
        allBoxBlocks.AddRange(FindObjectsByType<BoxBlock>(FindObjectsSortMode.None));
        allFloorBlocks.AddRange(FindObjectsByType<FloorBlock>(FindObjectsSortMode.None));
    }
    
    /// <summary>
    /// Called when time of day changes
    /// </summary>
    void OnTimeOfDayChanged(bool isDayTime)
    {
        // Blocks handle their own time-of-day logic via GameManager
        // This manager can be extended for additional block management features
    }
}

