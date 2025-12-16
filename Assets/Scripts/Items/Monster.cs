using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Monster - simple death point that shows death screen when player touches it
/// Appears only at night
/// Supports Flying Eye and Mushroom sprites from Monsters Creatures Fantasy asset pack
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Monster : MonoBehaviour
{
    public enum MonsterType
    {
        FlyingEye,
        Mushroom,
        Goblin,
        Skeleton
    }
    
    [Header("Monster Settings")]
    public MonsterType monsterType = MonsterType.FlyingEye;
    public Color monsterColor = Color.white; // Default white (sprites have their own colors)
    public float size = 1f; // Size of the monster
    
    [Header("Day/Night Behavior")]
    public bool visibleOnlyAtNight = true; // Monsters only appear at night
    
    [Header("Future Movement Settings")]
    [Tooltip("Will be used for future movement implementation")]
    public bool canMove = false;
    public float moveSpeed = 0f;
    public Vector2 moveDirection = Vector2.zero;
    
    private BoxCollider2D monsterCollider;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;
    private bool isDayTime = true;
    private DeathScreen deathScreen;
    
    void Awake()
    {
        monsterCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        SetupMonster();
        
        // Add collider visualizer for debugging (only in editor)
        #if UNITY_EDITOR
        if (GetComponent<ColliderVisualizer>() == null)
        {
            ColliderVisualizer visualizer = gameObject.AddComponent<ColliderVisualizer>();
            visualizer.colliderColor = new Color(1f, 0.5f, 0f, 0.8f); // Orange for monster collider
            visualizer.spriteBoundsColor = new Color(0f, 1f, 0f, 0.8f); // Green for sprite
            visualizer.showInGameView = false; // Only show in Scene view
            visualizer.showInSceneView = true;
        }
        #endif
    }
    
    void SetupMonster()
    {
        // Load sprite based on monster type FIRST
        if (spriteRenderer.sprite == null)
        {
            Sprite loadedSprite = LoadMonsterSprite(monsterType);
            if (loadedSprite != null)
            {
                spriteRenderer.sprite = loadedSprite;
            }
            else
            {
                // Fallback to default sprite if loading fails
                spriteRenderer.sprite = CreateDefaultMonsterSprite();
            }
        }
        
        // Setup collider as trigger
        monsterCollider.isTrigger = true;
        
        // CRITICAL: Size collider to match sprite bounds, but make it much smaller for tighter hitbox
        // Use 40% of sprite size to ensure player only dies when actually touching the visual sprite
        if (spriteRenderer.sprite != null)
        {
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            monsterCollider.size = spriteSize * 0.4f; // 40% of sprite size for very tight hitbox
        }
        else
        {
            // Only use size parameter as temporary fallback if no sprite loaded yet
            monsterCollider.size = new Vector2(size * 0.4f, size * 0.4f);
        }
        
        spriteRenderer.color = monsterColor;
        spriteRenderer.sortingOrder = 2; // Above ground and player
    }
    
    /// <summary>
    /// Load monster sprite from assets based on monster type
    /// Follows Unity best practices for asset loading
    /// Uses AssetDatabase in editor, Resources at runtime
    /// </summary>
    Sprite LoadMonsterSprite(MonsterType type)
    {
        string spriteName = "";
        string folderName = "";
        
        switch (type)
        {
            case MonsterType.FlyingEye:
                spriteName = "Flight";
                folderName = "Flying eye";
                break;
            case MonsterType.Mushroom:
                spriteName = "Idle";
                folderName = "Mushroom";
                break;
            case MonsterType.Goblin:
                spriteName = "Idle";
                folderName = "Goblin";
                break;
            case MonsterType.Skeleton:
                spriteName = "Idle";
                folderName = "Skeleton";
                break;
        }
        
        if (string.IsNullOrEmpty(spriteName)) return null;
        
        Sprite sprite = null;
        
#if UNITY_EDITOR
        // In editor, use AssetDatabase to find and load the sprite
        // This is the recommended Unity approach for editor-time asset loading
        string searchFolder = $"Assets/Monsters Creatures Fantasy/Sprites/{folderName}";
        
        // First, try to find the specific sprite by name (case-insensitive search)
        string[] guids = AssetDatabase.FindAssets($"{spriteName} t:Sprite", new[] { searchFolder });
        
        // Also try searching in the parent folder if direct search fails
        if (guids.Length == 0)
        {
            guids = AssetDatabase.FindAssets($"{spriteName} t:Sprite", new[] { "Assets/Monsters Creatures Fantasy" });
        }
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // Verify it's the correct sprite (contains folder name and sprite name, case-insensitive)
            string lowerPath = assetPath.ToLower();
            string lowerFolder = folderName.ToLower();
            string lowerSprite = spriteName.ToLower();
            
            if (lowerPath.Contains(lowerFolder) && lowerPath.Contains(lowerSprite))
            {
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite != null)
                {
                    Debug.Log($"Loaded {type} sprite from: {assetPath}");
                    return sprite;
                }
            }
        }
        
        // Fallback: find any sprite in the monster's folder (prefer Idle/Flight)
        guids = AssetDatabase.FindAssets("t:Sprite", new[] { searchFolder });
        if (guids.Length == 0)
        {
            // Try parent folder
            guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Monsters Creatures Fantasy/Sprites" });
        }
        
        if (guids.Length > 0)
        {
            // Prefer Idle or Flight sprites
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string lowerPath = assetPath.ToLower();
                // Check if it's in the correct folder
                if (lowerPath.Contains(folderName.ToLower()))
                {
                    if (lowerPath.Contains("idle") || lowerPath.Contains("flight"))
                    {
                        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                        if (sprite != null)
                        {
                            Debug.Log($"Loaded {type} sprite (fallback) from: {assetPath}");
                            return sprite;
                        }
                    }
                }
            }
            // If no Idle/Flight found, use first sprite from correct folder
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.ToLower().Contains(folderName.ToLower()))
                {
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                    if (sprite != null)
                    {
                        Debug.Log($"Loaded {type} sprite (first available) from: {assetPath}");
                        return sprite;
                    }
                }
            }
        }
        
        Debug.LogWarning($"Could not find sprite for {type} in {searchFolder}. Using default sprite.");
