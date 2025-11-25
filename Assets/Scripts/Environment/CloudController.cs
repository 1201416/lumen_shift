using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages clouds in the sky that change color with day/night
/// Creates and manages multiple clouds
/// </summary>
public class CloudController : MonoBehaviour
{
    [Header("Cloud Generation")]
    public int cloudCount = 8;
    public float cloudSpacing = 15f;
    public float minY = 5f;
    public float maxY = 10f;
    public float startX = -10f;
    public float endX = 200f;
    
    [Header("Cloud Prefab (Optional)")]
    public GameObject cloudPrefab; // If you have a prefab, use it. Otherwise creates default clouds.
    
    private List<Cloud> allClouds = new List<Cloud>();
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        
        // Subscribe to day/night changes
        if (gameManager != null)
        {
            gameManager.OnTimeOfDayChanged += UpdateAllClouds;
        }
        
        // Generate clouds
        GenerateClouds();
    }
    
    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnTimeOfDayChanged -= UpdateAllClouds;
        }
    }
    
    /// <summary>
    /// Generate clouds across the sky
    /// </summary>
    void GenerateClouds()
    {
        System.Random random = new System.Random(123); // Fixed seed for consistency
        
        for (int i = 0; i < cloudCount; i++)
        {
            // Random X position across the level
            float x = startX + (float)(random.NextDouble() * (endX - startX));
            
            // Random Y position in sky
            float y = minY + (float)(random.NextDouble() * (maxY - minY));
            
            Vector3 position = new Vector3(x, y, 0f);
            
            GameObject cloudObj;
            if (cloudPrefab != null)
            {
                cloudObj = Instantiate(cloudPrefab, position, Quaternion.identity);
                cloudObj.transform.SetParent(transform);
                
                // If using prefab, get Cloud component
                Cloud cloud = cloudObj.GetComponent<Cloud>();
                if (cloud != null)
                {
                    allClouds.Add(cloud);
                }
                else
                {
                    // If prefab doesn't have Cloud component, add it
                    cloud = cloudObj.AddComponent<Cloud>();
                    allClouds.Add(cloud);
                }
            }
            else
            {
                // Create default cloud
                cloudObj = new GameObject($"Cloud_{i}");
                cloudObj.transform.position = position;
                cloudObj.transform.SetParent(transform);
                
                Cloud cloud = cloudObj.AddComponent<Cloud>();
                allClouds.Add(cloud);
            }
        }
        
        // Update all clouds with initial time state
        if (gameManager != null)
        {
            UpdateAllClouds(gameManager.isDayTime);
        }
    }
    
    /// <summary>
    /// Update all clouds when day/night changes
    /// </summary>
    void UpdateAllClouds(bool isDay)
    {
        foreach (var cloud in allClouds)
        {
            if (cloud != null)
            {
                cloud.SetTimeOfDay(isDay);
            }
        }
    }
}

