using UnityEngine;

/// <summary>
/// Floor block - solid ground that players can walk on.
/// Always visible, doesn't react to day/night cycle.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class FloorBlock : MonoBehaviour
{
    public enum FloorType
    {
        Dirt,   // Pure dirt block
        Grass   // Grass block (with grass on top)
    }
    
    [Header("Floor Settings")]
    public Sprite floorSprite;
    public Color floorColor = Color.white;
    public FloorType floorType = FloorType.Grass;
    
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        SetupFloorBlock();
    }
    
    /// <summary>
    /// Setup or refresh the floor block visuals and collider
    /// Call this after changing floorType to update the sprite
    /// </summary>
    public void SetupFloorBlock()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
        
        // Setup floor properties
        if (floorSprite != null)
        {
            spriteRenderer.sprite = floorSprite;
        }
        else
        {
            // Create a default sprite based on floor type
            spriteRenderer.sprite = CreateDefaultSprite();
        }
        
        spriteRenderer.color = floorColor;
        
        // Set sprite renderer properties for smooth rendering
        spriteRenderer.drawMode = SpriteDrawMode.Simple;
        // Enable mipmaps for better quality at different scales (optional)
        if (spriteRenderer.sprite != null && spriteRenderer.sprite.texture != null)
        {
            spriteRenderer.sprite.texture.filterMode = FilterMode.Bilinear;
        }
        
        // Ensure collider is enabled and sized correctly for solid ground
        boxCollider.enabled = true;
        boxCollider.isTrigger = false;
        
        // Auto-size collider to match sprite bounds
        if (spriteRenderer.sprite != null)
        {
            boxCollider.size = spriteRenderer.sprite.bounds.size;
        }
        else
        {
            // Default size if no sprite
            boxCollider.size = Vector2.one;
        }
    }
    
    /// <summary>
    /// Creates a default sprite based on floor type
    /// Dirt: pure brown/dirt color
    /// Grass: green on top, dirt at bottom
    /// </summary>
    public Sprite CreateDefaultSprite()
    {
        // Create a higher resolution texture (32x32) for more realistic, less pixelated look
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        // Dirt colors with variation for realism
        Color dirtColorDark = new Color(0.5f, 0.3f, 0.15f);
        Color dirtColorLight = new Color(0.7f, 0.5f, 0.3f);
        Color dirtColor = dirtColorDark;
        
        // Grass colors with variation for realism (different shades of green)
        Color grassColorDark = new Color(0.2f, 0.6f, 0.15f);
        Color grassColorMid = new Color(0.3f, 0.7f, 0.2f);
        Color grassColorLight = new Color(0.4f, 0.8f, 0.25f);
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int index = y * size + x;
                
                if (floorType == FloorType.Dirt)
                {
                    // Dirt block with texture variation for realism
                    float noise = ((x * 7 + y * 11) % 20) / 20f;
                    pixels[index] = Color.Lerp(dirtColorDark, dirtColorLight, noise);
                }
                else // FloorType.Grass
                {
                    // Grass block: create irregular, weedy grass pattern with realistic colors
                    bool isGrassPixel = IsGrassPixel(x, y, size);
                    
                    if (isGrassPixel)
                    {
                        // Use gradient and variation for realistic grass
                        float grassHeight = (float)(size - 1 - y) / size; // 0 at bottom, 1 at top
                        float variation = ((x * 13 + y * 17) % 15) / 15f;
                        
                        // Blend between grass colors based on height and variation
                        Color baseGrass = Color.Lerp(grassColorDark, grassColorMid, grassHeight);
                        Color finalGrass = Color.Lerp(baseGrass, grassColorLight, variation * 0.3f);
                        pixels[index] = finalGrass;
                    }
                    else
                    {
                        // Dirt below grass with variation
                        float noise = ((x * 7 + y * 11) % 20) / 20f;
                        pixels[index] = Color.Lerp(dirtColorDark, dirtColorLight, noise);
                    }
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Use Bilinear filtering for smoother, less pixelated look
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        
        // Create sprite from texture (1 unit = 32 pixels, so pixelsPerUnit = 32)
        // Use center pivot (0.5, 0.5) for standard alignment
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }
    
    /// <summary>
    /// Determine if a pixel at (x, y) should be grass or dirt
    /// Creates irregular, weedy grass pattern instead of a straight line
    /// y=0 is bottom, y=size-1 is top
    /// </summary>
    bool IsGrassPixel(int x, int y, int size)
    {
        // Calculate distance from top (y=size-1 is top)
        int distanceFromTop = size - 1 - y;
        
        // Use multiple noise functions for organic variation
        // Each x position has a base grass height
        float xPos = (float)x / size;
        
        // Create wavy, organic pattern using sine waves at different frequencies
        float wave1 = Mathf.Sin(xPos * Mathf.PI * 4f) * 0.3f; // Fast wave
        float wave2 = Mathf.Sin(xPos * Mathf.PI * 1.5f) * 0.5f; // Medium wave
        float wave3 = Mathf.Sin(xPos * Mathf.PI * 0.8f) * 0.7f; // Slow wave
        
        // Combine waves for organic shape
        float combinedWave = (wave1 + wave2 + wave3) / 3f;
        
        // Add pseudo-random variation per column for weedy look
        int columnSeed = (x * 17 + 23) % 100;
        float randomVariation = (columnSeed / 100f - 0.5f) * 0.4f;
        
        // Base height in pixels (4-8 pixels tall for higher resolution)
        float baseHeight = 5f + combinedWave * 2f + randomVariation * 2f;
        int grassHeight = Mathf.RoundToInt(baseHeight);
        grassHeight = Mathf.Clamp(grassHeight, 3, 10);
        
        // Check if this pixel is within the base grass area
        if (distanceFromTop < grassHeight)
        {
            return true;
        }
        
        // Add some "weedy" spikes - some individual pixels stick up higher
        // This creates irregular, organic grass shapes
        if (distanceFromTop == grassHeight)
        {
            int pixelSeed = (x * 7 + y * 13) % 10;
            if (pixelSeed < 2) // 20% chance for a spike
            {
                return true;
            }
        }
        
        return false;
    }

    void OnValidate()
    {
        // Update visuals in editor when values change
        if (spriteRenderer != null)
        {
            if (floorSprite != null)
        {
            spriteRenderer.sprite = floorSprite;
            }
            else if (Application.isPlaying)
            {
                // Only create default sprite at runtime, not in editor
                spriteRenderer.sprite = CreateDefaultSprite();
            }
            spriteRenderer.color = floorColor;
        }
    }
    
    void OnEnable()
    {
        // Ensure sprite is updated when component is enabled
        if (spriteRenderer != null && floorSprite == null && Application.isPlaying)
        {
            spriteRenderer.sprite = CreateDefaultSprite();
        }
    }
}

