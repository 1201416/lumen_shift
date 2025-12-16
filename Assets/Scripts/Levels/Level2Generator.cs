using UnityEngine;

/// <summary>
/// Generates Level 2 - Simple fixed level with 2 lightning bolts
/// </summary>
public class Level2Generator : MonoBehaviour
{
    [Header("Level Settings")]
    public float blockSize = 1f;
    
    [Header("Block Prefabs")]
    public GameObject floorBlockPrefab;
    public GameObject boxBlockPrefab;
    public GameObject lightningBoltPrefab;
    public GameObject playerPrefab;
    public Sprite playerSprite;
    
    [Header("Parent Objects (Optional)")]
    public Transform blocksParent;
    public Transform itemsParent;
    
    [Header("Auto-Generate")]
    public bool generateOnStart = true;
    
    private GameObject playerInstance;
    private int levelLength = 25; // Fixed length

    void Start()
    {
        // Check if LevelManager wants to control generation
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null && levelManager.currentLevel != 2)
        {
            // LevelManager will handle generation, don't auto-generate
            gameObject.SetActive(false);
            return;
        }
        
        if (generateOnStart)
        {
            GenerateLevel2();
        }
    }

    public void GenerateLevel2()
    {
        ClearLevel();

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

        CreateBoundaryWalls();
        CreateFixedLevel();
        // Create player at start position (on ground, visible in camera)
        CreatePlayer(new Vector3(1f * blockSize, 1f, 0f)); // Start 1 block from left (at x=1)
        SetupCamera();
        EnsureGameManagerExists();
        SetupInputSystemFixer();
        SetupBackgroundController();
        SetupLightningBoltCounter();
        SetupDeathScreen();
        SetupWinnerScreen();
        SetupClouds();
        RefreshGameManager();
        
        // Refresh lightning bolt counter AFTER bolts are created
        LightningBoltCounter counter = FindFirstObjectByType<LightningBoltCounter>();
        if (counter != null)
        {
            counter.RefreshTotalBolts();
            counter.ResetToOriginalTotal();
            // Force update display
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                counter.UpdateCounter(gameManager.totalBoltsCollected);
            }
        }

        Debug.Log("Level 2 generated successfully!");
    }

    void CreateFixedLevel()
    {
        // Ground floor: 25 blocks
        for (int i = 0; i < levelLength; i++)
        {
            CreateFloorBlock(new Vector3(i * blockSize, 0f, 0f), FloorBlock.FloorType.Grass);
        }
        
        // Platform 1: Small platform at x=6, height 1.5 blocks (above ground)
        CreateBoxBlock(new Vector3(6f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(6.5f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        
        // Platform 2: Small platform at x=12, height 1.5 blocks (above ground)
        CreateBoxBlock(new Vector3(12f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(12.5f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        
        // Platform 3: Small platform at x=18, height 1.5 blocks (above ground, near finish)
        CreateBoxBlock(new Vector3(18f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(18.5f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        
        // Place 1 monster on ground
        CreateMonster(new Vector3(10f * blockSize, 0.5f, 0f), Monster.MonsterType.Mushroom);
        
        // Place 2 lightning bolts (Level 1-2: 2 bolts) - above platforms
        CreateLightningBolt(new Vector3(6.25f * blockSize, 1.5f + 1.5f, 0f));
        CreateLightningBolt(new Vector3(18.25f * blockSize, 1.5f + 1.5f, 0f));
        
        // Place finish line at the end
        CreateFinishLine(new Vector3(levelLength * blockSize, 1.5f, 0f));
    }

    void CreateBoundaryWalls()
    {
        float wallHeight = 20f;
        float wallThickness = 0.5f;
        
        // Only create left boundary wall to prevent going backwards
        // Right side is open - player can pass finish line and will be stopped by finish line logic
        GameObject leftWall = new GameObject("LeftBoundary");
        leftWall.transform.position = new Vector3(-wallThickness * 0.5f, wallHeight * 0.5f, 0f);
        leftWall.transform.SetParent(blocksParent);
        BoxCollider2D leftCollider = leftWall.AddComponent<BoxCollider2D>();
        leftCollider.size = new Vector2(wallThickness, wallHeight);
        leftCollider.isTrigger = false;
        
        // No right boundary wall - player can pass finish line
    }

    void SetupWinnerScreen()
    {
        WinnerScreen existingWinnerScreen = FindFirstObjectByType<WinnerScreen>();
        if (existingWinnerScreen == null)
        {
            GameObject winnerScreenObj = new GameObject("WinnerScreen");
            WinnerScreen winnerScreen = winnerScreenObj.AddComponent<WinnerScreen>();
            winnerScreen.SetCurrentLevel(2);
        }
    }

    void CreateFinishLine(Vector3 position)
    {
        GameObject finishLine = new GameObject("FinishLine");
        finishLine.transform.position = position;
        finishLine.transform.SetParent(itemsParent);
        
        FinishLine finish = finishLine.AddComponent<FinishLine>();
        finish.requireAllBolts = true;
        finish.nextLevelSceneName = "";
    }

    void CreatePlayer(Vector3 position)
    {
        if (playerPrefab != null)
        {
            playerInstance = Instantiate(playerPrefab, position, Quaternion.identity);
            playerInstance.name = "Player";
            
            // Ensure PlayerController is enabled
            PlayerController controller = playerInstance.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = true;
            }
        }
        else
        {
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                playerInstance = existingPlayer;
                playerInstance.transform.position = position;
                
                // Re-enable PlayerController if it was disabled (e.g., from level completion)
                PlayerController controller = playerInstance.GetComponent<PlayerController>();
                if (controller != null)
                {
                    controller.enabled = true;
                }
                
                // Reset velocity
                Rigidbody2D rb = playerInstance.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }
        }
    }

    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            mainCamera = cameraObj.AddComponent<Camera>();
            mainCamera.orthographic = true;
        }
        
        CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
        if (cameraFollow == null)
        {
            cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
        }
        
        if (playerInstance != null)
        {
            cameraFollow.target = playerInstance.transform;
        }
        
        // Use same camera setup as Level 1 (tutorial level) for consistency
        float aspectRatio = mainCamera.aspect;
        float desiredHalfWidth = 16f * 0.5f; // Half of visible blocks (16 blocks like Level 1)
        float calculatedSize = desiredHalfWidth / aspectRatio;
        
        // Zoom in 3x total - use 1/3 the size (makes everything appear 3x bigger)
        float zoomedInSize = calculatedSize * 0.333f;
        
        cameraFollow.cameraSize = zoomedInSize;
        mainCamera.orthographicSize = zoomedInSize;
        
        float cameraHalfWidth = zoomedInSize * aspectRatio;
        
        // Camera bounds: left edge at x=0, right edge at last block (x=levelLength-1)
        float levelStartX = 0f * blockSize; // Level starts at x=0
        float levelEndX = (levelLength - 1) * blockSize; // Last block is at x=24 (for 25-block level)
        
        cameraFollow.useBounds = true;
        cameraFollow.minX = levelStartX + cameraHalfWidth; // Left edge of camera at x=0
        cameraFollow.maxX = levelEndX - cameraHalfWidth; // Right edge of camera at last block
        cameraFollow.minY = -2f;
        cameraFollow.maxY = 10f;
        
        // Position camera so player is exactly one block away from left edge
        // Player spawns at x=1, we want left edge at x=0 (so player is 1 block from left)
        // Camera center = left edge + cameraHalfWidth = 0 + cameraHalfWidth
        float playerStartX = 1f * blockSize; // Player spawns at x=1 (1 block from left)
        float leftEdgeX = 0f * blockSize; // Left edge should be at x=0
        float initialCameraX = leftEdgeX + cameraHalfWidth;
        
        if (playerInstance != null)
        {
            float playerX = playerInstance.transform.position.x;
            // Player at x=1, left edge at x=0, so camera center = 0 + cameraHalfWidth
            leftEdgeX = 0f; // Always keep left edge at x=0
            initialCameraX = leftEdgeX + cameraHalfWidth;
            
            // Clamp to bounds to ensure full floor is visible
            initialCameraX = Mathf.Clamp(
                initialCameraX,
                cameraFollow.minX,
                cameraFollow.maxX
            );
        }
        
        mainCamera.transform.position = new Vector3(initialCameraX, playerInstance != null ? playerInstance.transform.position.y : 2f, -10f);
    }

    void EnsureGameManagerExists()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            GameObject gmObject = new GameObject("GameManager");
            gmObject.AddComponent<GameManager>();
        }
    }

    void SetupInputSystemFixer()
    {
        InputSystemFixer existingFixer = FindFirstObjectByType<InputSystemFixer>();
        if (existingFixer == null)
        {
            GameObject fixerObj = new GameObject("InputSystemFixer");
            fixerObj.AddComponent<InputSystemFixer>();
        }
    }

    void SetupBackgroundController()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            BackgroundController bgController = mainCamera.GetComponent<BackgroundController>();
            if (bgController == null)
            {
                bgController = mainCamera.gameObject.AddComponent<BackgroundController>();
            }
            
            bgController.targetCamera = mainCamera;
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                bgController.gameManager = gm;
                bgController.UpdateBackground(gm.isDayTime);
            }
        }
    }

    void SetupLightningBoltCounter()
    {
        LightningBoltCounter existingCounter = FindFirstObjectByType<LightningBoltCounter>();
        if (existingCounter == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
            
            GameObject counterObj = new GameObject("LightningBoltCounter");
            if (mainCamera != null)
            {
                counterObj.transform.SetParent(mainCamera.transform, false);
            }
            
            LightningBoltCounter counter = counterObj.AddComponent<LightningBoltCounter>();
            counter.gameManager = FindFirstObjectByType<GameManager>();
        }
    }

    void SetupDeathScreen()
    {
        DeathScreen existingDeathScreen = FindFirstObjectByType<DeathScreen>();
        if (existingDeathScreen == null)
        {
            GameObject deathScreenObj = new GameObject("DeathScreen");
            deathScreenObj.AddComponent<DeathScreen>();
        }
    }

    void SetupClouds()
    {
        CloudController existingController = FindFirstObjectByType<CloudController>();
        if (existingController == null)
        {
            GameObject cloudControllerObj = new GameObject("CloudController");
            CloudController controller = cloudControllerObj.AddComponent<CloudController>();
            controller.startX = -10f;
            controller.endX = levelLength * blockSize + 20f;
            controller.minY = 5f;
            controller.maxY = 10f;
            controller.cloudCount = 8;
        }
    }

    void RefreshGameManager()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            // Reset bolt count for new level
            gameManager.totalBoltsCollected = 0;
            gameManager.RefreshAllObjects();
            gameManager.UpdateTimeOfDay(gameManager.isDayTime);
        }
    }

    GameObject CreateBoxBlock(Vector3 position, bool visibleDuringDay)
    {
        GameObject block;
        if (boxBlockPrefab != null)
        {
            block = Instantiate(boxBlockPrefab, position, Quaternion.identity);
            block.transform.SetParent(blocksParent);
            BoxBlock boxBlock = block.GetComponent<BoxBlock>();
            if (boxBlock != null)
            {
                boxBlock.visibleDuringDay = visibleDuringDay;
            }
        }
        else
        {
            block = new GameObject("BoxBlock");
            block.transform.position = position;
            block.transform.SetParent(blocksParent);
            BoxBlock boxBlock = block.AddComponent<BoxBlock>();
            boxBlock.visibleDuringDay = visibleDuringDay;
        }
        block.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        return block;
    }

    GameObject CreateFloorBlock(Vector3 position, FloorBlock.FloorType floorType)
    {
        GameObject block;
        if (floorBlockPrefab != null)
        {
            block = Instantiate(floorBlockPrefab, position, Quaternion.identity);
            block.transform.SetParent(blocksParent);
            FloorBlock floorBlock = block.GetComponent<FloorBlock>();
            if (floorBlock != null)
            {
                floorBlock.floorType = floorType;
                floorBlock.SetupFloorBlock();
            }
        }
        else
        {
            block = new GameObject(floorType == FloorBlock.FloorType.Dirt ? "DirtBlock" : "GrassBlock");
            block.transform.position = position;
            block.transform.SetParent(blocksParent);
            FloorBlock floorBlock = block.AddComponent<FloorBlock>();
            floorBlock.floorType = floorType;
            floorBlock.SetupFloorBlock();
        }
        return block;
    }

    GameObject CreateLightningBolt(Vector3 position)
    {
        // Ensure lightning bolt is near night blocks and away from day blocks
        // Find nearest night block
        float nearestNightBlockDistance = float.MaxValue;
        float nearestDayBlockDistance = float.MaxValue;
        float checkRadius = 3f * blockSize; // Check within 3 blocks
        
        BoxBlock[] allBlocks = FindObjectsByType<BoxBlock>(FindObjectsSortMode.None);
        foreach (BoxBlock block in allBlocks)
        {
            float distance = Vector3.Distance(position, block.transform.position);
            if (block.visibleDuringDay)
            {
                // Day block - should be far away
                if (distance < nearestDayBlockDistance)
                {
                    nearestDayBlockDistance = distance;
                }
            }
            else
            {
                // Night block - should be nearby
                if (distance < nearestNightBlockDistance)
                {
                    nearestNightBlockDistance = distance;
                }
            }
        }
        
        // Validate: bolt should be near night block (within 2 blocks) and away from day blocks (at least 1.5 blocks)
        if (nearestNightBlockDistance > 2f * blockSize)
        {
            Debug.LogWarning($"Lightning bolt at {position} is too far from night blocks (distance: {nearestNightBlockDistance}). Adjusting position.");
            // Try to find a better position near a night block
            foreach (BoxBlock block in allBlocks)
            {
                if (!block.visibleDuringDay)
                {
                    // Found a night block, place bolt above it
                    Vector3 newPosition = block.transform.position + Vector3.up * 1.5f;
                    position = newPosition;
                    break;
                }
            }
        }
        
        if (nearestDayBlockDistance < 1.5f * blockSize)
        {
            Debug.LogWarning($"Lightning bolt at {position} is too close to day blocks (distance: {nearestDayBlockDistance}). Adjusting position.");
            // Move bolt away from day blocks, towards night blocks
            Vector3 awayFromDay = Vector3.zero;
            int nightBlockCount = 0;
            foreach (BoxBlock block in allBlocks)
            {
                if (!block.visibleDuringDay)
                {
                    Vector3 direction = (block.transform.position - position).normalized;
                    awayFromDay += direction;
                    nightBlockCount++;
                }
            }
            if (nightBlockCount > 0)
            {
                awayFromDay /= nightBlockCount;
                position += awayFromDay * 1.5f * blockSize;
            }
        }
        
        GameObject bolt;
        if (lightningBoltPrefab != null)
        {
            bolt = Instantiate(lightningBoltPrefab, position, Quaternion.identity);
            bolt.transform.SetParent(itemsParent);
            bolt.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        }
        else
        {
            bolt = new GameObject("LightningBolt");
            bolt.transform.position = position;
            bolt.transform.SetParent(itemsParent);
            bolt.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            LightningBolt lightningBolt = bolt.AddComponent<LightningBolt>();
        }
        return bolt;
    }

    void CreateMonster(Vector3 position, Monster.MonsterType monsterType = Monster.MonsterType.FlyingEye)
    {
        GameObject monster = new GameObject($"Monster_{monsterType}");
        monster.transform.position = position;
        monster.transform.SetParent(itemsParent);
        
        Monster monsterComponent = monster.AddComponent<Monster>();
        monsterComponent.monsterType = monsterType;
        monsterComponent.monsterColor = Color.white;
        monsterComponent.size = 0.8f;
        monsterComponent.visibleOnlyAtNight = true;
    }

    void ClearLevel()
    {
        if (blocksParent != null)
        {
            foreach (Transform child in blocksParent)
            {
                #if UNITY_EDITOR
                DestroyImmediate(child.gameObject);
                #else
                Destroy(child.gameObject);
                #endif
            }
        }

        if (itemsParent != null)
        {
            foreach (Transform child in itemsParent)
            {
                #if UNITY_EDITOR
                DestroyImmediate(child.gameObject);
                #else
                Destroy(child.gameObject);
                #endif
            }
        }
    }
}
