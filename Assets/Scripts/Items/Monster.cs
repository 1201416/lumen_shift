using UnityEngine;

/// <summary>
/// Monster - simple death point that shows death screen when player touches it
/// Appears only at night
/// Designed to be extended with movement in the future
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Monster : MonoBehaviour
{
    [Header("Monster Settings")]
    public Color monsterColor = new Color(0.8f, 0.2f, 0.2f); // Red color
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
    }
    
    void SetupMonster()
    {
        // Setup collider as trigger
        monsterCollider.isTrigger = true;
        monsterCollider.size = new Vector2(size, size);
        
        // Create default sprite if none exists
        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = CreateDefaultMonsterSprite();
        }
        
        spriteRenderer.color = monsterColor;
        spriteRenderer.sortingOrder = 2; // Above ground and player
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

