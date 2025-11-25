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
    public bool requireAllBolts = false; // If true, player must collect all bolts first
    public string nextLevelSceneName = ""; // Leave empty to return to menu
    public float winDelay = 1f; // Delay before showing win message/loading next level
    
    private BoxCollider2D finishCollider;
    private SpriteRenderer spriteRenderer;
    private bool isCompleted = false;
    private GameManager gameManager;
    
    void Awake()
    {
        finishCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Setup as trigger
        finishCollider.isTrigger = true;
        finishCollider.size = new Vector2(0.5f, 2f); // Tall enough to catch player
        
        // Create finish line sprite
        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = CreateFinishLineSprite();
        }
        
        spriteRenderer.color = finishLineColor;
        spriteRenderer.sortingOrder = 5; // Above ground, below UI
    }
    
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }
    
    /// <summary>
    /// Create a finish line sprite - flag on a pole
    /// </summary>
    Sprite CreateFinishLineSprite()
    {
        int width = 16;
        int height = 32;
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        // Initialize with transparent
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        Color poleColor = new Color(0.4f, 0.2f, 0.1f); // Brown pole
        Color flagColor = finishLineColor;
        
        // Draw flag pole (vertical line in center)
        int poleX = width / 2;
        for (int y = 0; y < height; y++)
        {
            int index = y * width + poleX;
            if (index >= 0 && index < pixels.Length)
            {
                pixels[index] = poleColor;
            }
            // Make pole slightly wider
            if (poleX - 1 >= 0)
            {
                int leftIndex = y * width + (poleX - 1);
                if (leftIndex >= 0 && leftIndex < pixels.Length)
                {
                    pixels[leftIndex] = poleColor;
                }
            }
        }
        
        // Draw flag (triangle on right side of pole)
        int flagStartY = (int)(height * 0.6f); // Flag starts at 60% from bottom
        int flagEndY = height - 2;
        int flagWidth = width - poleX - 2;
        
        for (int y = flagStartY; y < flagEndY; y++)
        {
            float progress = (float)(y - flagStartY) / (flagEndY - flagStartY);
            int currentFlagWidth = Mathf.RoundToInt(flagWidth * (1f - progress * 0.3f)); // Tapered flag
            
            for (int x = poleX + 2; x < poleX + 2 + currentFlagWidth && x < width; x++)
            {
                int index = y * width + x;
                if (index >= 0 && index < pixels.Length)
                {
                    pixels[index] = flagColor;
                    
                    // Add checkered pattern
                    if ((x + y) % 4 < 2)
                    {
                        pixels[index] = Color.Lerp(flagColor, Color.white, 0.2f);
                    }
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0f), 32f);
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
                if (gameManager.totalBoltsCollected < gameManager.boltsRequiredToProgress)
                {
                    Debug.Log($"Need {gameManager.boltsRequiredToProgress} lightning bolts! You have {gameManager.totalBoltsCollected}.");
                    return; // Don't complete if not enough bolts
                }
            }
            
            CompleteLevel();
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
        
        // Invoke completion after delay
        Invoke(nameof(HandleLevelCompletion), winDelay);
    }
    
    /// <summary>
    /// Handle what happens after level completion
    /// </summary>
    void HandleLevelCompletion()
    {
        if (!string.IsNullOrEmpty(nextLevelSceneName))
        {
            // Load next level
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevelSceneName);
        }
        else
        {
            // Return to menu or show win message
            Debug.Log("Level Complete! Returning to menu...");
            // You can add menu scene loading here if you have one
            // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
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