#else
        // At runtime, try Resources.Load (requires sprites to be in Resources folder)
        // Note: For runtime, sprites should be moved to Resources folder or referenced directly
        string resourcesPath = $"Monsters Creatures Fantasy/Sprites/{folderName}/{spriteName}";
        sprite = Resources.Load<Sprite>(resourcesPath);
        if (sprite == null)
        {
            // Try without spaces
            resourcesPath = resourcesPath.Replace(" ", "_");
            sprite = Resources.Load<Sprite>(resourcesPath);
        }
#endif
        
        return sprite;
    }
    
    void Start()
    {
        // Find DeathScreen
        deathScreen = FindFirstObjectByType<DeathScreen>();
        if (deathScreen == null)
        {
            // Create DeathScreen if it doesn't exist
            GameObject deathScreenObj = new GameObject("DeathScreen");
            deathScreen = deathScreenObj.AddComponent<DeathScreen>();
        }
        
        // Check GameManager for initial time state
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            isDayTime = gameManager.isDayTime;
            UpdateVisuals();
            
            // Subscribe to day/night changes
            gameManager.OnTimeOfDayChanged += SetTimeOfDay;
        }
        
        // CRITICAL: Ensure collider size matches sprite (40% scale) after sprite is definitely loaded
        // This is called in Start() to ensure sprite is loaded and collider matches it
        // Use 40% of sprite size for very tight hitbox - player only dies when actually touching visual sprite
        if (spriteRenderer.sprite != null && monsterCollider != null)
        {
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            monsterCollider.size = spriteSize * 0.4f; // 40% of sprite size for very tight hitbox
            
        }
        
        // Future: Initialize movement if canMove is enabled
        if (canMove && moveSpeed > 0f)
        {
            // Movement will be implemented here in the future
        }
    }
    
    void OnDestroy()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnTimeOfDayChanged -= SetTimeOfDay;
        }
    }
    
    /// <summary>
    /// Called when day/night changes
    /// </summary>
    public void SetTimeOfDay(bool isDay)
    {
        isDayTime = isDay;
        UpdateVisuals();
    }
    
    /// <summary>
    /// Update monster visibility based on time of day
    /// </summary>
    void UpdateVisuals()
    {
        if (visibleOnlyAtNight)
        {
            // Only visible at night
            bool shouldBeVisible = !isDayTime;
            spriteRenderer.enabled = shouldBeVisible;
            monsterCollider.enabled = shouldBeVisible;
        }
        else
        {
            // Always visible
            spriteRenderer.enabled = true;
            monsterCollider.enabled = true;
        }
    }
    
    void Update()
    {
        // Future: Handle movement if enabled
        if (canMove && moveSpeed > 0f && !isDead)
        {
            // Movement logic will be added here
            // Example: transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
        }
    }
    
    void LateUpdate()
    {
        // CRITICAL: Ensure collider matches sprite size (40% scale) after sprite is loaded
        // This runs after all updates to ensure sprite is set and collider matches it
        // Use 40% of sprite size for very tight hitbox - player only dies when actually touching visual sprite
        if (spriteRenderer != null && spriteRenderer.sprite != null && monsterCollider != null)
        {
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            Vector2 targetColliderSize = spriteSize * 0.4f; // 40% of sprite size
            
            // Only update if sizes don't match (avoid constant updates)
            if (Vector2.Distance(monsterCollider.size, targetColliderSize) > 0.001f)
            {
                monsterCollider.size = targetColliderSize; // 40% of sprite size for very tight hitbox
            }
        }
    }
    
    /// <summary>
    /// Create a simple monster sprite - red, menacing shape
    /// </summary>
    Sprite CreateDefaultMonsterSprite()
    {
        int spriteSize = Mathf.RoundToInt(size * 32f); // Convert to pixels
        spriteSize = Mathf.Clamp(spriteSize, 16, 64); // Reasonable size range
        
        Texture2D texture = new Texture2D(spriteSize, spriteSize);
        Color[] pixels = new Color[spriteSize * spriteSize];
        
        // Initialize with transparent
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        Color bodyColor = monsterColor;
        Color outlineColor = new Color(monsterColor.r * 0.5f, monsterColor.g * 0.5f, monsterColor.b * 0.5f);
        Color eyeColor = Color.yellow;
        
        // Draw monster shape - simple circular/rounded enemy
        int centerX = spriteSize / 2;
        int centerY = spriteSize / 2;
        float radius = spriteSize * 0.4f;
        
        for (int y = 0; y < spriteSize; y++)
        {
            for (int x = 0; x < spriteSize; x++)
            {
                int index = y * spriteSize + x;
                float distFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                
                // Body
                if (distFromCenter < radius)
                {
                    if (distFromCenter > radius - 2f)
                    {
                        pixels[index] = outlineColor;
                    }
                    else
                    {
                        pixels[index] = bodyColor;
                        
                        // Add spikes/teeth on top
                        if (y > spriteSize * 0.7f && (x % 4 < 2))
                        {
                            pixels[index] = outlineColor;
                        }
                    }
                }
                
                // Eyes
                float eyeDist1 = Vector2.Distance(new Vector2(x, y), new Vector2(centerX - spriteSize * 0.15f, centerY - spriteSize * 0.1f));
                float eyeDist2 = Vector2.Distance(new Vector2(x, y), new Vector2(centerX + spriteSize * 0.15f, centerY - spriteSize * 0.1f));
                
                if (eyeDist1 < spriteSize * 0.08f || eyeDist2 < spriteSize * 0.08f)
                {
                    pixels[index] = eyeColor;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        
        return Sprite.Create(texture, new Rect(0, 0, spriteSize, spriteSize), new Vector2(0.5f, 0.5f), 32f);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player touched the monster
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            KillPlayer();
        }
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        // Also check on stay to ensure detection
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            KillPlayer();
        }
    }
    
    /// <summary>
    /// Kill the player and show death screen
    /// </summary>
    void KillPlayer()
    {
        // Prevent multiple death triggers
        if (isDead) return;
        
        // Check if death screen is already showing
        if (deathScreen != null && deathScreen.isShowing)
        {
            return;
        }
        
        isDead = true;
        
        Debug.Log("Player died! Showing death screen...");
        
        // Show death screen
        if (deathScreen != null)
        {
            deathScreen.ShowDeathScreen();
        }
        else
        {
            // Fallback: reload scene if death screen not found
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
    }
    
    /// <summary>
    /// Reset death state (called when player respawns)
    /// </summary>
    public void ResetDeathState()
    {
        isDead = false;
    }
    
    /// <summary>
    /// Set the position of the monster
    /// </summary>
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
    
    /// <summary>
    /// Enable movement for this monster (future feature)
    /// </summary>
    public void EnableMovement(float speed, Vector2 direction)
    {
        canMove = true;
        moveSpeed = speed;
        moveDirection = direction.normalized;
    }
}

