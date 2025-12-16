using UnityEngine;

/// <summary>
/// Generates Level 4 - Simple fixed level with 3 lightning bolts
/// </summary>
public class Level4Generator : MonoBehaviour
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
    private int levelLength = 30; // Fixed length

    void Start()
    {
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null && levelManager.currentLevel != 4)
        {
            gameObject.SetActive(false);
            return;
        }
        
        if (generateOnStart)
        {
            GenerateLevel4();
        }
    }

    public void GenerateLevel4()
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

        Debug.Log("Level 4 generated successfully!");
    }

    void CreateFixedLevel()
    {
        // Ground floor: extend past finish line to ensure no gaps
        for (int i = 0; i < levelLength + 5; i++)
        {
            CreateFloorBlock(new Vector3(i * blockSize, 0f, 0f), FloorBlock.FloorType.Grass);
        }
        
        CreateBoxBlock(new Vector3(7f * blockSize, 0.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(7.5f * blockSize, 0.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(14f * blockSize, 0.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(14.5f * blockSize, 0.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(21f * blockSize, 0.5f, 0f), visibleDuringDay: false);
        CreateBoxBlock(new Vector3(21.5f * blockSize, 0.5f, 0f), visibleDuringDay: false);
        
        CreateMonster(new Vector3(11f * blockSize, 0.5f, 0f), Monster.MonsterType.Mushroom);
        CreateMonster(new Vector3(19f * blockSize, 0.5f, 0f), Monster.MonsterType.FlyingEye);
        
        CreateLightningBolt(new Vector3(7.25f * blockSize, 0.5f + 1.5f, 0f));
        CreateLightningBolt(new Vector3(14.25f * blockSize, 0.5f + 1.5f, 0f));
        CreateLightningBolt(new Vector3(21.25f * blockSize, 0.5f + 1.5f, 0f));
        
        CreateFinishLine(new Vector3(levelLength * blockSize, 1.5f, 0f));
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
        float rightWallX = levelLength * blockSize + wallThickness * 0.5f;
        rightWall.transform.position = new Vector3(rightWallX, wallHeight * 0.5f, 0f);
        rightWall.transform.SetParent(blocksParent);
        BoxCollider2D rightCollider = rightWall.AddComponent<BoxCollider2D>();
        rightCollider.size = new Vector2(wallThickness, wallHeight);
        rightCollider.isTrigger = false;
    }

    void SetupWinnerScreen()
    {
        WinnerScreen existingWinnerScreen = FindFirstObjectByType<WinnerScreen>();
        if (existingWinnerScreen == null)
        {
            GameObject winnerScreenObj = new GameObject("WinnerScreen");
            WinnerScreen winnerScreen = winnerScreenObj.AddComponent<WinnerScreen>();
            winnerScreen.SetCurrentLevel(4);
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
        
        mainCamera.transform.position = new Vector3(2f, 2f, -10f);
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
