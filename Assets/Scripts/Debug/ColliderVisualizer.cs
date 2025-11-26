using UnityEngine;

/// <summary>
/// Debug tool to visualize collider bounds in both Scene and Game view
/// Helps verify that collider sizes match sprite sizes
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ColliderVisualizer : MonoBehaviour
{
    [Header("Visualization Settings")]
    public bool showInGameView = false; // Disabled - only show in Scene view for debugging
    public bool showInSceneView = true;
    public Color colliderColor = Color.red;
    public Color spriteBoundsColor = Color.green;
    public float lineWidth = 2f;
    
    private Collider2D targetCollider;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        targetCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void OnDrawGizmos()
    {
        if (!showInSceneView) return;
        
        if (targetCollider == null)
            targetCollider = GetComponent<Collider2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (targetCollider == null) return;
        
        // Draw collider bounds
        DrawColliderBounds();
        
        // Draw sprite bounds if sprite exists
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            DrawSpriteBounds();
        }
    }
    
    
    void DrawColliderBounds()
    {
        if (targetCollider == null) return;
        
        Gizmos.color = colliderColor;
        
        Bounds bounds = targetCollider.bounds;
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;
        
        // Draw rectangle outline
        Vector3 topLeft = center + new Vector3(-size.x / 2, size.y / 2, 0);
        Vector3 topRight = center + new Vector3(size.x / 2, size.y / 2, 0);
        Vector3 bottomLeft = center + new Vector3(-size.x / 2, -size.y / 2, 0);
        Vector3 bottomRight = center + new Vector3(size.x / 2, -size.y / 2, 0);
        
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
        
        // Draw center point
        Gizmos.DrawWireSphere(center, 0.05f);
    }
    
    void DrawSpriteBounds()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;
        
        Gizmos.color = spriteBoundsColor;
        
        Bounds spriteBounds = spriteRenderer.bounds;
        Vector3 center = spriteBounds.center;
        Vector3 size = spriteBounds.size;
        
        // Draw rectangle outline
        Vector3 topLeft = center + new Vector3(-size.x / 2, size.y / 2, 0);
        Vector3 topRight = center + new Vector3(size.x / 2, size.y / 2, 0);
        Vector3 bottomLeft = center + new Vector3(-size.x / 2, -size.y / 2, 0);
        Vector3 bottomRight = center + new Vector3(size.x / 2, -size.y / 2, 0);
        
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
    
    string GetColliderInfo()
    {
        if (targetCollider == null) return "";
        
        string info = "";
        
        // Collider info
        if (targetCollider is BoxCollider2D boxCol)
        {
            info += $"Collider Size: {boxCol.size.x:F3} x {boxCol.size.y:F3}\n";
        }
        else if (targetCollider is CapsuleCollider2D capsuleCol)
        {
            info += $"Collider Size: {capsuleCol.size.x:F3} x {capsuleCol.size.y:F3}\n";
        }
        else if (targetCollider is CircleCollider2D circleCol)
        {
            info += $"Collider Radius: {circleCol.radius:F3}\n";
        }
        
        // Bounds info
        Bounds bounds = targetCollider.bounds;
        info += $"Bounds: {bounds.size.x:F3} x {bounds.size.y:F3}\n";
        
        // Sprite info
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Bounds spriteBounds = spriteRenderer.sprite.bounds;
            info += $"Sprite Bounds: {spriteBounds.size.x:F3} x {spriteBounds.size.y:F3}\n";
            
            // Compare
            float sizeDiffX = Mathf.Abs(bounds.size.x - spriteBounds.size.x);
            float sizeDiffY = Mathf.Abs(bounds.size.y - spriteBounds.size.y);
            info += $"Difference: {sizeDiffX:F3} x {sizeDiffY:F3}\n";
            
            if (sizeDiffX > 0.01f || sizeDiffY > 0.01f)
            {
                info += "⚠️ MISMATCH!";
            }
            else
            {
                info += "✓ Match";
            }
        }
        
        return info;
    }
    
    void Update()
    {
        // Log collider info periodically (every 2 seconds)
        if (Time.frameCount % 120 == 0 && targetCollider != null)
        {
            string info = GetColliderInfo();
            if (!string.IsNullOrEmpty(info))
            {
                Debug.Log($"[{gameObject.name}] Collider Info:\n{info}", this);
            }
        }
    }
}

