using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public int totalFloorBlocks = 96; // Reduced to 3/5 of original (160 * 0.6 = 96)
    [Tooltip("Number of blocks visible on screen at once")]
    public int visibleBlocks = 16;
    
    [Header("Block Prefabs")]
    public GameObject floorBlockPrefab;
    public GameObject boxBlockPrefab;
    public GameObject lightningBoltPrefab;
    [Tooltip("Player prefab to instantiate. If not set, will find existing Player or create a new one.")]
    public GameObject playerPrefab;
    [Tooltip("Player sprite to use when creating player from code. If not set, will try to load 'Jump' sprite from PixelAdventure.")]
    public Sprite playerSprite;
    
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

        // Place monsters (death points) in the middle of the level
        PlaceMonsters();
        
        // Place finish line at the end of the level
        PlaceFinishLine();

        // Create player at start position (on top of first grass block, safe from falling)
        // Calculate proper spawn position based on ground height
        float playerStartX = 1f * blockSize;
        float startY = -0.5f;
        float endY = 0.5f;
        float leftmostX = -5f * blockSize;
        float fillUntilX = playerStartX + (2f * blockSize);
        float progress = Mathf.Max(0f, (playerStartX - leftmostX) / (fillUntilX - leftmostX + 1f));
        float baseY = startY + (progress * (endY - startY) * 0.1f); // Match ground calculation
        float grassHeight = 0.1f; // Approximate grass height
        float playerSpawnY = (baseY + grassHeight) * blockSize + 0.5f; // On top of grass + 0.5 units
        CreatePlayer(new Vector3(playerStartX, playerSpawnY, 0f));

        // Setup camera to follow player (CameraFollow script will handle it)
        SetupCamera();
        
        // Ensure GameManager exists in scene
        EnsureGameManagerExists();
        
        // Fix Input System compatibility (must be done early)
        SetupInputSystemFixer();
        
        // Setup background controller for day/night background changes
        SetupBackgroundController();
        
        // Setup UI for lightning bolt counter
        SetupLightningBoltCounter();
        
        // Setup death screen
        SetupDeathScreen();
        
        // Setup clouds in the sky
        SetupClouds();
        
        // Refresh GameManager to find all new boxes and monsters
        RefreshGameManager();

        Debug.Log("First Level generated successfully! Welcome to Lumen-Shift!");
    }
    
    /// <summary>
    /// Ensure GameManager exists in the scene (needed for SHIFT key toggle)
    /// </summary>
    void EnsureGameManagerExists()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            GameObject gmObject = new GameObject("GameManager");
            gameManager = gmObject.AddComponent<GameManager>();
            Debug.Log("GameManager created automatically - SHIFT key should now work!");
        }
    }
    
    /// <summary>
    /// Setup InputSystemFixer to ensure all EventSystems use Input System
    /// </summary>
    void SetupInputSystemFixer()
    {
        // Check if InputSystemFixer already exists
        InputSystemFixer existingFixer = FindFirstObjectByType<InputSystemFixer>();
        if (existingFixer == null)
        {
            GameObject fixerObj = new GameObject("InputSystemFixer");
            fixerObj.AddComponent<InputSystemFixer>();
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
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                bgController.gameManager = gm;
                // Immediately set the background color based on current day/night state
                bgController.UpdateBackground(gm.isDayTime);
            }
        }
    }
    
    /// <summary>
    /// Setup UI counter for lightning bolts in top right corner (attached to camera)
    /// </summary>
    void SetupLightningBoltCounter()
    {
        // Check if counter already exists
        LightningBoltCounter existingCounter = FindFirstObjectByType<LightningBoltCounter>();
        if (existingCounter == null)
        {
            // Attach to camera so it follows the camera view
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
    
    /// <summary>
    /// Setup death screen for respawn functionality
    /// </summary>
    void SetupDeathScreen()
    {
        // Check if death screen already exists
        DeathScreen existingDeathScreen = FindFirstObjectByType<DeathScreen>();
        if (existingDeathScreen == null)
        {
            GameObject deathScreenObj = new GameObject("DeathScreen");
            DeathScreen deathScreen = deathScreenObj.AddComponent<DeathScreen>();
        }
    }
    
    /// <summary>
    /// Setup clouds in the sky that change color with day/night
    /// </summary>
    void SetupClouds()
    {
        // Check if cloud controller already exists
        CloudController existingController = FindFirstObjectByType<CloudController>();
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
        GameManager gameManager = FindFirstObjectByType<GameManager>();
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
            // Zoom in 2x by reducing camera size to half (makes player appear 2x bigger)
            float aspectRatio = mainCamera.aspect;
            float desiredHalfWidth = visibleBlocks * 0.5f; // Half of visible blocks
            float calculatedSize = desiredHalfWidth / aspectRatio;
            
            // Make camera 2x closer (37.5% of calculated size = 2x zoom)
            // Original was 75%, now 37.5% = 2x zoom
            float zoomedInSize = calculatedSize * 0.375f;
            
            // Reduce ground visibility - show only 1/4 of current screen (1/2 of current ground view)
            // Adjust camera to show less ground by reducing vertical view
            float groundReduction = 0.5f; // Show half the ground (which is 1/4 of screen)
            zoomedInSize = zoomedInSize * groundReduction;
            
            cameraFollow.cameraSize = zoomedInSize;
            mainCamera.orthographicSize = zoomedInSize;
            
            // Calculate camera bounds - level starts at player spawn (x=1) and ends at x=totalFloorBlocks + extra blocks
            float playerStartX = 1f * blockSize; // Player spawns here - this is where level "starts" visually
            int blocksPerOriginal = 10;
            float subBlockSize = blockSize / blocksPerOriginal;
            float levelEndX = (totalFloorBlocks * blockSize) + (blocksPerOriginal * subBlockSize);
            float cameraHalfWidth = zoomedInSize * aspectRatio;
            
            // Set camera bounds to prevent showing empty space
            // Left bound: camera center can't go below playerStartX (so left edge is at playerStartX - cameraHalfWidth)
            // But we want to start at playerStartX, so minX should allow camera to center on playerStartX
            // Right bound: camera center can't go above levelEndX - cameraHalfWidth (so right edge is at levelEndX)
            cameraFollow.useBounds = true;
            cameraFollow.minX = playerStartX; // Can't go left of player start position
            cameraFollow.maxX = levelEndX - cameraHalfWidth; // Right edge of level
            cameraFollow.minY = -2f;
            cameraFollow.maxY = 10f;
            
            // Set initial camera position to player start position (centered on player)
            // Camera should center on player at start
            float initialCameraX = playerStartX;
            if (playerInstance != null)
            {
                // Start camera centered on player
                initialCameraX = playerInstance.transform.position.x;
                // Clamp to bounds (but should be fine since player starts at playerStartX)
                initialCameraX = Mathf.Clamp(
                    initialCameraX,
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
        // Also ensure floor covers player spawn position (x=1) and beyond
        float leftmostX = -5f * blockSize; // Start 5 blocks before 0 to ensure complete coverage
        float playerStartX = 1f * blockSize; // Player spawns at x=1
        float fillUntilX = playerStartX + (2f * blockSize); // Fill until 2 blocks after player start to ensure no gap
        int leftBlockIndex = 0;
        
        // Fill from leftmostX to fillUntilX to ensure no gap at start
        for (float x = leftmostX; x <= fillUntilX; x += subBlockSize)
        {
            // Use startY for leftmost blocks (no incline yet, or very slight incline)
            float progress = Mathf.Max(0f, (x - leftmostX) / (fillUntilX - leftmostX + 1f));
            float baseY = startY + (progress * totalHeightDiff * 0.1f); // Very slight incline before main area
            
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
        // Start from where we left off to ensure continuity
        float lastBlockX = fillUntilX;
        
        // Calculate starting x index for main loop (start from 0, but skip blocks we already filled)
        int startXIndex = 0;
        
        for (int x = startXIndex; x < totalFloorBlocks; x++)
        {
            // For each original block position, create 10 sub-blocks
            for (int subX = 0; subX < blocksPerOriginal; subX++)
            {
                float blockX = (x * blockSize) + (subX * subBlockSize);
                
                // Skip blocks we already filled in the leftmost section
                if (blockX <= fillUntilX)
                {
                    lastBlockX = blockX; // Update lastBlockX to maintain continuity
                    continue;
                }
                
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
    /// LEVEL DESIGN: Minimal blocks during day, many blocks during night
    /// Level is completable only at night
    /// </summary>
    void CreatePlatforms()
    {
        // Create 10x more blocks - each platform block becomes 10 smaller blocks
        int densityMultiplier = 10;
        float subBlockSize = blockSize / densityMultiplier;
        
        // DAY-ONLY BLOCKS: More blocks during day, but still incomplete path
        // These create a path that cannot be completed during day (requires night blocks)
        
        // Day platform 1: Starting area - extended
        for (int i = 0; i < 3 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((1f + i * subBlockSize) * blockSize, 1.0f, 0f), visibleDuringDay: true);
        }
        
        // Day platform 2: First jump area
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((8f + i * subBlockSize) * blockSize, 1.2f, 0f), visibleDuringDay: true);
        }
        
        // Day platform 3: Gap area
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((15f + i * subBlockSize) * blockSize, 1.3f, 0f), visibleDuringDay: true);
        }
        
        // Day platform 4: Mid-level
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((25f + i * subBlockSize) * blockSize, 1.5f, 0f), visibleDuringDay: true);
        }
        
        // Day platform 5: Further ahead
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((35f + i * subBlockSize) * blockSize, 1.4f, 0f), visibleDuringDay: true);
        }
        
        // Day platform 6: Even further
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((45f + i * subBlockSize) * blockSize, 1.6f, 0f), visibleDuringDay: true);
        }
        
        // Day platform 7: Late game
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((60f + i * subBlockSize) * blockSize, 1.5f, 0f), visibleDuringDay: true);
        }
        
        // Day platform 8: Near end (but gap prevents completion)
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((70f + i * subBlockSize) * blockSize, 1.25f, 0f), visibleDuringDay: true);
        }
        
        // NIGHT-ONLY BLOCKS: Complete path from start to finish
        // These make the level fully playable and completable at night
        
        // Night path from start: Continuous ground path - Lowered significantly
        for (int i = 0; i < 5 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((1f + i * subBlockSize) * blockSize, 1.0f, 0f), visibleDuringDay: false);
        }
        
        // Night bridge 1: First major bridge - Lowered significantly
        for (int i = 0; i < 8 * densityMultiplier; i++)
        {
            float x = 6f + i * subBlockSize;
            float y = 1.0f + (i * 0.05f); // Slight incline (reduced)
            CreateBoxBlock(new Vector3(x * blockSize, y, 0f), visibleDuringDay: false);
        }
        
        // Night platform 1: Landing area - Lowered significantly
        for (int i = 0; i < 3 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((14f + i * subBlockSize) * blockSize, 1.4f, 0f), visibleDuringDay: false);
        }
        
        // Night staircase 1: Upward climb - Lowered significantly
        for (int i = 0; i < 8 * densityMultiplier; i++)
        {
            float x = 17f + i * subBlockSize;
            float y = 1.4f + (i * 0.1f); // Steady climb (reduced)
            CreateBoxBlock(new Vector3(x * blockSize, y, 0f), visibleDuringDay: false);
        }
        
        // Night platform 2: High landing - Lowered significantly
        for (int i = 0; i < 4 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((25f + i * subBlockSize) * blockSize, 2.2f, 0f), visibleDuringDay: false);
        }
        
        // Night bridge 2: Long bridge across gap - Lowered significantly
        for (int i = 0; i < 12 * densityMultiplier; i++)
        {
            float x = 29f + i * subBlockSize;
            float y = 2.2f - (i * 0.025f); // Slight decline (reduced)
            CreateBoxBlock(new Vector3(x * blockSize, y, 0f), visibleDuringDay: false);
        }
        
        // Night platform 3: Mid-level landing - Lowered significantly
        for (int i = 0; i < 3 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((41f + i * subBlockSize) * blockSize, 1.9f, 0f), visibleDuringDay: false);
        }
        
        // Night staircase 2: Another climb - Lowered significantly
        for (int i = 0; i < 6 * densityMultiplier; i++)
        {
            float x = 44f + i * subBlockSize;
            float y = 1.9f + (i * 0.075f); // Moderate climb (reduced)
            CreateBoxBlock(new Vector3(x * blockSize, y, 0f), visibleDuringDay: false);
        }
        
        // Night platform 4: High point - Lowered significantly
        for (int i = 0; i < 4 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((50f + i * subBlockSize) * blockSize, 2.35f, 0f), visibleDuringDay: false);
        }
        
        // Night bridge 3: Long descent bridge - Lowered significantly
        for (int i = 0; i < 15 * densityMultiplier; i++)
        {
            float x = 54f + i * subBlockSize;
            float y = 2.35f - (i * 0.04f); // Gradual descent (reduced)
            CreateBoxBlock(new Vector3(x * blockSize, y, 0f), visibleDuringDay: false);
        }
        
        // Night platform 5: Lower landing - Lowered significantly
        for (int i = 0; i < 3 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((69f + i * subBlockSize) * blockSize, 1.75f, 0f), visibleDuringDay: false);
        }
        
        // Night bridge 4: Final bridge to finish - Lowered significantly
        for (int i = 0; i < 20 * densityMultiplier; i++)
        {
            float x = 72f + i * subBlockSize;
            float y = 1.75f - (i * 0.015f); // Very gradual descent (reduced)
            CreateBoxBlock(new Vector3(x * blockSize, y, 0f), visibleDuringDay: false);
        }
        
        // Night: Additional support platforms for easier navigation - All lowered significantly
        // Support platform 1
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((10f + i * subBlockSize) * blockSize, 1.25f, 0f), visibleDuringDay: false);
        }
        
        // Support platform 2
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((20f + i * subBlockSize) * blockSize, 1.6f, 0f), visibleDuringDay: false);
        }
        
        // Support platform 3
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((33f + i * subBlockSize) * blockSize, 2.1f, 0f), visibleDuringDay: false);
        }
        
        // Support platform 4
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((47f + i * subBlockSize) * blockSize, 2.25f, 0f), visibleDuringDay: false);
        }
        
        // Support platform 5
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((62f + i * subBlockSize) * blockSize, 1.9f, 0f), visibleDuringDay: false);
        }
        
        // Support platform 6
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((80f + i * subBlockSize) * blockSize, 1.6f, 0f), visibleDuringDay: false);
        }
    }

    /// <summary>
    /// Place lightning bolts throughout the level
    /// Lightning bolts must be:
    /// - Within 2 blocks of at least one night block (reachable)
    /// - At least 5 blocks away from all day blocks (not reachable during day)
    /// </summary>
    void PlaceLightningBolts()
    {
        // First, collect all block positions
        List<Vector3> nightBlockPositions = new List<Vector3>();
        List<Vector3> dayBlockPositions = new List<Vector3>();
        
        // Find all blocks in the scene
        BoxBlock[] allBlocks = FindObjectsByType<BoxBlock>(FindObjectsSortMode.None);
        foreach (BoxBlock block in allBlocks)
        {
            Vector3 blockPos = block.transform.position;
            if (block.visibleDuringDay)
            {
                dayBlockPositions.Add(blockPos);
            }
            else
            {
                nightBlockPositions.Add(blockPos);
            }
        }
        
        // If no blocks found, use calculated positions based on CreatePlatforms layout
        if (nightBlockPositions.Count == 0 && dayBlockPositions.Count == 0)
        {
            // Fallback: calculate positions from known layout
            CalculateBlockPositions(out nightBlockPositions, out dayBlockPositions);
        }
        
        // Distance constraints
        float minDistanceToNightBlock = 0.5f * blockSize; // At least 0.5 blocks from night block
        float maxDistanceToNightBlock = 2f * blockSize; // At most 2 blocks from night block
        float minDistanceToDayBlock = 5f * blockSize; // At least 5 blocks from day block
        
        // Try to place 12 lightning bolts, spread out evenly
        int boltsPlaced = 0;
        List<Vector3> placedBoltPositions = new List<Vector3>(); // Track placed bolts to avoid clustering
        float minDistanceBetweenBolts = 8f * blockSize; // Minimum distance between bolts to spread them out
        
        // Candidate positions near night blocks - prioritize positions above blocks
        List<Vector3> candidatePositions = new List<Vector3>();
        
        // Generate candidate positions above night blocks (not all around, just above)
        foreach (Vector3 nightBlockPos in nightBlockPositions)
        {
            // Try positions above the block at different heights
            for (float height = 1.0f; height <= 2.5f; height += 0.3f)
            {
                Vector3 candidatePos = nightBlockPos + new Vector3(0f, height, 0f);
                candidatePositions.Add(candidatePos);
            }
            
            // Also try slightly to the left and right
            for (float offset = -1.5f; offset <= 1.5f; offset += 0.5f)
            {
                for (float height = 1.2f; height <= 2.0f; height += 0.3f)
                {
                    Vector3 candidatePos = nightBlockPos + new Vector3(offset * blockSize, height, 0f);
                    candidatePositions.Add(candidatePos);
                }
            }
        }
        
        // Shuffle candidates to avoid patterns
        System.Random rng = new System.Random(42);
        for (int i = candidatePositions.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            Vector3 temp = candidatePositions[i];
            candidatePositions[i] = candidatePositions[j];
            candidatePositions[j] = temp;
        }
        
        // Try each candidate position
        foreach (Vector3 candidatePos in candidatePositions)
        {
            if (boltsPlaced >= 12) break;
            
            // Check distance to nearest night block
            float minDistToNight = float.MaxValue;
            foreach (Vector3 nightPos in nightBlockPositions)
            {
                float dist = Vector3.Distance(candidatePos, nightPos);
                if (dist < minDistToNight)
                {
                    minDistToNight = dist;
                }
            }
            
            // Check distance to nearest day block
            float minDistToDay = float.MaxValue;
            foreach (Vector3 dayPos in dayBlockPositions)
            {
                float dist = Vector3.Distance(candidatePos, dayPos);
                if (dist < minDistToDay)
                {
                    minDistToDay = dist;
                }
            }
            
            // Check distance to other placed bolts (to avoid clustering)
            bool tooCloseToOtherBolts = false;
            foreach (Vector3 placedPos in placedBoltPositions)
            {
                float dist = Vector3.Distance(candidatePos, placedPos);
                if (dist < minDistanceBetweenBolts)
                {
                    tooCloseToOtherBolts = true;
                    break;
                }
            }
            
            // Check if position satisfies all conditions
            bool withinNightRange = minDistToNight >= minDistanceToNightBlock && minDistToNight <= maxDistanceToNightBlock;
            bool farFromDayBlocks = minDistToDay >= minDistanceToDayBlock;
            bool spreadOut = !tooCloseToOtherBolts;
            
            // CRITICAL: Check if position is NOT inside any block (must have background behind)
            bool notInsideBlock = !IsPositionInsideBlock(candidatePos);
            
            if (withinNightRange && farFromDayBlocks && spreadOut && notInsideBlock)
            {
                CreateLightningBolt(candidatePos);
                placedBoltPositions.Add(candidatePos);
                boltsPlaced++;
            }
        }
        
        // If we didn't place enough bolts, use fallback positions
        if (boltsPlaced < 12)
        {
            Debug.LogWarning($"Only placed {boltsPlaced}/12 lightning bolts. Using fallback positions.");
            PlaceLightningBoltsFallback(nightBlockPositions, dayBlockPositions);
        }
    }
    
    /// <summary>
    /// Calculate block positions from known layout (fallback if blocks not found in scene)
    /// </summary>
    void CalculateBlockPositions(out List<Vector3> nightBlocks, out List<Vector3> dayBlocks)
    {
        nightBlocks = new List<Vector3>();
        dayBlocks = new List<Vector3>();
        
        int densityMultiplier = 10;
        float subBlockSize = blockSize / densityMultiplier;
        
        // Day blocks (updated to match new layout)
        for (int i = 0; i < 3 * densityMultiplier; i++)
        {
            dayBlocks.Add(new Vector3((1f + i * subBlockSize) * blockSize, 1.0f, 0f));
        }
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            dayBlocks.Add(new Vector3((8f + i * subBlockSize) * blockSize, 1.2f, 0f));
        }
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            dayBlocks.Add(new Vector3((15f + i * subBlockSize) * blockSize, 1.3f, 0f));
        }
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            dayBlocks.Add(new Vector3((25f + i * subBlockSize) * blockSize, 1.5f, 0f));
        }
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            dayBlocks.Add(new Vector3((35f + i * subBlockSize) * blockSize, 1.4f, 0f));
        }
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            dayBlocks.Add(new Vector3((45f + i * subBlockSize) * blockSize, 1.6f, 0f));
        }
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            dayBlocks.Add(new Vector3((60f + i * subBlockSize) * blockSize, 1.5f, 0f));
        }
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            dayBlocks.Add(new Vector3((70f + i * subBlockSize) * blockSize, 1.25f, 0f));
        }
        
        // Night blocks (sample key positions)
        nightBlocks.Add(new Vector3(2f * blockSize, 1.0f, 0f)); // Night path
        nightBlocks.Add(new Vector3(8f * blockSize, 1.1f, 0f)); // Night bridge 1
        nightBlocks.Add(new Vector3(15f * blockSize, 1.4f, 0f)); // Night platform 1
        nightBlocks.Add(new Vector3(20f * blockSize, 1.6f, 0f)); // Night staircase 1
        nightBlocks.Add(new Vector3(27f * blockSize, 2.2f, 0f)); // Night platform 2
        nightBlocks.Add(new Vector3(35f * blockSize, 2.15f, 0f)); // Night bridge 2
        nightBlocks.Add(new Vector3(42f * blockSize, 1.9f, 0f)); // Night platform 3
        nightBlocks.Add(new Vector3(47f * blockSize, 2.0f, 0f)); // Night staircase 2
        nightBlocks.Add(new Vector3(52f * blockSize, 2.35f, 0f)); // Night platform 4
        nightBlocks.Add(new Vector3(60f * blockSize, 2.1f, 0f)); // Night bridge 3
        nightBlocks.Add(new Vector3(70f * blockSize, 1.75f, 0f)); // Night platform 5
        nightBlocks.Add(new Vector3(80f * blockSize, 1.7f, 0f)); // Night bridge 4
    }
    
    /// <summary>
    /// Fallback method to place lightning bolts if automatic placement fails
    /// Spreads them out evenly along the night path, far from day blocks
    /// </summary>
    void PlaceLightningBoltsFallback(List<Vector3> nightBlocks, List<Vector3> dayBlocks)
    {
        int blocksPerOriginal = 10;
        float subBlockSize = blockSize / blocksPerOriginal;
        
        // Place bolts spread out along the night path, ensuring they're:
        // 1. Near night blocks (within 2 blocks)
        // 2. Far from day blocks (at least 5 blocks away)
        // 3. Spread out evenly (not clumped together)
        
        // Calculate positions that are spread out along the level
        float[] boltXPositions = { 2.5f, 7f, 13f, 18f, 23f, 32f, 39f, 45f, 49f, 58f, 65f, 75f };
        float[] boltYOffsets = { 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f };
        
        // Find corresponding night block Y positions
        for (int i = 0; i < boltXPositions.Length && i < 12; i++)
        {
            float boltX = boltXPositions[i] * blockSize;
            float boltY = 1.0f + boltYOffsets[i]; // Default height
            
            // Find nearest night block to get proper Y position
            float nearestNightY = 1.0f;
            float minDist = float.MaxValue;
            foreach (Vector3 nightPos in nightBlocks)
            {
                float dist = Mathf.Abs(nightPos.x - boltX);
                if (dist < minDist && dist < 5f * blockSize)
                {
                    minDist = dist;
                    nearestNightY = nightPos.y;
                }
            }
            
            // Check if far enough from day blocks
            bool tooCloseToDay = false;
            foreach (Vector3 dayPos in dayBlocks)
            {
                float dist = Vector3.Distance(new Vector3(boltX, boltY, 0f), dayPos);
                if (dist < 5f * blockSize)
                {
                    tooCloseToDay = true;
                    break;
                }
            }
            
            // Only place if far from day blocks AND not inside any block
            if (!tooCloseToDay)
            {
                boltY = nearestNightY + 1.5f; // Above the night block
                Vector3 boltPos = new Vector3(boltX, boltY, 0f);
                
                // Check if position is not inside any block
                if (!IsPositionInsideBlock(boltPos))
                {
                    CreateLightningBolt(boltPos);
                }
            }
        }
    }
    
    /// <summary>
    /// Place monsters (death points) on night-only platforms
    /// These are obstacles the player must avoid (only appear at night)
    /// </summary>
    void PlaceMonsters()
    {
        // Place monsters in the MIDDLE of night paths where players will definitely encounter them
        // Position them on top of night blocks, in the center of platforms/bridges
        
        // Monster 1: Middle of night bridge 1 (player must cross this)
        CreateMonster(new Vector3(9f * blockSize, 1.15f, 0f));
        
        // Monster 2: Middle of night platform 1 (landing area - player will land here)
        CreateMonster(new Vector3(15.5f * blockSize, 1.55f, 0f));
        
        // Monster 3: Middle of night staircase 1 (player climbing up)
        CreateMonster(new Vector3(20f * blockSize, 1.7f, 0f));
        
        // Monster 4: Middle of night platform 2 (high landing - player must pass)
        CreateMonster(new Vector3(27f * blockSize, 2.35f, 0f));
        
        // Monster 5: Middle of night bridge 2 (long bridge - player must cross)
        CreateMonster(new Vector3(35f * blockSize, 2.1f, 0f));
        
        // Monster 6: Middle of night platform 3 (mid-level landing)
        CreateMonster(new Vector3(42f * blockSize, 2.05f, 0f));
        
        // Monster 7: Middle of night staircase 2 (player climbing)
        CreateMonster(new Vector3(47f * blockSize, 2.15f, 0f));
        
        // Monster 8: Middle of night platform 4 (high point)
        CreateMonster(new Vector3(52f * blockSize, 2.5f, 0f));
        
        // Monster 9: Middle of night bridge 3 (long descent bridge)
        CreateMonster(new Vector3(60f * blockSize, 2.15f, 0f));
        
        // Monster 10: Middle of night platform 5 (lower landing)
        CreateMonster(new Vector3(70f * blockSize, 1.9f, 0f));
        
        // Monster 11: Middle of night bridge 4 (final bridge to finish)
        CreateMonster(new Vector3(80f * blockSize, 1.7f, 0f));
    }
    
    /// <summary>
    /// Check if a position is inside any block (lightning bolts must have background behind)
    /// </summary>
    bool IsPositionInsideBlock(Vector3 position)
    {
        // Check collision with all blocks
        Collider2D[] overlaps = Physics2D.OverlapPointAll(position);
        foreach (Collider2D col in overlaps)
        {
            // If it overlaps with a block (not a trigger), it's inside a block
            if (col != null && !col.isTrigger)
            {
                if (col.GetComponent<BoxBlock>() != null || col.GetComponent<FloorBlock>() != null)
                {
                    return true; // Position is inside a block
                }
            }
        }
        return false; // Position has background behind it
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
        
        // Scale block to 50% size
        block.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        
        // IMPORTANT: Ensure collider is properly configured for collision
        BoxCollider2D blockCollider = block.GetComponent<BoxCollider2D>();
        if (blockCollider != null)
        {
            blockCollider.isTrigger = false; // Must be solid for collision
            blockCollider.enabled = true;
            // Collider size will be auto-adjusted by BoxBlock.SetupBox() based on sprite
            // But we need to ensure it accounts for the scale
            if (blockCollider.size.x > 0 && blockCollider.size.y > 0)
            {
                // Collider size is already set by BoxBlock, but scale affects it
                // The collider will automatically scale with the transform
            }
        }
    }

    void CreateLightningBolt(Vector3 position)
    {
        if (lightningBoltPrefab != null)
        {
            GameObject bolt = Instantiate(lightningBoltPrefab, position, Quaternion.identity);
            bolt.transform.SetParent(itemsParent);
            // Scale lightning bolt to 50% of its original size
            bolt.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        }
        else
        {
            GameObject bolt = new GameObject("LightningBolt");
            bolt.transform.position = position;
            bolt.transform.SetParent(itemsParent);
            
            // Scale lightning bolt to 50% of its original size
            bolt.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            
            LightningBolt lightningBolt = bolt.AddComponent<LightningBolt>();
        }
    }
    
    void CreateMonster(Vector3 position)
    {
        GameObject monster = new GameObject("Monster");
        monster.transform.position = position;
        monster.transform.SetParent(itemsParent);
        
        Monster monsterComponent = monster.AddComponent<Monster>();
        monsterComponent.monsterColor = new Color(0.8f, 0.2f, 0.2f); // Red
        monsterComponent.size = 0.8f; // Slightly smaller than a block
        // Note: Monster handles death through GameManager, no need for restartLevelOnDeath or levelSceneName
    }
    
    void CreateFinishLine(Vector3 position)
    {
        GameObject finishLine = new GameObject("FinishLine");
        finishLine.transform.position = position;
        finishLine.transform.SetParent(itemsParent);
        
        FinishLine finish = finishLine.AddComponent<FinishLine>();
        finish.requireAllBolts = true; // Player must collect all bolts to win
        finish.nextLevelSceneName = ""; // Leave empty to return to menu, or set to next level scene name
    }

    void CreatePlayer(Vector3 position)
    {
        // First, check if player prefab is assigned - use it if available
        if (playerPrefab != null)
        {
            // Destroy any existing player first
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                DestroyImmediate(existingPlayer);
            }
            
            // Instantiate player from prefab
            playerInstance = Instantiate(playerPrefab, position, Quaternion.identity);
            playerInstance.tag = "Player";
            return;
        }
        
        // Fallback: Find existing player or create new one
        GameObject existingPlayerObj = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayerObj != null)
        {
            playerInstance = existingPlayerObj;
            playerInstance.transform.position = position;
            
            // Ensure existing player has the correct sprite
            SpriteRenderer existingSr = playerInstance.GetComponent<SpriteRenderer>();
            if (existingSr != null)
            {
                // If sprite is not set or is the default, try to set the Jump sprite
                if (existingSr.sprite == null || existingSr.sprite.name.Contains("Default") || existingSr.sprite.name == "New Sprite")
                {
                    Sprite correctSprite = GetPlayerSprite();
                    if (correctSprite != null)
                    {
                        existingSr.sprite = correctSprite;
                        Debug.Log($"Updated existing player sprite to: {correctSprite.name}");
                    }
                }
            }
        }
        else
        {
            // Create player GameObject from scratch with all components
            playerInstance = CreatePlayerFromScratch(position);
        }
    }

    /// <summary>
    /// Get the player sprite (Jump sprite from Virtual Guy)
    /// </summary>
    Sprite GetPlayerSprite()
    {
        // First, check if sprite is assigned in Inspector
        if (playerSprite != null)
        {
            return playerSprite;
        }
        
        // Try to load "Jump" sprite from PixelAdventure folder
        // Method 1: Try Resources.Load with multiple paths
        string[] possiblePaths = {
            "PixelAdventure/Main Characters/Virtual Guy/Jump",
            "Virtual Guy/Jump",
            "Jump"
        };
        
        foreach (string path in possiblePaths)
        {
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                return sprite;
            }
        }
        
        // Method 2: Find by name from all loaded sprites
        Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
        Sprite bestMatch = null;
        
        foreach (Sprite s in allSprites)
        {
            if (s.name.Contains("Jump"))
            {
                if (s.name.Contains("32x32"))
                {
                    return s; // Preferred format
                }
                if (bestMatch == null)
                {
                    bestMatch = s;
                }
            }
        }
        
        // Method 3: Try using AssetDatabase in editor with exact path
        #if UNITY_EDITOR
        // Try multiple path variations
        string[] pathVariations = {
            "Assets/PixelAdventure/Main Characters/Virtual Guy/Jump.asset",
            "Assets/PixelAdventure/Main Characters/Virtual Guy/Jump",
            "Assets/PixelAdventure/Main Characters/Virtual Guy/Jump (32x32).asset",
            "Assets/PixelAdventure/Main Characters/Virtual Guy/Jump (32x32)"
        };
        
        foreach (string path in pathVariations)
        {
            Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                Debug.Log($"✓ Found sprite at path: {path}");
                return sprite;
            }
        }
        
        // Search in Virtual Guy folder - find all sprites with "Jump" in name
        string[] guids = UnityEditor.AssetDatabase.FindAssets("Jump", new[] { "Assets/PixelAdventure/Main Characters/Virtual Guy" });
        Debug.Log($"Searching for Jump sprite in Virtual Guy folder, found {guids.Length} assets");
        
        foreach (string guid in guids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            Debug.Log($"Checking asset: {assetPath}");
            
            // Try loading as Sprite
            Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null)
            {
                Debug.Log($"✓ Found sprite: {assetPath} (name: {sprite.name})");
                return sprite;
            }
            
            // If it's a texture, try loading it
            Texture2D texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture != null && assetPath.Contains("Jump"))
            {
                Debug.Log($"Found texture (not sprite): {assetPath}");
            }
        }
        
        // Broader search in PixelAdventure - find all Jump sprites
        guids = UnityEditor.AssetDatabase.FindAssets("Jump t:Sprite", new[] { "Assets/PixelAdventure" });
        Debug.Log($"Broad search found {guids.Length} Jump sprites in PixelAdventure");
        
        foreach (string guid in guids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.Contains("Virtual Guy"))
            {
                Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite != null)
                {
                    Debug.Log($"✓ Found sprite via broad search: {assetPath}");
                    return sprite;
                }
            }
        }
        
        Debug.LogWarning("Could not find Jump sprite using AssetDatabase. Please assign it manually in Inspector.");
        #endif
        
        return bestMatch;
    }
    
    /// <summary>
    /// Create player GameObject from scratch with all necessary components
    /// Automatically loads the "Jump" sprite from PixelAdventure if available
    /// </summary>
    GameObject CreatePlayerFromScratch(Vector3 position)
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = position;
        
        // Add SpriteRenderer
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        
        // Try to load the sprite
        Sprite spriteToUse = GetPlayerSprite();
        
        // Log sprite finding result
        if (spriteToUse != null)
        {
            Debug.Log($"Using player sprite: {spriteToUse.name}");
        }
        else
        {
            Debug.LogWarning("Player sprite not found! Will use default sprite from PlayerController. " +
                "Please assign the 'Jump' sprite to the 'Player Sprite' field in FirstLevelGenerator Inspector.");
        }
        
        // Set sprite BEFORE adding PlayerController to ensure it's preserved
        if (spriteToUse != null)
        {
            sr.sprite = spriteToUse;
            Debug.Log($"Sprite assigned to player: {spriteToUse.name}");
        }
        // If no sprite found, PlayerController will create a default one
        
        sr.color = Color.white;
        sr.sortingOrder = 1;
        sr.drawMode = SpriteDrawMode.Simple;
        
        // Add Rigidbody2D
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 3f;
        
        // Add Collider2D - use BoxCollider2D to match sprite exactly
        BoxCollider2D playerCollider = player.AddComponent<BoxCollider2D>();
        playerCollider.isTrigger = false;
        playerCollider.usedByEffector = false;
        
        // Auto-size collider to match sprite EXACTLY
        if (spriteToUse != null)
        {
            Vector2 spriteSize = spriteToUse.bounds.size;
            playerCollider.size = spriteSize; // Exact match to sprite size
        }
        else
        {
            // Default size if no sprite (will be updated by PlayerController)
            playerCollider.size = new Vector2(0.875f, 0.5625f);
        }
        
        // Ensure rotation is correct (facing right, not sideways)
        player.transform.rotation = Quaternion.identity; // No rotation
        
        // Add PlayerController (this will set up additional things in Awake)
        PlayerController controller = player.AddComponent<PlayerController>();
        
        // Add PlayerAnimationController for animations
        PlayerAnimationController animController = player.AddComponent<PlayerAnimationController>();
        
        // IMPORTANT: Ensure sprite is set AFTER PlayerController is added
        // Use a coroutine to set sprite after all Awake() methods complete
        if (spriteToUse != null)
        {
            StartCoroutine(SetSpriteAfterInitialization(player, spriteToUse));
        }
        
        return player;
    }
    
    /// <summary>
    /// Coroutine to set sprite after all components are initialized
    /// </summary>
    IEnumerator SetSpriteAfterInitialization(GameObject playerObj, Sprite sprite)
    {
        // Wait for end of frame to ensure all Awake() methods have completed
        yield return new WaitForEndOfFrame();
        
        if (playerObj != null)
        {
            SpriteRenderer finalSr = playerObj.GetComponent<SpriteRenderer>();
            if (finalSr != null)
            {
                finalSr.sprite = sprite;
                Debug.Log($"Sprite set after initialization: {sprite.name}");
            }
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



