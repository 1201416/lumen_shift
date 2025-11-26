using UnityEngine;

/// <summary>
/// Lightning Bolt collectible - items the player must collect to progress.
/// These are the key items needed to "keep conquering" through levels.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class LightningBolt : MonoBehaviour
{
    [Header("Visual Settings")]
    public Sprite lightningSprite;
    public Color lightningColor = new Color(1f, 0.9f, 0f); // Bright yellow/gold
    public float rotationSpeed = 180f; // Degrees per second
    public float floatAmplitude = 0.3f; // How much it bobs up and down
    public float floatSpeed = 2f; // Speed of bobbing
    
    [Header("Collection Settings")]
    public int boltValue = 1; // How many bolts this is worth
    public GameObject collectionEffect;
    public AudioClip collectionSound;
    
    [Header("Day/Night Behavior")]
    public bool glowsAtNight = true;
    public Color nightGlowColor = new Color(1f, 1f, 0.5f); // Brighter at night
    
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private Vector3 startPosition;
    public bool isCollected { get; private set; } = false;
    private bool isDayTime = true;
    private float floatTimer = 0f;
    private bool isShaking = false;
    private float shakeTimer = 0f;
    private Vector3 originalPosition;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        startPosition = transform.position;
        originalPosition = startPosition;
        
        SetupLightningBolt();
    }

    void SetupLightningBolt()
    {
        if (lightningSprite != null)
        {
            spriteRenderer.sprite = lightningSprite;
        }
        else
        {
            // Create default lightning bolt sprite if none exists
            spriteRenderer.sprite = CreateDefaultLightningSprite();
        }
        
        spriteRenderer.color = lightningColor;
        spriteRenderer.sortingOrder = 10; // Make sure it appears above other sprites
        
        // Setup collider as trigger so player can collect it
        circleCollider.isTrigger = true;
        circleCollider.radius = 0.5f; // Adjust based on sprite size
    }
    
    /// <summary>
    /// Create a default lightning bolt sprite
    /// </summary>
    Sprite CreateDefaultLightningSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        // Initialize with transparent
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        Color boltColor = lightningColor;
        Color brightColor = Color.white;
        
        // Draw lightning bolt shape (zigzag)
        int centerX = size / 2;
        
        // Lightning bolt path
        for (int y = 0; y < size; y++)
        {
            float normalizedY = (float)y / size;
            int x = centerX;
            
            // Create zigzag pattern
            if (normalizedY < 0.3f)
            {
                x = centerX + (int)(Mathf.Sin(normalizedY * 20f) * 3f);
            }
            else if (normalizedY < 0.7f)
            {
                x = centerX - (int)(Mathf.Sin(normalizedY * 15f) * 4f);
            }
            else
            {
                x = centerX + (int)(Mathf.Sin(normalizedY * 25f) * 2f);
            }
            
            x = Mathf.Clamp(x, 2, size - 3);
            
            // Draw bolt with thickness
            for (int offset = -1; offset <= 1; offset++)
            {
                int px = x + offset;
                if (px >= 0 && px < size)
                {
                    int index = y * size + px;
                    if (index >= 0 && index < pixels.Length)
                    {
                        pixels[index] = offset == 0 ? brightColor : boltColor;
                    }
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }
    
    void Start()
    {
        // Check GameManager for initial time state
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            isDayTime = gameManager.isDayTime;
            UpdateVisuals();
        }
        else
        {
            // Default to day if no GameManager found
            isDayTime = true;
            UpdateVisuals();
        }
    }

    void Update()
    {
        if (isCollected) return;
        
        // Handle shake animation
        if (isShaking)
        {
            shakeTimer += Time.deltaTime;
            float shakeDuration = 0.5f; // Total duration for 2 shakes (left-right-left-right)
            float shakeAmount = 0.3f; // How far to shake
            float shakeSpeed = 20f; // Speed of shake
            
            if (shakeTimer < shakeDuration)
            {
                // Shake left and right twice
                float xOffset = Mathf.Sin(shakeTimer * shakeSpeed) * shakeAmount;
                transform.position = originalPosition + new Vector3(xOffset, 0f, 0f);
            }
            else
            {
                // Stop shaking
                isShaking = false;
                shakeTimer = 0f;
                transform.position = originalPosition;
            }
        }
        else
        {
            // Normal rotation and floating
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            
            // Float up and down
            floatTimer += Time.deltaTime * floatSpeed;
            float yOffset = Mathf.Sin(floatTimer) * floatAmplitude;
            transform.position = startPosition + new Vector3(0f, yOffset, 0f);
            originalPosition = startPosition + new Vector3(0f, yOffset, 0f);
        }
    }
    
    /// <summary>
    /// Start shake animation - shakes left and right twice
    /// </summary>
    public void StartShakeAnimation()
    {
        if (isCollected) return;
        isShaking = true;
        shakeTimer = 0f;
        originalPosition = transform.position;
    }

    /// <summary>
    /// Called by TimeOfDayController when day/night changes
    /// </summary>
    public void SetTimeOfDay(bool isDay)
    {
        isDayTime = isDay;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        // Lightning bolts are available at any point (day or night)
        spriteRenderer.enabled = true;
        circleCollider.enabled = true;
        
        // Use glow color at night, normal color during day
        if (!isDayTime && glowsAtNight)
        {
            spriteRenderer.color = nightGlowColor;
        }
        else
        {
            spriteRenderer.color = lightningColor;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player collected it
        if (isCollected) return;
        
        // Assuming player has a tag "Player" or a component like "PlayerController"
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            CollectBolt();
        }
    }

    void CollectBolt()
    {
        if (isCollected) return;
        isCollected = true;
        
        // Notify game manager or player that bolt was collected
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.CollectLightningBolt(boltValue);
        }
        
        // Play collection effect
        if (collectionEffect != null)
        {
            Instantiate(collectionEffect, transform.position, Quaternion.identity);
        }
        
        // Play sound if available
        if (collectionSound != null)
        {
            AudioSource.PlayClipAtPoint(collectionSound, transform.position);
        }
        
        // Disable visuals and collider
        spriteRenderer.enabled = false;
        circleCollider.enabled = false;
        
        // Destroy after a short delay to allow effects to play
        Destroy(gameObject, 0.5f);
    }

    void OnValidate()
    {
        if (spriteRenderer != null && lightningSprite != null)
        {
            spriteRenderer.sprite = lightningSprite;
            UpdateVisuals();
        }
    }
}

