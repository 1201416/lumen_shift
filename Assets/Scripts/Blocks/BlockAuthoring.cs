using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BlockAuthoring : MonoBehaviour
{
    public BlockType blockType;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ApplyBlockType(dayState: true);
    }

    public void ApplyBlockType(bool dayState)
    {
        if (blockType == null || spriteRenderer == null) return;

        spriteRenderer.sprite = blockType.sprite;
        spriteRenderer.color = dayState ? blockType.dayTint : blockType.nightTint;

        var collider = GetComponent<Collider2D>();
        if (blockType.hasCollider)
        {
            if (collider == null) collider = gameObject.AddComponent<BoxCollider2D>();
            collider.enabled = true;
        }
        else if (collider != null)
        {
            collider.enabled = false;
        }
    }
}