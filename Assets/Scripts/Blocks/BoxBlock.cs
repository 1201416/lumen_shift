using UnityEngine;

/// <summary>
/// Box/Carton block - can be pushed, destroyed, or used as platform.
/// May react to day/night cycle (e.g., becomes fragile at night).
/// One-way platform: player can pass through from below, but lands on top.
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
        
        // Add PlatformEffector2D to make platform passable from below
        PlatformEffector2D platformEffector = GetComponent<PlatformEffector2D>();
        if (platformEffector == null)
        {
            platformEffector = gameObject.AddComponent<PlatformEffector2D>();
        }
        // Configure as one-way platform: player can pass through from below, but lands on top
        platformEffector.useOneWay = true;
        platformEffector.useOneWayGrouping = true;
        platformEffector.surfaceArc = 180f; // Top half of platform (180 degrees from top)
        
        // Update visuals (this will set initial visibility)
        UpdateVisuals();
    }
    
    /// <summary>
    /// Creates a default sprite for blocks - vertical stripe with chess pattern (like finish line)
    /// </summary>
    Sprite CreateDefaultSprite()
    {
        // Create a vertical stripe: tall and thin (vertical platform shape)
        // Chess pattern like finish line
        int width = 8;   // Thin (vertical stripe)
        int height = 32; // Tall
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        Color stripeColor = dayColor;
        Color borderColor = new Color(stripeColor.r * 0.7f, stripeColor.g * 0.7f, stripeColor.b * 0.7f);
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
        
        // Create sprite from texture (1 unit = 32 pixels, so pixelsPerUnit = 32)
        // Vertical stripe: width = 0.25 units, height = 1 unit
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

