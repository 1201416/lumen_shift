using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Level Manager - handles progression between levels
/// Automatically switches between level generators in the same scene
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Level Generators")]
    public FirstLevelGenerator level1Generator;
    public Level2Generator level2Generator;
    public Level3Generator level3Generator;
    public Level4Generator level4Generator;
    public Level5Generator level5Generator;
    public Level6Generator level6Generator;
    public Level7Generator level7Generator;
    public Level8Generator level8Generator;
    public Level9Generator level9Generator;
    public Level10Generator level10Generator;
    public Level11Generator level11Generator;
    public Level12Generator level12Generator;
    public Level13Generator level13Generator;
    public Level14Generator level14Generator;
    public Level15Generator level15Generator;
    
    [Header("Settings")]
    public int currentLevel = 1;
    public int totalLevels = 15;
    
    private MonoBehaviour[] allGenerators;
    
    void Awake()
    {
        // Ensure all generator GameObjects exist in the scene (so they persist after Unity crashes)
        EnsureGeneratorsExist();
        
        // Collect all level generators
        allGenerators = new MonoBehaviour[]
        {
            level1Generator,
            level2Generator,
            level3Generator,
            level4Generator,
            level5Generator,
            level6Generator,
            level7Generator,
            level8Generator,
            level9Generator,
            level10Generator,
            level11Generator,
            level12Generator,
            level13Generator,
            level14Generator,
            level15Generator
        };
        
        // Auto-find generators if not assigned
        if (level1Generator == null) level1Generator = FindFirstObjectByType<FirstLevelGenerator>();
        if (level2Generator == null) level2Generator = FindFirstObjectByType<Level2Generator>();
        if (level3Generator == null) level3Generator = FindFirstObjectByType<Level3Generator>();
        if (level4Generator == null) level4Generator = FindFirstObjectByType<Level4Generator>();
        if (level5Generator == null) level5Generator = FindFirstObjectByType<Level5Generator>();
        if (level6Generator == null) level6Generator = FindFirstObjectByType<Level6Generator>();
        if (level7Generator == null) level7Generator = FindFirstObjectByType<Level7Generator>();
        if (level8Generator == null) level8Generator = FindFirstObjectByType<Level8Generator>();
        if (level9Generator == null) level9Generator = FindFirstObjectByType<Level9Generator>();
        if (level10Generator == null) level10Generator = FindFirstObjectByType<Level10Generator>();
        if (level11Generator == null) level11Generator = FindFirstObjectByType<Level11Generator>();
        if (level12Generator == null) level12Generator = FindFirstObjectByType<Level12Generator>();
        if (level13Generator == null) level13Generator = FindFirstObjectByType<Level13Generator>();
        if (level14Generator == null) level14Generator = FindFirstObjectByType<Level14Generator>();
        if (level15Generator == null) level15Generator = FindFirstObjectByType<Level15Generator>();
        
        // Detect current level from active generator
        DetectCurrentLevel();
    }
    
    /// <summary>
    /// Ensure all level generator GameObjects exist in the scene
    /// This prevents them from disappearing after Unity crashes
    /// </summary>
    void EnsureGeneratorsExist()
    {
        // Create Level 1 generator if it doesn't exist
        if (level1Generator == null)
        {
            GameObject go = GameObject.Find("FirstLevelGenerator");
            if (go == null)
            {
                go = new GameObject("FirstLevelGenerator");
                level1Generator = go.AddComponent<FirstLevelGenerator>();
            }
            else
            {
                level1Generator = go.GetComponent<FirstLevelGenerator>();
                if (level1Generator == null)
                {
                    level1Generator = go.AddComponent<FirstLevelGenerator>();
                }
            }
        }
        
        // Create Level 2-15 generators if they don't exist
        if (level2Generator == null) level2Generator = FindOrCreateGenerator<Level2Generator>("Level2Generator");
        if (level3Generator == null) level3Generator = FindOrCreateGenerator<Level3Generator>("Level3Generator");
        if (level4Generator == null) level4Generator = FindOrCreateGenerator<Level4Generator>("Level4Generator");
        if (level5Generator == null) level5Generator = FindOrCreateGenerator<Level5Generator>("Level5Generator");
        if (level6Generator == null) level6Generator = FindOrCreateGenerator<Level6Generator>("Level6Generator");
        if (level7Generator == null) level7Generator = FindOrCreateGenerator<Level7Generator>("Level7Generator");
        if (level8Generator == null) level8Generator = FindOrCreateGenerator<Level8Generator>("Level8Generator");
        if (level9Generator == null) level9Generator = FindOrCreateGenerator<Level9Generator>("Level9Generator");
        if (level10Generator == null) level10Generator = FindOrCreateGenerator<Level10Generator>("Level10Generator");
        if (level11Generator == null) level11Generator = FindOrCreateGenerator<Level11Generator>("Level11Generator");
        if (level12Generator == null) level12Generator = FindOrCreateGenerator<Level12Generator>("Level12Generator");
        if (level13Generator == null) level13Generator = FindOrCreateGenerator<Level13Generator>("Level13Generator");
        if (level14Generator == null) level14Generator = FindOrCreateGenerator<Level14Generator>("Level14Generator");
        if (level15Generator == null) level15Generator = FindOrCreateGenerator<Level15Generator>("Level15Generator");
    }
    
    /// <summary>
    /// Find an existing generator GameObject or create a new one
    /// Marks it as a scene object so it persists after Unity crashes
    /// </summary>
    T FindOrCreateGenerator<T>(string gameObjectName) where T : MonoBehaviour
    {
        GameObject go = GameObject.Find(gameObjectName);
        if (go == null)
        {
            go = new GameObject(gameObjectName);
            T component = go.AddComponent<T>();
            
            // Mark as scene object so it persists in the scene file
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // In editor mode, mark the scene as dirty so it saves
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
                );
            }
            #endif
            
            return component;
        }
        else
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
                
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                        UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
                    );
                }
                #endif
            }
            return component;
        }
    }
    
    void DetectCurrentLevel()
    {
        // Find which generator is active
        if (level1Generator != null && level1Generator.gameObject.activeSelf) currentLevel = 1;
        else if (level2Generator != null && level2Generator.gameObject.activeSelf) currentLevel = 2;
        else if (level3Generator != null && level3Generator.gameObject.activeSelf) currentLevel = 3;
        else if (level4Generator != null && level4Generator.gameObject.activeSelf) currentLevel = 4;
        else if (level5Generator != null && level5Generator.gameObject.activeSelf) currentLevel = 5;
        else if (level6Generator != null && level6Generator.gameObject.activeSelf) currentLevel = 6;
        else if (level7Generator != null && level7Generator.gameObject.activeSelf) currentLevel = 7;
        else if (level8Generator != null && level8Generator.gameObject.activeSelf) currentLevel = 8;
        else if (level9Generator != null && level9Generator.gameObject.activeSelf) currentLevel = 9;
        else if (level10Generator != null && level10Generator.gameObject.activeSelf) currentLevel = 10;
        else if (level11Generator != null && level11Generator.gameObject.activeSelf) currentLevel = 11;
        else if (level12Generator != null && level12Generator.gameObject.activeSelf) currentLevel = 12;
        else if (level13Generator != null && level13Generator.gameObject.activeSelf) currentLevel = 13;
        else if (level14Generator != null && level14Generator.gameObject.activeSelf) currentLevel = 14;
        else if (level15Generator != null && level15Generator.gameObject.activeSelf) currentLevel = 15;
    }
    
    /// <summary>
    /// Load the next level
    /// </summary>
    public void LoadNextLevel()
    {
        if (currentLevel >= totalLevels)
        {
            Debug.Log("All levels complete! Returning to Level 1...");
            LoadLevel(1);
            return;
        }
        
        LoadLevel(currentLevel + 1);
    }
    
    /// <summary>
    /// Load a specific level
    /// </summary>
    public void LoadLevel(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > totalLevels)
        {
            Debug.LogWarning($"Invalid level number: {levelNumber}");
            return;
        }
        
        Debug.Log($"LevelManager: Loading level {levelNumber}");
        
        // Set current level FIRST before disabling generators
        currentLevel = levelNumber;
        
        // Disable all generators
        DisableAllGenerators();
        
        // Enable and generate the requested level
        MonoBehaviour generator = GetGeneratorForLevel(levelNumber);
        if (generator != null)
        {
            generator.gameObject.SetActive(true);
            
            // Call the generate method based on level
            if (levelNumber == 1 && level1Generator != null)
            {
                level1Generator.GenerateFirstLevel();
            }
            else if (levelNumber == 2 && level2Generator != null)
            {
                level2Generator.GenerateLevel2();
            }
            else if (levelNumber == 3 && level3Generator != null)
            {
                level3Generator.GenerateLevel3();
            }
            else if (levelNumber == 4 && level4Generator != null)
            {
                level4Generator.GenerateLevel4();
            }
            else if (levelNumber == 5 && level5Generator != null)
            {
                level5Generator.GenerateLevel5();
            }
            else if (levelNumber == 6 && level6Generator != null)
            {
                level6Generator.GenerateLevel6();
            }
            else if (levelNumber == 7 && level7Generator != null)
            {
                level7Generator.GenerateLevel7();
            }
            else if (levelNumber == 8 && level8Generator != null)
            {
                level8Generator.GenerateLevel8();
            }
            else if (levelNumber == 9 && level9Generator != null)
            {
                level9Generator.GenerateLevel9();
            }
            else if (levelNumber == 10 && level10Generator != null)
            {
                level10Generator.GenerateLevel10();
            }
            else if (levelNumber == 11 && level11Generator != null)
            {
                level11Generator.GenerateLevel11();
            }
            else if (levelNumber == 12 && level12Generator != null)
            {
                level12Generator.GenerateLevel12();
            }
            else if (levelNumber == 13 && level13Generator != null)
            {
                level13Generator.GenerateLevel13();
            }
            else if (levelNumber == 14 && level14Generator != null)
            {
                level14Generator.GenerateLevel14();
            }
            else if (levelNumber == 15 && level15Generator != null)
            {
                level15Generator.GenerateLevel15();
            }
            
            Debug.Log($"Loaded Level {levelNumber}");
        }
        else
        {
            Debug.LogWarning($"Level {levelNumber} generator not found!");
        }
    }
    
    MonoBehaviour GetGeneratorForLevel(int level)
    {
        switch (level)
        {
            case 1: return level1Generator;
            case 2: return level2Generator;
            case 3: return level3Generator;
            case 4: return level4Generator;
            case 5: return level5Generator;
            case 6: return level6Generator;
            case 7: return level7Generator;
            case 8: return level8Generator;
            case 9: return level9Generator;
            case 10: return level10Generator;
            case 11: return level11Generator;
            case 12: return level12Generator;
            case 13: return level13Generator;
            case 14: return level14Generator;
            case 15: return level15Generator;
            default: return null;
        }
    }
    
    void DisableAllGenerators()
    {
        foreach (var generator in allGenerators)
        {
            if (generator != null)
            {
                generator.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Reload current level
    /// </summary>
    public void ReloadCurrentLevel()
    {
        LoadLevel(currentLevel);
    }
}
