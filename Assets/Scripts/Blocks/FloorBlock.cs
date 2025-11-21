using UnityEngine;

/// <summary>
/// Floor block - solid ground that players can walk on.
/// Always visible, doesn't react to day/night cycle.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class FloorBlock : MonoBehaviour
{
    [Header("Floor Settings")]
    public Sprite floorSprite;
    public Color floorColor = Color.white;
    
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        // Setup floor properties
        if (floorSprite != null)
        {
            spriteRenderer.sprite = floorSprite;
        }
        
        spriteRenderer.color = floorColor;
        
        // Ensure collider is enabled for solid ground
        boxCollider.enabled = true;
        boxCollider.isTrigger = false;
    }

    void OnValidate()
    {
        // Update visuals in editor when values change
        if (spriteRenderer != null && floorSprite != null)
        {
            spriteRenderer.sprite = floorSprite;
            spriteRenderer.color = floorColor;
        }
    }
}

