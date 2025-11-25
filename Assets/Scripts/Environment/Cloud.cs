using UnityEngine;

/// <summary>
/// Individual cloud that changes color based on day/night
/// White during day, dark gray during night
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Cloud : MonoBehaviour
{
    [Header("Cloud Colors")]
    public Color dayColor = Color.white;
    public Color nightColor = new Color(0.3f, 0.3f, 0.3f); // Dark gray
    
    [Header("Cloud Settings")]
    public float moveSpeed = 0.5f; // Horizontal movement speed
    public float floatAmplitude = 0.2f; // Vertical floating amplitude
    public float floatSpeed = 1f; // Vertical floating speed
    
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private float floatTimer = 0f;
    private bool isDayTime = true;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        
        // Create default cloud sprite if none exists
        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = CreateCloudSprite();
        }
        
        // Set initial color
        spriteRenderer.color = dayColor;
        spriteRenderer.sortingOrder = -1; // Behind everything but in front of background
    }
    
    void Start()
    {
        // Check GameManager for initial time state
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            isDayTime = gameManager.isDayTime;
            UpdateColor();
        }
    }
    
    void Update()
    {
        // Float up and down
        floatTimer += Time.deltaTime * floatSpeed;
        float yOffset = Mathf.Sin(floatTimer) * floatAmplitude;
        transform.position = startPosition + new Vector3(0f, yOffset, 0f);
        
        // Move horizontally (optional - can be disabled)
        // transform.position += Vector3.right * moveSpeed * Time.deltaTime;
    }
    
    /// <summary>
    /// Create a simple cloud sprite
    /// </summary>
    Sprite CreateCloudSprite()
    {
        int width = 64;
        int height = 32;
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        // Create cloud shape using Perlin noise
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float xNorm = (float)x / width;
                float yNorm = (float)y / height;
                
                // Create cloud shape using multiple noise layers
                float noise1 = Mathf.PerlinNoise(xNorm * 3f, yNorm * 2f);
                float noise2 = Mathf.PerlinNoise(xNorm * 6f + 100f, yNorm * 4f + 100f);
                float noise3 = Mathf.PerlinNoise(xNorm * 12f + 200f, yNorm * 8f + 200f);
                
                float combinedNoise = (noise1 * 0.5f + noise2 * 0.3f + noise3 * 0.2f);
                
                // Create soft cloud edges
                float distanceFromCenter = Vector2.Distance(new Vector2(xNorm, yNorm), new Vector2(0.5f, 0.5f));
                float edgeFade = 1f - Mathf.Clamp01((distanceFromCenter - 0.2f) * 2f);
                
                float alpha = combinedNoise * edgeFade;
                pixels[y * width + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f);
    }
    
    /// <summary>
    /// Called by CloudController when day/night changes
    /// </summary>
    public void SetTimeOfDay(bool isDay)
    {
        isDayTime = isDay;
        UpdateColor();
    }
    
    /// <summary>
    /// Update cloud color based on time of day
    /// </summary>
    void UpdateColor()
    {
        spriteRenderer.color = isDayTime ? dayColor : nightColor;
    }
}

