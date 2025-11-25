using UnityEngine;

/// <summary>
/// Generates the first level/tutorial level for new players.
/// This creates a beginner-friendly level that teaches basic mechanics.
/// Attach this to an empty GameObject in your scene.
/// </summary>
public class FirstLevelGenerator : MonoBehaviour
{
    [Header("Level Settings")]
    public float blockSize = 1f;
    [Tooltip("Total number of floor blocks to create")]
    public int totalFloorBlocks = 160;
    [Tooltip("Number of blocks visible on screen at once")]
    public int visibleBlocks = 16;
    
    [Header("Block Prefabs")]
    public GameObject floorBlockPrefab;
    public GameObject boxBlockPrefab;
    public GameObject lightningBoltPrefab;
    
    [Header("Parent Objects (Optional)")]
    public Transform blocksParent;
    public Transform itemsParent;
    
    [Header("Auto-Generate")]
    public bool generateOnStart = true;
    
    private GameObject playerInstance;

    void Start()
    {
        if (generateOnStart)
        {
            GenerateFirstLevel();
        }
    }

    /// <summary>
    /// Generate the first level - a tutorial/intro level for new players
    /// </summary>
    public void GenerateFirstLevel()
    {
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

        // Create invisible boundary walls to prevent player from leaving level
        CreateBoundaryWalls();

        // Generate ground floor (solid foundation)
        CreateGroundFloor();

        // Generate platforms and obstacles (vertical level design)
        CreatePlatforms();

        // Place lightning bolts (collectibles)
        PlaceLightningBolts();
        
        // Place finish line at the end of the level
        PlaceFinishLine();

        // Create player at start position (on top of first grass block, safe from falling)
        // Position: x=1 (1 block from left), y=1.5 (on top of grass which is at y~0.5)
        CreatePlayer(new Vector3(1f * blockSize, 1.5f, 0f));

        // Setup camera to follow player (CameraFollow script will handle it)
        SetupCamera();
        
        // Ensure GameManager exists in scene
        EnsureGameManagerExists();
        
        // Setup background controller for day/night background changes
        SetupBackgroundController();
        
        // Setup UI for lightning bolt counter
        SetupLightningBoltCounter();
        
        // Setup clouds in the sky
        SetupClouds();
        
        // Refresh GameManager to find all new boxes
        RefreshGameManager();
        
        Debug.Log("First Level generated successfully! Welcome to Lumen-Shift!");
    }
    
