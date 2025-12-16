using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Generates Level 5 - Final Challenge
/// The ultimate test with extreme vertical challenges and precision
/// </summary>
public class Level11Generator : MonoBehaviour
{
    [Header("Level Settings")]
    public float blockSize = 1f;
    public int totalFloorBlocks = 130; // Level 11: 130 blocks
    public int visibleBlocks = 16;
    
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

    void Start()
    {
        // Check if LevelManager wants to control generation
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null && levelManager.currentLevel != 11)
        {
            // LevelManager will handle generation, don't auto-generate
            gameObject.SetActive(false);
            return;
        }
        
        if (generateOnStart)
        {
            GenerateLevel11();
        }
    }

    public void GenerateLevel11()
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
        CreateGroundFloor();
        CreatePlatforms();
        PlaceLightningBolts();
        PlaceMonsters();
        PlaceFinishLine();

        float playerStartX = 1f * blockSize;
        float startY = -0.5f;
        float endY = 0.5f;
        float leftmostX = -5f * blockSize;
        float fillUntilX = playerStartX + (2f * blockSize);
        float progress = Mathf.Max(0f, (playerStartX - leftmostX) / (fillUntilX - leftmostX + 1f));
        float baseY = startY + (progress * (endY - startY) * 0.1f);
        float grassHeight = 0.1f;
        float playerSpawnY = (baseY + grassHeight) * blockSize + 0.5f;
        CreatePlayer(new Vector3(playerStartX, playerSpawnY, 0f));

        SetupCamera();
        EnsureGameManagerExists();
        SetupInputSystemFixer();
        SetupBackgroundController();
        SetupLightningBoltCounter();
        SetupDeathScreen();
        SetupWinnerScreen();
        SetupBlockManager();
        SetupTimeOfDayController();
        SetupClouds();
        RefreshGameManager();

        Debug.Log("Level 11 generated successfully!");
    }

    void CreatePlatforms()
    {
        int densityMultiplier = 10;
        float subBlockSize = blockSize / densityMultiplier;
        
        // DAY-ONLY BLOCKS: Extreme vertical challenges - very few platforms
        // Day platform 1: Starting area (very small)
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((1f + i * subBlockSize) * blockSize, 1.0f, 0f), visibleDuringDay: true);
        }
        
        // Day platform 2: Very high isolated platform
        for (int i = 0; i < 1 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((25f + i * subBlockSize) * blockSize, 4.5f, 0f), visibleDuringDay: true);
        }
        
        // Day platform 3: Another high platform
        for (int i = 0; i < 1 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((60f + i * subBlockSize) * blockSize, 5.0f, 0f), visibleDuringDay: true);
        }
        
        // Day platform 4: Near end
        for (int i = 0; i < 1 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((120f + i * subBlockSize) * blockSize, 1.2f, 0f), visibleDuringDay: true);
        }
        
        // NIGHT-ONLY BLOCKS: Extreme vertical path - ultimate challenge
        // Night path 1: Starting path
        for (int i = 0; i < 4 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((1f + i * subBlockSize) * blockSize, 1.0f, 0f), visibleDuringDay: false);
        }
        
        // Night tower 1: Extreme vertical climb
        for (int i = 0; i < 12; i++)
        {
            for (int j = 0; j < 2 * densityMultiplier; j++)
            {
                CreateBoxBlock(new Vector3((5f + j * subBlockSize) * blockSize, 1.0f + (i * 0.4f), 0f), visibleDuringDay: false);
            }
        }
        
        // Night platform 1: High landing
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((7f + i * subBlockSize) * blockSize, 5.8f, 0f), visibleDuringDay: false);
        }
        
        // Night bridge 1: Narrow bridge across gap
        for (int i = 0; i < 6 * densityMultiplier; i++)
        {
            float x = 9f + i * subBlockSize;
            float y = 5.8f - (i * 0.05f);
            CreateBoxBlock(new Vector3(x * blockSize, y, 0f), visibleDuringDay: false);
        }
        
        // Night tower 2: Another extreme climb
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 2 * densityMultiplier; j++)
            {
                CreateBoxBlock(new Vector3((15f + j * subBlockSize) * blockSize, 5.5f + (i * 0.45f), 0f), visibleDuringDay: false);
            }
        }
        
        // Night platform 2: Very high checkpoint
        for (int i = 0; i < 3 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((17f + i * subBlockSize) * blockSize, 10.0f, 0f), visibleDuringDay: false);
        }
        
        // Night bridge 2: Long high bridge
        for (int i = 0; i < 12 * densityMultiplier; i++)
        {
            float x = 20f + i * subBlockSize;
            float y = 10.0f - (i * 0.03f);
            CreateBoxBlock(new Vector3(x * blockSize, y, 0f), visibleDuringDay: false);
        }
        
        // Night staircase 1: Steep descent
        for (int i = 0; i < 15 * densityMultiplier; i++)
        {
            float x = 32f + i * subBlockSize;
            float y = 9.64f - (i * 0.12f);
            CreateBoxBlock(new Vector3(x * blockSize, y, 0f), visibleDuringDay: false);
        }
        
        // Night platform 3: Mid-level checkpoint
        for (int i = 0; i < 2 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((47f + i * subBlockSize) * blockSize, 7.84f, 0f), visibleDuringDay: false);
        }
        
        // Night tower 3: Third extreme climb
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 2 * densityMultiplier; j++)
            {
                CreateBoxBlock(new Vector3((49f + j * subBlockSize) * blockSize, 7.84f + (i * 0.5f), 0f), visibleDuringDay: false);
            }
        }
        
        // Night platform 4: Another high checkpoint
        for (int i = 0; i < 3 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((51f + i * subBlockSize) * blockSize, 11.84f, 0f), visibleDuringDay: false);
        }
        
        // Night bridge 3: Very long bridge
        for (int i = 0; i < 20 * densityMultiplier; i++)
        {
            float x = 54f + i * subBlockSize;
            float y = 11.84f - (i * 0.025f);
            CreateBoxBlock(new Vector3(x * blockSize, y, 0f), visibleDuringDay: false);
        }
        
        // Night staircase 2: Final descent
        for (int i = 0; i < 25 * densityMultiplier; i++)
        {
            float x = 74f + i * subBlockSize;
            float y = 11.34f - (i * 0.08f);
            CreateBoxBlock(new Vector3(x * blockSize, y, 0f), visibleDuringDay: false);
        }
        
        // Night platform 5: Final landing
        for (int i = 0; i < 30 * densityMultiplier; i++)
        {
            CreateBoxBlock(new Vector3((99f + i * subBlockSize) * blockSize, 9.34f, 0f), visibleDuringDay: false);
        }
    }

    void CreateGroundFloor()
    {
        int blockCount = 0;
        System.Random random = new System.Random(42);
        int blocksPerOriginal = 10;
        float subBlockSize = blockSize / blocksPerOriginal;
        float startY = -0.5f;
        float endY = 0.5f;
        float totalHeightDiff = endY - startY;
        
        Camera mainCamera = Camera.main;
        float cameraBottom = -5f;
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
        
        float minDirtY = Mathf.Min(cameraBottom - 1f, startY * blockSize - 2f);
        float leftmostX = -5f * blockSize;
        float playerStartX = 1f * blockSize;
        float fillUntilX = playerStartX + (2f * blockSize);
        int leftBlockIndex = 0;
        
        for (float x = leftmostX; x <= fillUntilX; x += subBlockSize)
        {
            float progress = Mathf.Max(0f, (x - leftmostX) / (fillUntilX - leftmostX + 1f));
            float baseY = startY + (progress * totalHeightDiff * 0.1f);
            
            for (float dirtY = minDirtY; dirtY < baseY * blockSize; dirtY += blockSize)
            {
                CreateFloorBlock(new Vector3(x, dirtY, 0f), FloorBlock.FloorType.Dirt);
                blockCount++;
            }
            
            CreateFloorBlock(new Vector3(x, baseY * blockSize, 0f), FloorBlock.FloorType.Dirt);
            float grassHeight = GetGrassHeight(leftBlockIndex, random);
            CreateFloorBlock(new Vector3(x, (baseY + grassHeight) * blockSize, 0f), FloorBlock.FloorType.Grass);
            blockCount += 2;
            leftBlockIndex++;
        }
        
        float lastBlockX = fillUntilX;
        
        for (int x = 0; x < totalFloorBlocks; x++)
        {
            for (int subX = 0; subX < blocksPerOriginal; subX++)
            {
                float blockX = (x * blockSize) + (subX * subBlockSize);
                
                if (blockX <= fillUntilX)
                {
                    lastBlockX = blockX;
                    continue;
                }
                
                if (blockX - lastBlockX > subBlockSize * 1.1f)
                {
                    for (float fillX = lastBlockX + subBlockSize; fillX < blockX; fillX += subBlockSize)
                    {
                        float fillProgress = (fillX - leftmostX) / ((totalFloorBlocks * blockSize) - leftmostX);
                        fillProgress = Mathf.Clamp01(fillProgress);
                        float fillBaseY = startY + (fillProgress * totalHeightDiff);
                        
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
                
                float progress = (float)(x * blocksPerOriginal + subX) / (float)(totalFloorBlocks * blocksPerOriginal);
                float baseY = startY + (progress * totalHeightDiff);
                
                for (float dirtY = minDirtY; dirtY < baseY * blockSize - blockSize * 0.1f; dirtY += blockSize)
                {
                    CreateFloorBlock(new Vector3(blockX, dirtY, 0f), FloorBlock.FloorType.Dirt);
                    blockCount++;
                }
                
                CreateFloorBlock(new Vector3(blockX, baseY * blockSize, 0f), FloorBlock.FloorType.Dirt);
                float grassHeight = GetGrassHeight(x * blocksPerOriginal + subX, random);
                CreateFloorBlock(new Vector3(blockX, (baseY + grassHeight) * blockSize, 0f), FloorBlock.FloorType.Grass);
                blockCount += 2;
                lastBlockX = blockX;
            }
        }
        
        float rightmostX = totalFloorBlocks * blockSize;
        
        if (rightmostX - lastBlockX > subBlockSize * 1.1f)
        {
            for (float fillX = lastBlockX + subBlockSize; fillX < rightmostX; fillX += subBlockSize)
            {
                float fillProgress = (fillX - leftmostX) / ((totalFloorBlocks * blockSize) - leftmostX);
                fillProgress = Mathf.Clamp01(fillProgress);
                float fillBaseY = startY + (fillProgress * totalHeightDiff);
                
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
        
        for (int i = 0; i < blocksPerOriginal; i++)
        {
            float blockX = rightmostX + (i * subBlockSize);
            float progress = 1f;
            float baseY = startY + (progress * totalHeightDiff);
            
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
    }

    float GetGrassHeight(int x, System.Random random)
    {
        float wave1 = Mathf.Sin(x * 0.25f) * 0.12f;
        float wave2 = Mathf.Sin(x * 0.7f) * 0.08f;
        float wave3 = Mathf.Sin(x * 1.5f) * 0.05f;
        float randomSeed = (x * 17 + 23) % 100;
        float randomVariation = (randomSeed / 100f - 0.5f) * 0.15f;
        float baseHeight = 0.3f;
        float height = baseHeight + wave1 + wave2 + wave3 + randomVariation;
        return Mathf.Clamp(height, 0f, 0.5f);
    }

    void CreateBoundaryWalls()
    {
        float wallHeight = 20f;
        float wallThickness = 0.5f;
        
        GameObject leftWall = new GameObject("LeftBoundary");
        leftWall.transform.position = new Vector3(-wallThickness * 0.5f, wallHeight * 0.5f, 0f);
        leftWall.transform.SetParent(blocksParent);
        BoxCollider2D leftCollider = leftWall.AddComponent<BoxCollider2D>();
        leftCollider.size = new Vector2(wallThickness, wallHeight);
        leftCollider.isTrigger = false;
        
        GameObject rightWall = new GameObject("RightBoundary");
        int blocksPerOriginal = 10;
        float subBlockSize = blockSize / blocksPerOriginal;
        float rightWallX = (totalFloorBlocks * blockSize) + (blocksPerOriginal * subBlockSize) + wallThickness * 0.5f;
        rightWall.transform.position = new Vector3(rightWallX, wallHeight * 0.5f, 0f);
        rightWall.transform.SetParent(blocksParent);
        BoxCollider2D rightCollider = rightWall.AddComponent<BoxCollider2D>();
        rightCollider.size = new Vector2(wallThickness, wallHeight);
        rightCollider.isTrigger = false;
    }

    void PlaceLightningBolts()
    {
        List<Vector3> nightBlockPositions = new List<Vector3>();
        List<Vector3> dayBlockPositions = new List<Vector3>();
        
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
        
        // Also collect grass block positions (chão)
        List<Vector3> grassBlockPositions = new List<Vector3>();
        FloorBlock[] allFloorBlocks = FindObjectsByType<FloorBlock>(FindObjectsSortMode.None);
        foreach (FloorBlock floorBlock in allFloorBlocks)
        {
            if (floorBlock.floorType == FloorBlock.FloorType.Grass)
            {
                Collider2D col = floorBlock.GetComponent<Collider2D>();
                if (col != null)
                {
                    float grassTop = col.bounds.max.y;
                    Vector3 grassPos = new Vector3(floorBlock.transform.position.x, grassTop, 0f);
                    grassBlockPositions.Add(grassPos);
                }
            }
        }
        
        if (nightBlockPositions.Count == 0 && grassBlockPositions.Count == 0)
        {
            PlaceLightningBoltsFallback(nightBlockPositions, dayBlockPositions);
            return;
        }
        
        int boltsToPlace = 8; // Level 11: 8 lightning bolts
        int placed = 0;
        int attempts = 0;
        int maxAttempts = 300;
        
        // Combine all platforms
        List<Vector3> allPlatforms = new List<Vector3>();
        allPlatforms.AddRange(nightBlockPositions);
        allPlatforms.AddRange(grassBlockPositions);
        
        while (placed < boltsToPlace && attempts < maxAttempts)
        {
            attempts++;
            
            // FIXED: Use deterministic positions instead of random
            // Process platforms in order and place bolts at fixed offsets
            int platformIndex = attempts % allPlatforms.Count;
            Vector3 platformPos = allPlatforms[platformIndex];
            
            // Place bolt at fixed offset from platform (deterministic)
            Vector3 candidatePos = platformPos;
            // Use fixed height based on platform index (deterministic pattern)
            float heightOffset = 0.8f + ((platformIndex % 4) * 0.2f); // Cycles through 0.8, 1.0, 1.2, 1.4
            candidatePos.y += heightOffset;
            // Use fixed horizontal offset based on platform index (deterministic)
            float xOffset = ((platformIndex % 3) - 1) * 0.5f; // Cycles through -0.5, 0, 0.5
            candidatePos.x += xOffset;
            
            // Ensure it's near a platform/chão
            float minDistToPlatform = float.MaxValue;
            foreach (Vector3 platPos in allPlatforms)
            {
                float dist = Vector3.Distance(candidatePos, platPos);
                if (dist < minDistToPlatform)
                {
                    minDistToPlatform = dist;
                }
            }
            
            // Must be within 2 blocks of a platform
            if (minDistToPlatform > 2f * blockSize) continue;
            
            // CRITICAL: Ensure bolt is reachable during night
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
            
            // Check distance to nearest grass block (chão is always reachable)
            float minDistToGrass = float.MaxValue;
            foreach (Vector3 grassPos in grassBlockPositions)
            {
                float dist = Vector3.Distance(candidatePos, grassPos);
                if (dist < minDistToGrass)
                {
                    minDistToGrass = dist;
                }
            }
            
            // Must be within reachable distance of night block (2 blocks) OR grass/chão (2.5 blocks)
            bool reachableAtNight = (minDistToNight <= 2f * blockSize) || (minDistToGrass <= 2.5f * blockSize);
            if (!reachableAtNight) continue;
            
            bool tooCloseToDayBlock = false;
            foreach (Vector3 dayPos in dayBlockPositions)
            {
                if (Vector3.Distance(candidatePos, dayPos) < 5f)
                {
                    tooCloseToDayBlock = true;
                    break;
                }
            }
            
            if (tooCloseToDayBlock) continue;
            
            // CRITICAL: Check distance to other bolts - must be at least one screen width apart
            bool tooCloseToOtherBolt = false;
            float cameraScreenWidth = GetCameraScreenWidth();
            float minDistanceBetweenBolts = cameraScreenWidth * 1.1f; // At least 1 screen width + 10% buffer
            
            LightningBoltCollector[] existingBolts = FindObjectsByType<LightningBoltCollector>(FindObjectsSortMode.None);
            foreach (LightningBoltCollector bolt in existingBolts)
            {
                float dist = Vector3.Distance(candidatePos, bolt.transform.position);
                if (dist < minDistanceBetweenBolts)
                {
                    tooCloseToOtherBolt = true;
                    break;
                }
            }
            
            if (tooCloseToOtherBolt) continue;
            
            // Check if not inside a block
            if (IsPositionInsideBlock(candidatePos)) continue;
            
            CreateLightningBolt(candidatePos);
            placed++;
        }
        
        if (placed < boltsToPlace)
        {
            Debug.LogWarning($"Only placed {placed}/{boltsToPlace} lightning bolts. Using fallback.");
            PlaceLightningBoltsFallback(nightBlockPositions, dayBlockPositions);
        }
    }
    
    bool IsPositionInsideBlock(Vector3 position)
    {
        Collider2D[] overlaps = Physics2D.OverlapPointAll(position);
        foreach (Collider2D col in overlaps)
        {
            if (col != null && !col.isTrigger)
            {
                if (col.GetComponent<BoxBlock>() != null || col.GetComponent<FloorBlock>() != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Get the camera screen width in world units
    /// This ensures lightning bolts are spaced at least one screen width apart
    /// </summary>
    float GetCameraScreenWidth()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            float camAspectRatio = mainCamera.aspect;
            float orthographicSize = mainCamera.orthographicSize;
            return 2f * orthographicSize * camAspectRatio;
        }
        
        // Fallback: calculate from camera setup
        float fallbackAspectRatio = 16f / 9f;
        float desiredHalfWidth = visibleBlocks * 0.5f;
        float calculatedSize = desiredHalfWidth / fallbackAspectRatio;
        float zoomedInSize = calculatedSize * 0.333f;
        return 2f * zoomedInSize * fallbackAspectRatio;
    }
    
    void PlaceLightningBoltsFallback(List<Vector3> nightBlocks, List<Vector3> dayBlocks)
    {
        Vector3[] fallbackPositions = new Vector3[]
        {
            new Vector3(8f, 2.5f, 0f),
            new Vector3(18f, 2.0f, 0f),
            new Vector3(35f, 4.5f, 0f),
            new Vector3(50f, 3.0f, 0f),
            new Vector3(65f, 4.0f, 0f),
            new Vector3(80f, 3.5f, 0f),
            new Vector3(95f, 2.8f, 0f),
            new Vector3(110f, 3.2f, 0f)
        };
        
        foreach (Vector3 pos in fallbackPositions)
        {
            CreateLightningBolt(pos);
        }
    }

    void PlaceMonsters()
    {
        // Level 11: All monster types (13 total monsters)
        PlaceFlyingEyeMonsters();
        PlaceMushroomMonstersOnGrass();
        PlaceGoblinMonsters();
        PlaceSkeletonMonsters();
    }
    
    /// <summary>
    /// Place Flying Eye monsters - Level 11: 4 FlyingEye
    /// </summary>
    void PlaceFlyingEyeMonsters()
    {
        // Level 11: 4 FlyingEye monsters (distributed across 130 blocks)
        CreateMonster(new Vector3(25f, 2.5f, 0f), Monster.MonsterType.FlyingEye);
        CreateMonster(new Vector3(50f, 2.8f, 0f), Monster.MonsterType.FlyingEye);
        CreateMonster(new Vector3(80f, 2.6f, 0f), Monster.MonsterType.FlyingEye);
        CreateMonster(new Vector3(110f, 2.7f, 0f), Monster.MonsterType.FlyingEye);
    }
    
    /// <summary>
    /// Place Goblin monsters - Level 11: 3 Goblin
    /// </summary>
    void PlaceGoblinMonsters()
    {
        // Level 11: 3 Goblin monsters on ground
        CreateMonster(new Vector3(30f, 0.8f, 0f), Monster.MonsterType.Goblin);
        CreateMonster(new Vector3(65f, 0.9f, 0f), Monster.MonsterType.Goblin);
        CreateMonster(new Vector3(100f, 0.8f, 0f), Monster.MonsterType.Goblin);
    }
    
    /// <summary>
    /// Place Skeleton monsters - Level 11: 3 Skeleton
    /// </summary>
    void PlaceSkeletonMonsters()
    {
        // Level 11: 3 Skeleton monsters on ground
        CreateMonster(new Vector3(20f, 0.7f, 0f), Monster.MonsterType.Skeleton);
        CreateMonster(new Vector3(55f, 0.8f, 0f), Monster.MonsterType.Skeleton);
        CreateMonster(new Vector3(90f, 0.7f, 0f), Monster.MonsterType.Skeleton);
    }
    
    void PlaceMushroomMonstersOnGrass()
    {
        // Level 5: Place 3-4 Mushroom monsters at strategic positions
        FloorBlock[] allFloorBlocks = FindObjectsByType<FloorBlock>(FindObjectsSortMode.None);
        List<Vector3> grassBlockPositions = new List<Vector3>();
        
        foreach (FloorBlock floorBlock in allFloorBlocks)
        {
            if (floorBlock.floorType == FloorBlock.FloorType.Grass)
            {
                Collider2D col = floorBlock.GetComponent<Collider2D>();
                if (col != null)
                {
                    float grassTop = col.bounds.max.y;
                    Vector3 mushroomPos = new Vector3(floorBlock.transform.position.x, grassTop, 0f);
                    grassBlockPositions.Add(mushroomPos);
                }
            }
        }
        
        grassBlockPositions.Sort((a, b) => a.x.CompareTo(b.x));
        
        float minDistanceBetweenMushrooms = 15f * blockSize; // At least 15 blocks apart
        float playerStartX = 1f * blockSize;
        float avoidStartArea = playerStartX + (15f * blockSize);
        
        int blocksPerOriginal = 10;
        float subBlockSize = blockSize / blocksPerOriginal;
        float finishX = (totalFloorBlocks * blockSize) + (blocksPerOriginal * subBlockSize) - 2f;
        float avoidFinishArea = finishX - (15f * blockSize);
        
        float lastMushroomX = -1000f;
        int mushroomsPlaced = 0;
        int maxMushrooms = 3; // Level 11: 3 Mushroom monsters (to total 13-14 with other types)
        
        foreach (Vector3 grassPos in grassBlockPositions)
        {
            if (mushroomsPlaced >= maxMushrooms) break;
            
            if (grassPos.x < avoidStartArea || grassPos.x > avoidFinishArea) continue;
            if (grassPos.x - lastMushroomX < minDistanceBetweenMushrooms) continue;
            
            CreateMonster(grassPos, Monster.MonsterType.Mushroom);
            lastMushroomX = grassPos.x;
            mushroomsPlaced++;
        }
        
        Debug.Log($"Level 5: Placed {mushroomsPlaced} mushrooms on grass blocks");
    }
    
    void SetupWinnerScreen()
    {
        WinnerScreen existingWinnerScreen = FindFirstObjectByType<WinnerScreen>();
        if (existingWinnerScreen == null)
        {
            GameObject winnerScreenObj = new GameObject("WinnerScreen");
            WinnerScreen winnerScreen = winnerScreenObj.AddComponent<WinnerScreen>();
            winnerScreen.SetCurrentLevel(11);
        }
    }
    
    void SetupBlockManager()
    {
        BlockManager existingManager = FindFirstObjectByType<BlockManager>();
        if (existingManager == null)
        {
            GameObject blockManagerObj = new GameObject("BlockManager");
            BlockManager blockManager = blockManagerObj.AddComponent<BlockManager>();
            blockManager.autoFindBlocks = true;
        }
    }
    
    void SetupTimeOfDayController()
    {
        TimeOfDayController existingController = FindFirstObjectByType<TimeOfDayController>();
        if (existingController == null)
        {
            GameObject timeControllerObj = new GameObject("TimeOfDayController");
            TimeOfDayController controller = timeControllerObj.AddComponent<TimeOfDayController>();
            controller.isDayTime = true;
        }
    }

    void PlaceFinishLine()
    {
        // Calculate finish line position at end of level
        int blocksPerOriginal = 10;
        float subBlockSize = blockSize / blocksPerOriginal;
        float finishX = (totalFloorBlocks * blockSize) + (blocksPerOriginal * subBlockSize * 0.5f);
        
        // Position on ground level (Level 5 is higher, so adjust Y)
        float finishY = 1.5f; // On top of ground
        
        CreateFinishLine(new Vector3(finishX, finishY, 0f));
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
        if (playerPrefab != null)
        {
            playerInstance = Instantiate(playerPrefab, position, Quaternion.identity);
            playerInstance.name = "Player";
        }
        else
        {
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                playerInstance = existingPlayer;
                playerInstance.transform.position = position;
            }
            else
            {
                playerInstance = CreatePlayerFromScratch(position);
            }
        }
    }

    GameObject CreatePlayerFromScratch(Vector3 position)
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = position;
        
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            sr.sprite = playerSprite;
        }
        sr.color = Color.white;
        sr.sortingOrder = 1;
        sr.drawMode = SpriteDrawMode.Simple;
        player.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 3f;
        
        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerAnimationController>();
        
        return player;
    }

    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
            if (cameraFollow == null)
            {
                cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
            }
            
            if (playerInstance != null)
            {
                cameraFollow.target = playerInstance.transform;
            }
            
            float aspectRatio = mainCamera.aspect;
            float desiredHalfWidth = visibleBlocks * 0.5f;
            float calculatedSize = desiredHalfWidth / aspectRatio;
            float zoomedInSize = calculatedSize * 0.333f;
            
            cameraFollow.cameraSize = zoomedInSize;
            mainCamera.orthographicSize = zoomedInSize;
            
            float playerStartX = 1f * blockSize;
            int blocksPerOriginal = 10;
            float subBlockSize = blockSize / blocksPerOriginal;
            float levelEndX = (totalFloorBlocks * blockSize) + (blocksPerOriginal * subBlockSize);
            float cameraHalfWidth = zoomedInSize * aspectRatio;
            
            cameraFollow.useBounds = true;
            cameraFollow.minX = playerStartX;
            cameraFollow.maxX = levelEndX - cameraHalfWidth;
            cameraFollow.minY = -2f;
            cameraFollow.maxY = 10f;
            
            float initialCameraX = playerStartX;
            if (playerInstance != null)
            {
                initialCameraX = playerInstance.transform.position.x;
                initialCameraX = Mathf.Clamp(initialCameraX, cameraFollow.minX, cameraFollow.maxX);
            }
            
            mainCamera.transform.position = new Vector3(
                initialCameraX,
                playerInstance != null ? playerInstance.transform.position.y : 2f,
                -10f
            );
        }
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
            controller.endX = totalFloorBlocks * blockSize + 20f;
            controller.minY = 5f;
            controller.maxY = 10f;
            controller.cloudCount = 12;
        }
    }

    void RefreshGameManager()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.RefreshAllObjects();
            gameManager.UpdateTimeOfDay(gameManager.isDayTime);
        }
    }

    GameObject CreateBoxBlock(Vector3 position, bool visibleDuringDay)
    {
        if (boxBlockPrefab == null) return null;
        
        GameObject block = Instantiate(boxBlockPrefab, position, Quaternion.identity);
        block.transform.SetParent(blocksParent);
        
        BoxBlock boxBlock = block.GetComponent<BoxBlock>();
        if (boxBlock == null)
        {
            boxBlock = block.AddComponent<BoxBlock>();
        }
        boxBlock.visibleDuringDay = visibleDuringDay;
        block.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        
        return block;
    }

    GameObject CreateFloorBlock(Vector3 position, FloorBlock.FloorType floorType)
    {
        if (floorBlockPrefab == null) return null;
        
        GameObject block = Instantiate(floorBlockPrefab, position, Quaternion.identity);
        block.transform.SetParent(blocksParent);
        
        FloorBlock floorBlock = block.GetComponent<FloorBlock>();
        if (floorBlock == null)
        {
            floorBlock = block.AddComponent<FloorBlock>();
        }
        floorBlock.floorType = floorType;
        
        return block;
    }

    GameObject CreateLightningBolt(Vector3 position)
    {
        if (lightningBoltPrefab == null) return null;
        
        GameObject bolt = Instantiate(lightningBoltPrefab, position, Quaternion.identity);
        bolt.transform.SetParent(itemsParent);
        bolt.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
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
        
        // Add Rigidbody2D for movement (indie game pattern)
        Rigidbody2D rb = monster.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = monsterType == Monster.MonsterType.FlyingEye ? 0f : 1f; // Only Flying Eye doesn't fall
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        
        // Add MonsterAI for movement behavior (indie game pattern)
        MonsterAI monsterAI = monster.AddComponent<MonsterAI>();
        
        // Configure AI based on monster type
        if (monsterType == Monster.MonsterType.Mushroom)
        {
            monsterAI.canPatrol = true;
            monsterAI.patrolStartX = position.x - 3f;
            monsterAI.patrolEndX = position.x + 3f;
            monsterAI.patrolSpeed = 1.5f;
            monsterAI.canChasePlayer = true;
            monsterAI.chaseSpeed = 2.5f;
            monsterAI.chaseRange = 5f;
        }
        else if (monsterType == Monster.MonsterType.FlyingEye)
        {
            monsterAI.canPatrol = true;
            monsterAI.patrolStartX = position.x - 4f;
            monsterAI.patrolEndX = position.x + 4f;
            monsterAI.patrolSpeed = 2f;
            monsterAI.canChasePlayer = true;
            monsterAI.chaseSpeed = 3f;
            monsterAI.chaseRange = 6f;
        }
        else if (monsterType == Monster.MonsterType.Goblin)
        {
            monsterAI.canPatrol = true;
            monsterAI.patrolStartX = position.x - 4f;
            monsterAI.patrolEndX = position.x + 4f;
            monsterAI.patrolSpeed = 2f;
            monsterAI.canChasePlayer = true;
            monsterAI.chaseSpeed = 3f;
            monsterAI.chaseRange = 6f;
        }
        else if (monsterType == Monster.MonsterType.Skeleton)
        {
            monsterAI.canPatrol = true;
            monsterAI.patrolStartX = position.x - 3f;
            monsterAI.patrolEndX = position.x + 3f;
            monsterAI.patrolSpeed = 1.2f;
            monsterAI.canChasePlayer = true;
            monsterAI.chaseSpeed = 2.8f;
            monsterAI.chaseRange = 7f;
        }
        
        // Add MonsterAnimationController for animations (indie game pattern)
        MonsterAnimationController animController = monster.AddComponent<MonsterAnimationController>();
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
