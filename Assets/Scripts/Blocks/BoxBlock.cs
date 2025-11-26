using UnityEngine;

/// <summary>
/// Box/Carton block - can be pushed, destroyed, or used as platform.
/// May react to day/night cycle (e.g., becomes fragile at night).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class BoxBlock : MonoBehaviour
{
    [Header("Box Settings")]
    public Sprite boxSprite;
    public Color dayColor = new Color(0.9f, 0.8f, 0.7f); // Light brown/carton color
    public Color nightColor = new Color(0.6f, 0.5f, 0.4f); // Darker at night
    
    [Header("Day/Night Behavior")]
    public bool reactsToTimeOfDay = true;
    public bool canBeDestroyed = true;
    public bool canBePushed = true;
    [Tooltip("If true, box will be visible only during day. If false, visible only at night.")]
    public bool visibleDuringDay = true;
    
    [Header("Destruction")]
    public GameObject destructionEffect;
    public float health = 1f;
    
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private bool isDayTime = true;
    private float currentHealth;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        currentHealth = health;
        
        SetupBox();
    }
    
    void Start()
    {
        // Check GameManager for initial time state
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null && reactsToTimeOfDay)
        {
            isDayTime = gameManager.isDayTime;
            UpdateVisuals();
        }
    }

    void SetupBox()
    {
        if (boxSprite != null)
        {
            spriteRenderer.sprite = boxSprite;
        }
        else
        {
            // Create a default white sprite if none is assigned
            spriteRenderer.sprite = CreateDefaultSprite();
        }
        
        // Setup collider
        boxCollider.enabled = true;
        boxCollider.isTrigger = false;
        
        // Auto-size collider to match sprite bounds
        if (spriteRenderer.sprite != null)
        {
            boxCollider.size = spriteRenderer.sprite.bounds.size;
        }
        else
        {
            // Default size: thinner blocks (1 unit wide, 0.5 units tall)
            boxCollider.size = new Vector2(1f, 0.5f);
        }
        
        // Update visuals (this will set initial visibility)
        UpdateVisuals();
    }
    
    /// <summary>
    /// Creates a default sprite for blocks - thinner (wider than tall) so player can jump onto them
    /// </summary>
    Sprite CreateDefaultSprite()
    {
        // Create a thinner block: wider than tall (horizontal platform shape)
        // This makes it easier to jump onto blocks
        int width = 32;  // Wide
        int height = 16; // Thin (half the width)
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        // Use box color
        Color blockColor = dayColor;
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = blockColor;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Set texture filter mode to prevent gaps between sprites
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        
        // Create sprite from texture (1 unit = 32 pixels, so pixelsPerUnit = 32)
        // Thinner blocks: width = 1 unit, height = 0.5 units
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f);
    }

    /// <summary>
    /// Called by TimeOfDayController when day/night changes
    /// </summary>
    public void SetTimeOfDay(bool isDay)
    {
        if (!reactsToTimeOfDay) return;
        
        isDayTime = isDay;
        UpdateVisuals();
        
        // Boxes might become more fragile at night
        if (!isDay && canBeDestroyed)
        {
            currentHealth = health * 0.5f; // Half health at night
        }
        else
        {
            currentHealth = health;
        }
    }

    void UpdateVisuals()
    {
        // Blocks keep their original colors - don't change based on day/night
        // Only background changes, not block colors
        spriteRenderer.color = dayColor; // Always use day color
        
        // Handle visibility based on day/night
        if (reactsToTimeOfDay)
        {
            // Day-only blocks: visible only during day
            // Night-only blocks: visible only during night
            if (visibleDuringDay)
            {
                // Day-only blocks: visible during day only
                bool shouldBeVisible = isDayTime;
                spriteRenderer.enabled = shouldBeVisible;
                boxCollider.enabled = shouldBeVisible;
            }
            else
            {
                // Night-only blocks: visible during night only
                bool shouldBeVisible = !isDayTime;
                spriteRenderer.enabled = shouldBeVisible;
                boxCollider.enabled = shouldBeVisible;
            }
        }
        else
        {
            // Always visible if not reacting to time of day
            spriteRenderer.enabled = true;
            boxCollider.enabled = true;
        }
    }

    /// <summary>
    /// Damage the box (e.g., from player attacks or environmental hazards)
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (!canBeDestroyed) return;
        
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            DestroyBox();
        }
    }

    void DestroyBox()
    {
        // Spawn destruction effect if available
        if (destructionEffect != null)
        {
            Instantiate(destructionEffect, transform.position, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }

    void OnValidate()
    {
        if (spriteRenderer != null)
        {
            if (boxSprite != null)
        {
            spriteRenderer.sprite = boxSprite;
            }
            else if (Application.isPlaying)
            {
                // Only create default sprite at runtime, not in editor
                spriteRenderer.sprite = CreateDefaultSprite();
            }
            UpdateVisuals();
        }
    }
}