    /// <summary>
    /// Ensure GameManager exists in the scene (needed for T key toggle)
    /// </summary>
    void EnsureGameManagerExists()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            GameObject gmObject = new GameObject("GameManager");
            gameManager = gmObject.AddComponent<GameManager>();
            Debug.Log("GameManager created automatically - T key should now work!");
        }
    }
    
    /// <summary>
    /// Setup background controller to change background color with day/night
    /// </summary>
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
            bgController.gameManager = FindObjectOfType<GameManager>();
        }
    }
    
    /// <summary>
    /// Setup UI counter for lightning bolts in top right corner
    /// </summary>
    void SetupLightningBoltCounter()
    {
        // Check if counter already exists
        LightningBoltCounter existingCounter = FindObjectOfType<LightningBoltCounter>();
        if (existingCounter == null)
        {
            GameObject counterObj = new GameObject("LightningBoltCounter");
            LightningBoltCounter counter = counterObj.AddComponent<LightningBoltCounter>();
            counter.gameManager = FindObjectOfType<GameManager>();
        }
    }
    
    /// <summary>
    /// Setup clouds in the sky that change color with day/night
    /// </summary>
    void SetupClouds()
    {
        // Check if cloud controller already exists
        CloudController existingController = FindObjectOfType<CloudController>();
        if (existingController == null)
        {
            GameObject cloudControllerObj = new GameObject("CloudController");
            CloudController controller = cloudControllerObj.AddComponent<CloudController>();
            controller.startX = -10f;
            controller.endX = totalFloorBlocks * blockSize + 20f; // Cover entire level
            controller.minY = 5f;
            controller.maxY = 10f;
            controller.cloudCount = 12; // More clouds for better coverage
        }
    }
    
    /// <summary>
    /// Refresh GameManager to find all newly created boxes
    /// </summary>
    void RefreshGameManager()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.RefreshAllObjects();
            // Update time of day to ensure boxes are in correct state
            gameManager.UpdateTimeOfDay(gameManager.isDayTime);
        }
    }
    
    /// <summary>
    /// Setup camera to follow the player
    /// </summary>
    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Add CameraFollow component if it doesn't exist
            CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
            if (cameraFollow == null)
            {
                cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
            }
            
            // Set camera to follow player
            if (playerInstance != null)
            {
                cameraFollow.target = playerInstance.transform;
            }
            
            // Set camera size to show only visibleBlocks (16 blocks = 1/10 of 160)
            // But make it further away (larger orthographic size) for better resolution
            float aspectRatio = mainCamera.aspect;
            float desiredHalfWidth = visibleBlocks * 0.5f; // Half of visible blocks
            float calculatedSize = desiredHalfWidth / aspectRatio;
            
            // Make camera further away (2x zoom out for better resolution)
            float zoomedOutSize = calculatedSize * 2f;
            cameraFollow.cameraSize = zoomedOutSize;
            mainCamera.orthographicSize = zoomedOutSize;
            
            // Calculate camera bounds - level starts at x=0 and ends at x=totalFloorBlocks + extra blocks
            float levelStartX = 0f;
            int blocksPerOriginal = 10;
            float subBlockSize = blockSize / blocksPerOriginal;
            float levelEndX = (totalFloorBlocks * blockSize) + (blocksPerOriginal * subBlockSize);
            float cameraHalfWidth = calculatedSize * aspectRatio;
            
            // Set camera bounds to prevent showing empty space
            // Left bound: camera center can't go below cameraHalfWidth (so left edge is at 0)
            // Right bound: camera center can't go above levelEndX - cameraHalfWidth (so right edge is at levelEndX)
            cameraFollow.useBounds = true;
            cameraFollow.minX = levelStartX + cameraHalfWidth; // Left edge of level
            cameraFollow.maxX = levelEndX - cameraHalfWidth; // Right edge of level
            cameraFollow.minY = -2f;
            cameraFollow.maxY = 10f;
            
            // Set initial camera position to start of level (left edge visible)
            // Camera center should be at cameraHalfWidth so left edge is at 0
            float initialCameraX = cameraHalfWidth;
            if (playerInstance != null)
            {
                // Start camera at player position, but clamp to level bounds
                initialCameraX = Mathf.Clamp(
                    playerInstance.transform.position.x,
                    cameraFollow.minX,
                    cameraFollow.maxX
                );
            }
            
            mainCamera.transform.position = new Vector3(
                initialCameraX,
                playerInstance != null ? playerInstance.transform.position.y : 2f,
                -10f
            );
        }
    }

    /// <summary>
    /// Create the ground floor - solid foundation for the level
    /// Creates two layers: dirt blocks at y=-1 (lowest), grass blocks with irregular heights
    /// Ensures leftmost side is always filled and bottom is covered with dirt to hide background
    /// </summary>
    void CreateGroundFloor()
    {
        // Create a solid floor with totalFloorBlocks blocks (default 160)
        int blockCount = 0;
        
        // Create irregular grass pattern using Perlin noise or simple variation
        System.Random random = new System.Random(42); // Fixed seed for consistency
        
        // Create 10x more blocks for higher density and better resolution
        // Each original block position gets 10 smaller blocks
        int blocksPerOriginal = 10;
        float subBlockSize = blockSize / blocksPerOriginal; // Each sub-block is 1/10 the size
        
        // Create ground with 1 block incline for the first base
        // Start at y=-0.5, end at y=0.5 (1 block difference)
        float startY = -0.5f;
        float endY = 0.5f;
        float totalHeightDiff = endY - startY; // 1.0 block difference
        
        // Calculate camera bottom view to ensure dirt covers it
        Camera mainCamera = Camera.main;
        float cameraBottom = -5f; // Default fallback
        if (mainCamera != null)
        {
            CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
            if (cameraFollow != null)
            {
                cameraBottom = cameraFollow.minY - cameraFollow.cameraSize;
            }
            else
            {
                cameraBottom = mainCamera.transform.position.y - mainCamera.orthographicSize;
            }
        }
        
        // Ensure we have enough dirt blocks below to cover camera view
        float minDirtY = Mathf.Min(cameraBottom - 1f, startY * blockSize - 2f); // Extra layer below
        
        // Fill leftmost side BEFORE the main loop to ensure no gaps
        // Start from negative X to fill left side completely - extend further left
        float leftmostX = -5f * blockSize; // Start 5 blocks before 0 to ensure complete coverage
        int leftBlockIndex = 0;
        for (float x = leftmostX; x < 0f; x += subBlockSize)
        {
            // Use startY for leftmost blocks (no incline yet)
            float baseY = startY;
            
            // Create multiple layers of dirt to cover bottom
            for (float dirtY = minDirtY; dirtY < baseY * blockSize; dirtY += blockSize)
            {
                CreateFloorBlock(new Vector3(x, dirtY, 0f), FloorBlock.FloorType.Dirt);
                blockCount++;
            }
            
            // Create dirt block at the base level
            CreateFloorBlock(new Vector3(x, baseY * blockSize, 0f), FloorBlock.FloorType.Dirt);
            
            // Create grass block on top
            float grassHeight = GetGrassHeight(leftBlockIndex, random);
            CreateFloorBlock(new Vector3(x, (baseY + grassHeight) * blockSize, 0f), FloorBlock.FloorType.Grass);
            
            blockCount += 2;
            leftBlockIndex++;
        }
        
        // Main floor generation loop - ensure continuous floor with no gaps
        float lastBlockX = leftmostX;
        for (int x = 0; x < totalFloorBlocks; x++)
        {
            // For each original block position, create 10 sub-blocks
            for (int subX = 0; subX < blocksPerOriginal; subX++)
            {
                float blockX = (x * blockSize) + (subX * subBlockSize);
                
                // Ensure no gaps - if there's a gap, fill it
                if (blockX - lastBlockX > subBlockSize * 1.1f)
                {
                    // Fill gap between last block and current block
                    for (float fillX = lastBlockX + subBlockSize; fillX < blockX; fillX += subBlockSize)
                    {
                        float fillProgress = (fillX - leftmostX) / ((totalFloorBlocks * blockSize) - leftmostX);
                        fillProgress = Mathf.Clamp01(fillProgress);
                        float fillBaseY = startY + (fillProgress * totalHeightDiff);
                        
                        // Create dirt layers below
                        for (float dirtY = minDirtY; dirtY < fillBaseY * blockSize - blockSize * 0.1f; dirtY += blockSize)
                        {
                            CreateFloorBlock(new Vector3(fillX, dirtY, 0f), FloorBlock.FloorType.Dirt);
                            blockCount++;
                        }
                        
                        CreateFloorBlock(new Vector3(fillX, fillBaseY * blockSize, 0f), FloorBlock.FloorType.Dirt);
                        float fillGrassHeight = GetGrassHeight((int)((fillX - leftmostX) / subBlockSize), random);
                        CreateFloorBlock(new Vector3(fillX, (fillBaseY + fillGrassHeight) * blockSize, 0f), FloorBlock.FloorType.Grass);
                        blockCount += 2;
                    }
                }
                
                // Calculate Y position with incline (1 block difference from start to end)
                float progress = (float)(x * blocksPerOriginal + subX) / (float)(totalFloorBlocks * blocksPerOriginal);
                float baseY = startY + (progress * totalHeightDiff); // Incline from -0.5 to 0.5
                
                // Create multiple layers of dirt below to cover camera bottom view
                for (float dirtY = minDirtY; dirtY < baseY * blockSize - blockSize * 0.1f; dirtY += blockSize)
                {
                    CreateFloorBlock(new Vector3(blockX, dirtY, 0f), FloorBlock.FloorType.Dirt);
                    blockCount++;
                }
                
                // Create dirt block at the bottom (with incline)
                CreateFloorBlock(new Vector3(blockX, baseY * blockSize, 0f), FloorBlock.FloorType.Dirt);
                
                // Create grass block with irregular height (on top of dirt)
                float grassHeight = GetGrassHeight(x * blocksPerOriginal + subX, random);
                CreateFloorBlock(new Vector3(blockX, (baseY + grassHeight) * blockSize, 0f), FloorBlock.FloorType.Grass);
                
                blockCount += 2;
                lastBlockX = blockX;
            }
        }
        
        // Fill the rightmost side with floor blocks - ensure continuity
        float rightmostX = totalFloorBlocks * blockSize;
        
        // Check for gap between last main block and rightmost blocks
        if (rightmostX - lastBlockX > subBlockSize * 1.1f)
        {
            // Fill gap
            for (float fillX = lastBlockX + subBlockSize; fillX < rightmostX; fillX += subBlockSize)
            {
                float fillProgress = (fillX - leftmostX) / ((totalFloorBlocks * blockSize) - leftmostX);
                fillProgress = Mathf.Clamp01(fillProgress);
                float fillBaseY = startY + (fillProgress * totalHeightDiff);
                
                // Create dirt layers below
                for (float dirtY = minDirtY; dirtY < fillBaseY * blockSize - blockSize * 0.1f; dirtY += blockSize)
                {
                    CreateFloorBlock(new Vector3(fillX, dirtY, 0f), FloorBlock.FloorType.Dirt);
                    blockCount++;
                }
                
                CreateFloorBlock(new Vector3(fillX, fillBaseY * blockSize, 0f), FloorBlock.FloorType.Dirt);
                float fillGrassHeight = GetGrassHeight((int)((fillX - leftmostX) / subBlockSize), random);
                CreateFloorBlock(new Vector3(fillX, (fillBaseY + fillGrassHeight) * blockSize, 0f), FloorBlock.FloorType.Grass);
                blockCount += 2;
            }
        }
        
        // Create rightmost extension blocks
        for (int i = 0; i < blocksPerOriginal; i++)
        {
            float blockX = rightmostX + (i * subBlockSize);
            float progress = 1f; // At the end
            float baseY = startY + (progress * totalHeightDiff); // End Y position
            
            // Create dirt layers below
            for (float dirtY = minDirtY; dirtY < baseY * blockSize - blockSize * 0.1f; dirtY += blockSize)
            {
                CreateFloorBlock(new Vector3(blockX, dirtY, 0f), FloorBlock.FloorType.Dirt);
                blockCount++;
            }
            
            CreateFloorBlock(new Vector3(blockX, baseY * blockSize, 0f), FloorBlock.FloorType.Dirt);
            float grassHeight = GetGrassHeight(totalFloorBlocks * blocksPerOriginal + i, random);
            CreateFloorBlock(new Vector3(blockX, (baseY + grassHeight) * blockSize, 0f), FloorBlock.FloorType.Grass);
            blockCount += 2;
        }
        
        Debug.Log($"Created {blockCount} ground floor blocks (dirt + grass, with extra dirt layers to cover bottom)");
    }
    
    /// <summary>
    /// Generate irregular grass height for natural-looking terrain
    /// Returns height value between 0 and 0.5 (in block units)
    /// Creates a more organic, irregular pattern
    /// </summary>
    float GetGrassHeight(int x, System.Random random)
    {
        // Use multiple sine waves at different frequencies for organic variation
        float wave1 = Mathf.Sin(x * 0.25f) * 0.12f; // Slow wave
        float wave2 = Mathf.Sin(x * 0.7f) * 0.08f; // Medium wave
        float wave3 = Mathf.Sin(x * 1.5f) * 0.05f; // Fast wave
        
        // Add some random variation that's smoothed (not completely random per block)
        float randomSeed = (x * 17 + 23) % 100; // Pseudo-random but consistent
        float randomVariation = (randomSeed / 100f - 0.5f) * 0.15f;
        
        float baseHeight = 0.3f; // Base height
        
        float height = baseHeight + wave1 + wave2 + wave3 + randomVariation;
        
        // Clamp between 0 and 0.5 to keep it reasonable
        return Mathf.Clamp(height, 0f, 0.5f);
    }

    /// <summary>
    /// Create invisible boundary walls at left and right edges of level
    /// </summary>
    void CreateBoundaryWalls()
    {
        float wallHeight = 20f; // Tall enough to prevent jumping over
        float wallThickness = 0.5f;
        
        // Left boundary wall (invisible, blocks player from going left)
        GameObject leftWall = new GameObject("LeftBoundary");
        // Don't set tag if it doesn't exist - just use collider
        leftWall.transform.position = new Vector3(-wallThickness * 0.5f, wallHeight * 0.5f, 0f);
        leftWall.transform.SetParent(blocksParent);
        
        BoxCollider2D leftCollider = leftWall.AddComponent<BoxCollider2D>();
        leftCollider.size = new Vector2(wallThickness, wallHeight);
        leftCollider.isTrigger = false;
        
        // Make it invisible (no sprite renderer) - it's just a collider
        
        // Right boundary wall (invisible, blocks player from going right past level end)
        // Account for extra blocks added at the end (blocksPerOriginal sub-blocks)
        GameObject rightWall = new GameObject("RightBoundary");
        int blocksPerOriginal = 10;
        float subBlockSize = blockSize / blocksPerOriginal;
        float rightWallX = (totalFloorBlocks * blockSize) + (blocksPerOriginal * subBlockSize) + wallThickness * 0.5f;
        rightWall.transform.position = new Vector3(rightWallX, wallHeight * 0.5f, 0f);
        rightWall.transform.SetParent(blocksParent);
        
        BoxCollider2D rightCollider = rightWall.AddComponent<BoxCollider2D>();
        rightCollider.size = new Vector2(wallThickness, wallHeight);
        rightCollider.isTrigger = false;
        
        Debug.Log("Created boundary walls at left and right edges");
    }
    
    /// <summary>
    /// Create platforms and obstacles for the player to navigate
    /// LEVEL DESIGN: IMPOSSIBLE during day, POSSIBLE during night
    /// Day has gaps that are too large to jump, night fills those gaps
    /// </summary>
    void CreatePlatforms()
    {
        // Create 10x more blocks - each platform block becomes 10 smaller blocks
        int densityMultiplier = 10;
        float subBlockSize = blockSize / densityMultiplier;
        
        // DAY-ONLY BLOCKS: Create impossible gaps during day
        // These create sections that are too far apart to jump
        
        // Section 1: Starting platform (day) - small platform
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((5f + i * subBlockSize) * blockSize, 2f, 0f), visibleDuringDay: true);
        }
        
        // GAP 1: Impossible jump during day (4 blocks gap)
        // Section 2: Far platform (day) - too far to reach
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((12f + i * subBlockSize) * blockSize, 2.5f, 0f), visibleDuringDay: true);
        }
        
        // GAP 2: Another impossible gap
        // Section 3: Another far platform (day)
        for (int i = 0; i < 1 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((20f + i * subBlockSize) * blockSize, 3.5f, 0f), visibleDuringDay: true);
        }
        
        // GAP 3: Large vertical gap
        // Section 4: High platform (day) - too high to reach
        for (int i = 0; i < 1 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((28f + i * subBlockSize) * blockSize, 5.5f, 0f), visibleDuringDay: true);
        }
        
        // GAP 4: Another impossible gap
        // Section 5: Far right platform (day)
        for (int i = 0; i < 1 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((40f + i * subBlockSize) * blockSize, 4f, 0f), visibleDuringDay: true);
        }
        
        // GAP 5: Final impossible gap before finish
        // Section 6: Platform near finish (day) - but can't reach finish
        for (int i = 0; i < 1 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((60f + i * subBlockSize) * blockSize, 3f, 0f), visibleDuringDay: true);
        }
        
        // NIGHT-ONLY BLOCKS: Fill all gaps to make level possible
        // These bridges and platforms appear only at night
        
        // Night Bridge 1: Connects Section 1 to Section 2
        for (int i = 0; i < 5 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((7f + i * subBlockSize) * blockSize, 2.2f, 0f), visibleDuringDay: false);
        }
        
        // Night Bridge 2: Connects Section 2 to Section 3
        for (int i = 0; i < 6 * densityMultiplier; i++)
        {
            float x = 14f + i * subBlockSize;
            float y = 2.5f + (i * 0.15f); // Gradual incline
            CreateBoxBlock(new Vector3(x * blockSize, y, 0f), visibleDuringDay: false);
        }
        
        // Night Staircase 1: Connects Section 3 to Section 4 (vertical climb)
        for (int i = 0; i < 6 * densityMultiplier; i++)
        {
            float x = 21f + i * subBlockSize;
            float y = 3.5f + (i * 0.3f); // Steep climb
            CreateBoxBlock(new Vector3(x * blockSize, y, 0f), visibleDuringDay: false);
        }
        
        // Night Bridge 3: Connects Section 4 to Section 5
        for (int i = 0; i < 10 * densityMultiplier; i++)
        {
            float x = 29f + i * subBlockSize;
            float y = 5.5f - (i * 0.15f); // Gradual decline
            CreateBoxBlock(new Vector3(x * blockSize, y, 0f), visibleDuringDay: false);
        }
        
        // Night Bridge 4: Connects Section 5 to Section 6
        for (int i = 0; i < 18 * densityMultiplier; i++)
        {
            float x = 41f + i * subBlockSize;
            float y = 4f - (i * 0.055f); // Gradual decline
            CreateBoxBlock(new Vector3(x * blockSize, y, 0f), visibleDuringDay: false);
        }
        
        // Night Bridge 5: Final bridge to finish line
        for (int i = 0; i < 18 * densityMultiplier; i++)
        {
            float x = 61f + i * subBlockSize;
            float y = 3f + (i * 0.05f); // Slight incline
            CreateBoxBlock(new Vector3(x * blockSize, y, 0f), visibleDuringDay: false);
        }
        
        // Night: Additional platforms for easier navigation
        // Platform to reach lightning bolts
        for (int i = 0; i < 3 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((15f + i * subBlockSize) * blockSize, 3.5f, 0f), visibleDuringDay: false);
        }
        
        for (int i = 0; i < 3 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((25f + i * subBlockSize) * blockSize, 4.5f, 0f), visibleDuringDay: false);
        }
        
        for (int i = 0; i < 3 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((35f + i * subBlockSize) * blockSize, 4f, 0f), visibleDuringDay: false);
        }
        
        for (int i = 0; i < 3 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((50f + i * subBlockSize) * blockSize, 3.5f, 0f), visibleDuringDay: false);
        }
    }

    /// <summary>
    /// Place lightning bolts throughout the level
    /// All bolts are on night-only platforms - rewards for playing at night
    /// </summary>
    void PlaceLightningBolts()
    {
        // Bolt 1: On night bridge 1
        CreateLightningBolt(new Vector3(9f * blockSize, 3.2f, 0f));

        // Bolt 2: On night bridge 2
        CreateLightningBolt(new Vector3(17f * blockSize, 3.5f, 0f));

        // Bolt 3: On night staircase
        CreateLightningBolt(new Vector3(24f * blockSize, 5f, 0f));

        // Bolt 4: On night bridge 3
        CreateLightningBolt(new Vector3(32f * blockSize, 4.5f, 0f));

        // Bolt 5: On night bridge 4
        CreateLightningBolt(new Vector3(45f * blockSize, 3.2f, 0f));

        // Bolt 6: On night bridge 5 (near finish)
        CreateLightningBolt(new Vector3(70f * blockSize, 3.8f, 0f));

        // Bolt 7: Final reward near finish line
        CreateLightningBolt(new Vector3(85f * blockSize, 3.5f, 0f));
    }
    
    /// <summary>
    /// Place finish line at the end of the level
    /// </summary>
    void PlaceFinishLine()
    {
        // Calculate end position - at the rightmost edge of the level
        // Account for extra blocks at the end
        int blocksPerOriginal = 10;
        float subBlockSize = blockSize / blocksPerOriginal;
        float finishX = (totalFloorBlocks * blockSize) + (blocksPerOriginal * subBlockSize) - 2f; // 2 blocks before the end
        
        // Position finish line on the ground level (with incline)
        float startY = -0.5f;
        float endY = 0.5f;
        float progress = 1f; // At the end
        float finishY = (startY + (progress * (endY - startY))) * blockSize + 1.5f; // On top of ground
        
        CreateFinishLine(new Vector3(finishX, finishY, 0f));
    }

    void CreateFloorBlock(Vector3 position, FloorBlock.FloorType floorType = FloorBlock.FloorType.Grass)
    {
        if (floorBlockPrefab != null)
        {
            GameObject block = Instantiate(floorBlockPrefab, position, Quaternion.identity);
            block.transform.SetParent(blocksParent);
            
            FloorBlock floorBlock = block.GetComponent<FloorBlock>();
            if (floorBlock != null)
            {
                floorBlock.floorType = floorType;
                // Force sprite update after setting floor type
                floorBlock.SetupFloorBlock();
            }
        }
        else
        {
            GameObject block = new GameObject(floorType == FloorBlock.FloorType.Dirt ? "DirtBlock" : "GrassBlock");
            block.transform.position = position;
            block.transform.SetParent(blocksParent);
            
            FloorBlock floorBlock = block.AddComponent<FloorBlock>();
            floorBlock.floorType = floorType;
            // Force sprite update after setting floor type
            floorBlock.SetupFloorBlock();
        }
    }

    void CreateBoxBlock(Vector3 position, bool visibleDuringDay = true)
    {
        if (boxBlockPrefab != null)
        {
            GameObject block = Instantiate(boxBlockPrefab, position, Quaternion.identity);
            block.transform.SetParent(blocksParent);
            
            BoxBlock boxBlock = block.GetComponent<BoxBlock>();
            if (boxBlock != null)
            {
                boxBlock.visibleDuringDay = visibleDuringDay;
            }
        }
        else
        {
            GameObject block = new GameObject("BoxBlock");
            block.transform.position = position;
            block.transform.SetParent(blocksParent);
            
            BoxBlock boxBlock = block.AddComponent<BoxBlock>();
            boxBlock.visibleDuringDay = visibleDuringDay;
        }
    }

    void CreateLightningBolt(Vector3 position)
    {
        if (lightningBoltPrefab != null)
        {
            GameObject bolt = Instantiate(lightningBoltPrefab, position, Quaternion.identity);
            bolt.transform.SetParent(itemsParent);
        }
        else
        {
            GameObject bolt = new GameObject("LightningBolt");
            bolt.transform.position = position;
            bolt.transform.SetParent(itemsParent);
            
            LightningBolt lightningBolt = bolt.AddComponent<LightningBolt>();
        }
    }
    
    void CreateFinishLine(Vector3 position)
    {
        GameObject finishLine = new GameObject("FinishLine");
        finishLine.transform.position = position;
        finishLine.transform.SetParent(itemsParent);
        
        FinishLine finish = finishLine.AddComponent<FinishLine>();
        finish.requireAllBolts = false; // Set to true if you want to require all bolts
        finish.nextLevelSceneName = ""; // Leave empty to return to menu, or set to next level scene name
    }

    void CreatePlayer(Vector3 position)
    {
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null)
        {
            playerInstance = existingPlayer;
            playerInstance.transform.position = position;
        }
        else
        {
            playerInstance = new GameObject("Player");
            playerInstance.tag = "Player";
            playerInstance.transform.position = position;
            
            // PlayerController will add its own components in Awake()
            PlayerController controller = playerInstance.AddComponent<PlayerController>();
        }
    }

    void ClearLevel()
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



