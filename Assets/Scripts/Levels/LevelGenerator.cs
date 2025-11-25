using UnityEngine;

/// <summary>
/// Generates a level from LevelData - creates all blocks, collectibles, etc.
/// Attach this to an empty GameObject in your scene.
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    [Header("Level Data")]
    public LevelData levelData;
    
    [Header("Parent Objects (Optional)")]
    public Transform blocksParent;
    public Transform itemsParent;
    
    [Header("Auto-Generate")]
    public bool generateOnStart = true;
    
    private GameObject playerInstance;

    void Start()
    {
        if (generateOnStart && levelData != null)
        {
            GenerateLevel();
        }
    }

    /// <summary>
    /// Generate the entire level from levelData
    /// </summary>
    public void GenerateLevel()
    {
        if (levelData == null)
        {
            Debug.LogError("LevelData is not assigned!");
            return;
        }

        // Clear existing level if regenerating
        ClearLevel();

        // Create parent objects if they don't exist
        if (blocksParent == null)
        {
            GameObject blocksGO = new GameObject("Blocks");
            blocksGO.transform.SetParent(transform);
            blocksParent = blocksGO.transform;
        }

        if (itemsParent == null)
        {
            GameObject itemsGO = new GameObject("Items");
            itemsGO.transform.SetParent(transform);
            itemsParent = itemsGO.transform;
        }

        // Parse and generate level
        char[,] grid = levelData.ParseLayout();
        
        for (int y = 0; y < levelData.levelHeight; y++)
        {
            for (int x = 0; x < levelData.levelWidth; x++)
            {
                char cell = grid[y, x];
                Vector3 position = new Vector3(
                    x * levelData.blockSize,
                    y * levelData.blockSize,
                    0f
                );

                switch (cell)
                {
                    case 'F': // Floor block
                        CreateFloorBlock(position);
                        break;
                    
                    case 'B': // Box block
                        CreateBoxBlock(position);
                        break;
                    
                    case 'L': // Lightning bolt
                        CreateLightningBolt(position);
                        break;
                    
                    case 'P': // Player start
                        if (playerInstance == null)
                        {
                            CreatePlayer(position);
                        }
                        break;
                }
            }
        }

        // If player wasn't placed in layout, use playerStartPosition
        if (playerInstance == null)
        {
            CreatePlayer(levelData.playerStartPosition);
        }

        Debug.Log($"Level '{levelData.levelName}' generated successfully!");
    }

    void CreateFloorBlock(Vector3 position)
    {
        if (levelData.floorBlockPrefab != null)
        {
            GameObject block = Instantiate(levelData.floorBlockPrefab, position, Quaternion.identity);
            block.transform.SetParent(blocksParent);
        }
        else
        {
            // Create floor block manually if prefab not assigned
            GameObject block = new GameObject("FloorBlock");
            block.transform.position = position;
            block.transform.SetParent(blocksParent);
            
            FloorBlock floorBlock = block.AddComponent<FloorBlock>();
            // You'll need to assign sprite in Unity Inspector or via code
        }
    }

    void CreateBoxBlock(Vector3 position)
    {
        if (levelData.boxBlockPrefab != null)
        {
            GameObject block = Instantiate(levelData.boxBlockPrefab, position, Quaternion.identity);
            block.transform.SetParent(blocksParent);
        }
        else
        {
            // Create box block manually if prefab not assigned
            GameObject block = new GameObject("BoxBlock");
            block.transform.position = position;
            block.transform.SetParent(blocksParent);
            
            BoxBlock boxBlock = block.AddComponent<BoxBlock>();
        }
    }

    void CreateLightningBolt(Vector3 position)
    {
        if (levelData.lightningBoltPrefab != null)
        {
            GameObject bolt = Instantiate(levelData.lightningBoltPrefab, position, Quaternion.identity);
            bolt.transform.SetParent(itemsParent);
        }
        else
        {
            // Create lightning bolt manually if prefab not assigned
            GameObject bolt = new GameObject("LightningBolt");
            bolt.transform.position = position;
            bolt.transform.SetParent(itemsParent);
            
            LightningBolt lightningBolt = bolt.AddComponent<LightningBolt>();
        }
    }

    void CreatePlayer(Vector3 position)
    {
        // Find existing player or create one
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null)
        {
            playerInstance = existingPlayer;
            playerInstance.transform.position = position;
        }
        else
        {
            // Create a simple player if none exists
            playerInstance = new GameObject("Player");
            playerInstance.tag = "Player";
            playerInstance.transform.position = position;
            
            // Add components
            SpriteRenderer sr = playerInstance.AddComponent<SpriteRenderer>();
            sr.color = Color.blue; // Temporary visual
            
            Rigidbody2D rb = playerInstance.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            
            CircleCollider2D col = playerInstance.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;
            
            PlayerController controller = playerInstance.AddComponent<PlayerController>();
        }
    }

    /// <summary>
    /// Clear all generated level objects
    /// </summary>
    public void ClearLevel()
    {
        if (blocksParent != null)
        {
            foreach (Transform child in blocksParent)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        if (itemsParent != null)
        {
            foreach (Transform child in itemsParent)
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}



