using UnityEngine;

/// <summary>
/// Generates Level 9 - Simple fixed level with 6 lightning bolts
/// </summary>
public class Level9Generator : MonoBehaviour
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
    private int levelLength = 45; // Fixed length

    void Start()
    {
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null && levelManager.currentLevel != 9)
        {
            gameObject.SetActive(false);
            return;
        }
        
        if (generateOnStart)
        {
            GenerateLevel9();
        }
    }

    public void GenerateLevel9()
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
        CreatePlayer(new Vector3(2f * blockSize, 1f, 0f));
        SetupCamera();
        EnsureGameManagerExists();
        SetupInputSystemFixer();
        SetupBackgroundController();
        SetupLightningBoltCounter();
        SetupDeathScreen();
        SetupWinnerScreen();
        SetupClouds();
        RefreshGameManager();
        
        LightningBoltCounter counter = FindFirstObjectByType<LightningBoltCounter>();
        if (counter != null)
        {
            counter.RefreshTotalBolts();
            counter.ResetToOriginalTotal();
        }

        Debug.Log("Level 9 generated successfully!");
    }

    void CreateFixedLevel()
    {
        // Ground floor: extend past finish line to ensure no gaps
        for (int i = 0; i < levelLength + 5; i++)
        {
            CreateFloorBlock(new Vector3(i * blockSize, 0f, 0f), FloorBlock.FloorType.Grass);
        }
        
        CreateBoxBlock(new Vector3(6f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(6.5f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(10f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(10.5f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(14f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(14.5f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(18f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(18.5f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(22f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(22.5f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(26f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(26.5f * blockSize, 1.5f, 0f), visibleDuringDay: false);
        
        CreateMonster(new Vector3(8f * blockSize, 0.5f, 0f), Monster.MonsterType.Mushroom);
        CreateMonster(new Vector3(12f * blockSize, 0.5f, 0f), Monster.MonsterType.FlyingEye);
        CreateMonster(new Vector3(20f * blockSize, 0.5f, 0f), Monster.MonsterType.Mushroom);
        
        CreateLightningBolt(new Vector3(6.25f * blockSize, 1.5f + 1.5f, 0f));
        CreateLightningBolt(new Vector3(10.25f * blockSize, 1.5f + 1.5f, 0f));
        CreateLightningBolt(new Vector3(14.25f * blockSize, 1.5f + 1.5f, 0f));
        CreateLightningBolt(new Vector3(18.25f * blockSize, 1.5f + 1.5f, 0f));
        CreateLightningBolt(new Vector3(22.25f * blockSize, 1.5f + 1.5f, 0f));
        CreateLightningBolt(new Vector3(26.25f * blockSize, 1.5f + 1.5f, 0f));
        
        CreateFinishLine(new Vector3(levelLength * blockSize, 1.5f, 0f));
    }

    void CreateBoundaryWalls()
    {
        float wallHeight = 20f;
        float wallThickness = 0.5f;
        
        // Only create left boundary wall to prevent going backwards
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
            winnerScreen.SetCurrentLevel(9);
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
        }
        else
        {
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                playerInstance = existingPlayer;
                playerInstance.transform.position = position;
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
        
        float aspectRatio = mainCamera.aspect;
        float cameraSize = 5f;
        cameraFollow.cameraSize = cameraSize;
        mainCamera.orthographicSize = cameraSize;
        
        float cameraHalfWidth = cameraSize * aspectRatio;
        cameraFollow.useBounds = true;
        cameraFollow.minX = 0f + cameraHalfWidth;
        cameraFollow.maxX = levelLength * blockSize - cameraHalfWidth;
        cameraFollow.minY = -2f;
        cameraFollow.maxY = 10f;

        // Position camera so player is exactly one block away from left edge
        float playerStartX = 2f * blockSize;
        float playerOffsetFromLeft = 1f * blockSize;
        float leftEdgeX = playerStartX - playerOffsetFromLeft;
        float initialCameraX = leftEdgeX + cameraHalfWidth;
        
        if (playerInstance != null)
        {
            float playerX = playerInstance.transform.position.x;
            leftEdgeX = playerX - playerOffsetFromLeft;
            initialCameraX = leftEdgeX + cameraHalfWidth;
            initialCameraX = Mathf.Clamp(initialCameraX, cameraFollow.minX, cameraFollow.maxX);
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
        Vector3 validatedPosition = ValidateLightningBoltPosition(position);
        
        GameObject bolt;
        if (lightningBoltPrefab != null)
        {
            bolt = Instantiate(lightningBoltPrefab, validatedPosition, Quaternion.identity);
            bolt.transform.SetParent(itemsParent);
            bolt.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        }
        else
        {
            bolt = new GameObject("LightningBolt");
            bolt.transform.position = validatedPosition;
            bolt.transform.SetParent(itemsParent);
            bolt.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            LightningBolt lightningBolt = bolt.AddComponent<LightningBolt>();
        }
        return bolt;
    }
    
    Vector3 ValidateLightningBoltPosition(Vector3 position)
    {
        float nearestNightBlockDistance = float.MaxValue;
        float nearestDayBlockDistance = float.MaxValue;
        BoxBlock nearestNightBlock = null;
        
        BoxBlock[] allBlocks = FindObjectsByType<BoxBlock>(FindObjectsSortMode.None);
        foreach (BoxBlock block in allBlocks)
        {
            float distance = Vector3.Distance(position, block.transform.position);
            if (block.visibleDuringDay)
            {
                if (distance < nearestDayBlockDistance)
                {
                    nearestDayBlockDistance = distance;
                }
            }
            else
            {
                if (distance < nearestNightBlockDistance)
                {
                    nearestNightBlockDistance = distance;
                    nearestNightBlock = block;
                }
            }
        }
        
        // Bolt must be near night block (within 2 blocks) and away from day blocks (at least 1.5 blocks)
        if (nearestNightBlockDistance > 2f * blockSize && nearestNightBlock != null)
        {
            // Place bolt above nearest night block
            return nearestNightBlock.transform.position + Vector3.up * 1.5f;
        }
        
        if (nearestDayBlockDistance < 1.5f * blockSize && nearestNightBlock != null)
        {
            // Move bolt away from day blocks, towards night blocks
            Vector3 direction = (nearestNightBlock.transform.position - position).normalized;
            return position + direction * 1.5f * blockSize;
        }
        
        return position;
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
