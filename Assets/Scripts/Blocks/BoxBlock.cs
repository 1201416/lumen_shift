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

    void SetupBox()
    {
        if (boxSprite != null)
        {
            spriteRenderer.sprite = boxSprite;
        }
        
        UpdateVisuals();
        
        // Setup collider
        boxCollider.enabled = true;
        boxCollider.isTrigger = false;
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
        spriteRenderer.color = isDayTime ? dayColor : nightColor;
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
        if (spriteRenderer != null && boxSprite != null)
        {
            spriteRenderer.sprite = boxSprite;
            UpdateVisuals();
        }
    }
}

