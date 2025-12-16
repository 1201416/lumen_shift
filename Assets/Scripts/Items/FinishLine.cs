using UnityEngine;

/// <summary>
/// Finish line - when player reaches this, they've conquered the level!
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class FinishLine : MonoBehaviour
{
    [Header("Visual Settings")]
    public Color finishLineColor = new Color(1f, 0.8f, 0f); // Gold/yellow
    public float flagPoleWidth = 0.1f;
    public float flagHeight = 0.3f;
    
    [Header("Completion Settings")]
    public bool requireAllBolts = true; // If true, player must collect all bolts first
    public string nextLevelSceneName = ""; // Leave empty to return to menu
    public float winDelay = 1f; // Delay before showing win message/loading next level
    
    private BoxCollider2D finishCollider;
    private SpriteRenderer spriteRenderer;
    private bool isCompleted = false;
    private GameManager gameManager;
    private WinnerScreen winnerScreen;
    
    void Awake()
    {
        finishCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Setup as trigger for completion detection
        finishCollider.isTrigger = true;
        finishCollider.size = new Vector2(0.5f, 2f); // Tall enough to catch player
        
        // Create finish line sprite (simple banner)
        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = CreateFinishLineSprite();
        }
        
        spriteRenderer.color = finishLineColor;
        spriteRenderer.sortingOrder = 5; // Above ground, below UI
        
        // No boundary wall - player can pass through finish line and will be stopped after 0.5 seconds
    }
    
    
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        winnerScreen = FindFirstObjectByType<WinnerScreen>();
        
        // Create winner screen if it doesn't exist
        if (winnerScreen == null)
        {
            GameObject winnerObj = new GameObject("WinnerScreen");
            winnerScreen = winnerObj.AddComponent<WinnerScreen>();
        }
    }
    
    /// <summary>
    /// Create a finish line sprite - vertical stripe with chess pattern
    /// </summary>
    Sprite CreateFinishLineSprite()
    {
        int width = 8;   // Thin vertical stripe
        int height = 64; // Tall vertical stripe
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        Color stripeColor = finishLineColor;
        Color borderColor = new Color(finishLineColor.r * 0.7f, finishLineColor.g * 0.7f, finishLineColor.b * 0.7f);
        Color altColor = Color.Lerp(stripeColor, Color.white, 0.1f);
        
        // Draw vertical stripe with chess pattern
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                
                // Border
                if (y == 0 || y == height - 1 || x == 0 || x == width - 1)
                {
                    pixels[index] = borderColor;
                }
                else
                {
                    // Chess pattern (checkered)
                    if ((x + y) % 4 < 2)
                    {
                        pixels[index] = stripeColor;
                    }
                    else
                    {
                        pixels[index] = altColor;
                    }
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point; // Pixel art style
        texture.wrapMode = TextureWrapMode.Clamp;
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCompleted) return;
        
        // Check if player reached finish line
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            // Check if all bolts are required
            if (requireAllBolts && gameManager != null)
            {
                // Get total bolts in level
                int totalBoltsInLevel = GetTotalBoltsInLevel();
                
                if (gameManager.totalBoltsCollected < totalBoltsInLevel)
                {
                    Debug.Log($"Need all {totalBoltsInLevel} lightning bolts! You have {gameManager.totalBoltsCollected}.");
                    
                    // Shake lightning bolts to signal player needs to collect them
                    ShakeLightningBolts();
                    return; // Don't complete if not enough bolts
                }
            }
            
            CompleteLevel();
        }
    }
    
    /// <summary>
    /// Get total number of lightning bolts in the level
    /// </summary>
    int GetTotalBoltsInLevel()
    {
        // Count all bolts in the scene (collected + uncollected)
        LightningBolt[] allBolts = FindObjectsByType<LightningBolt>(FindObjectsSortMode.None);
        int total = 0;
        
        // Count uncollected bolts
        foreach (LightningBolt bolt in allBolts)
        {
            if (bolt != null && !bolt.isCollected)
            {
                total += bolt.boltValue;
            }
        }
        
        // Add collected bolts from GameManager
        if (gameManager != null)
        {
            total += gameManager.totalBoltsCollected;
        }
        
        // If no bolts found, use GameManager's count as fallback
        if (total == 0 && gameManager != null)
        {
            // Try to get from LightningBoltCounter which knows the total
            LightningBoltCounter counter = FindFirstObjectByType<LightningBoltCounter>();
            if (counter != null)
            {
                // The counter should have the total, but we'll use a different approach
                // Count all bolts that were ever in the scene
                total = gameManager.totalBoltsCollected;
                // Add remaining uncollected
                foreach (LightningBolt bolt in allBolts)
                {
                    if (bolt != null && !bolt.isCollected)
                    {
                        total += bolt.boltValue;
                    }
                }
            }
        }
        
        return total;
    }
    
    /// <summary>
    /// Shake lightning bolts left and right twice to signal player needs to collect them
    /// </summary>
    void ShakeLightningBolts()
    {
        LightningBolt[] allBolts = FindObjectsByType<LightningBolt>(FindObjectsSortMode.None);
        foreach (LightningBolt bolt in allBolts)
        {
            if (bolt != null && !bolt.isCollected)
            {
                bolt.StartShakeAnimation();
            }
        }
    }
    
    /// <summary>
    /// Called when player reaches the finish line
    /// </summary>
    void CompleteLevel()
    {
        if (isCompleted) return;
        isCompleted = true;
        
        Debug.Log("Level Complete! You've conquered the level!");
        
        // Change color to indicate completion
        spriteRenderer.color = Color.green;
        
        // Stop player movement after 0.5 seconds (allow them to pass finish line first)
        Invoke(nameof(StopPlayerMovement), 0.5f);
        
        // Show winner screen after player stops (slightly after stopping)
        Invoke(nameof(HandleLevelCompletion), 0.6f);
    }
    
    /// <summary>
    /// Stop player movement after passing finish line
    /// </summary>
    void StopPlayerMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = false; // Disable movement
            }
            
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // Stop movement
            }
        }
    }
    
    /// <summary>
    /// Handle what happens after level completion
    /// </summary>
    void HandleLevelCompletion()
    {
        // Show winner screen
        if (winnerScreen != null)
        {
            winnerScreen.ShowWinnerScreen();
        }
        else
        {
            // Fallback: load next level or return to menu
            if (!string.IsNullOrEmpty(nextLevelSceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevelSceneName);
            }
            else
            {
                Debug.Log("Level Complete! Returning to menu...");
            }
        }
    }
    
    /// <summary>
    /// Set the position of the finish line
    /// </summary>
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
}

