using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Basic player controller - placeholder for player movement and interaction.
/// Lightning bolts will check for this component when collecting.
/// Uses the new Input System.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 6f; // Reduced jump height
    [Tooltip("Movement speed multiplier when in air (0.5 = half speed)")]
    public float airMovementMultiplier = 0.5f; // Half speed in air
    [Tooltip("Maximum step height the player can walk up (in units)")]
    public float maxStepHeight = 1.1f; // Slightly more than 1 block to allow stepping up
    
    private Rigidbody2D rb;
    private bool isGrounded = false;
    private Vector2 lastPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Configure Rigidbody2D for platformer
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Ensure we have a sprite renderer for visibility
        if (GetComponent<SpriteRenderer>() == null)
        {
            SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
            sr.color = Color.blue;
            sr.sprite = CreateDefaultPlayerSprite();
        }
        
        // Ensure we have a collider - use BoxCollider2D for 3x2 shape
        // Player is 3x2: width is 3/4, height is 1/2 of a block (0.75 x 0.5 units)
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.75f, 0.5f); // 3x2: width 3/4, height 1/2
        }
    }
    
    /// <summary>
    /// Create a detailed character sprite - looks like an actual character
    /// 3x2 ratio: width is 3/4 of a block, height is 1/2 of a block
    /// </summary>
    Sprite CreateDefaultPlayerSprite()
    {
        // Player should be 3x2: width is 3/4, height is 1/2 of a block
        // If block is 32 pixels (at 32 pixels per unit):
        // width = 24 pixels (3/4 of 32), height = 16 pixels (1/2 of 32)
        int width = 24;  // 3/4 of block width
        int height = 16; // 1/2 of block height
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        // Initialize with transparent background
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        // Define colors for the character
        Color bodyColor = new Color(0.2f, 0.5f, 0.9f); // Blue shirt
        Color outlineColor = new Color(0.05f, 0.2f, 0.5f); // Dark blue outline
        Color headColor = new Color(1f, 0.85f, 0.7f); // Skin tone
        Color eyeColor = Color.black;
        Color pantsColor = new Color(0.3f, 0.2f, 0.1f); // Brown pants
        Color shoeColor = new Color(0.2f, 0.2f, 0.2f); // Dark shoes
        Color highlightColor = new Color(0.4f, 0.7f, 1f); // Light blue highlight
        
        // Draw character from bottom to top
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                float normalizedY = (float)y / height; // 0 at bottom, 1 at top
                float normalizedX = (float)x / width; // 0 at left, 1 at right
                
                // Feet/Shoes (bottom 15%)
                if (normalizedY < 0.15f)
                {
                    float centerX = 0.5f;
                    float distFromCenter = Mathf.Abs(normalizedX - centerX);
                    if (distFromCenter < 0.35f)
                    {
                        pixels[index] = shoeColor;
                        if (distFromCenter > 0.3f)
                        {
                            pixels[index] = outlineColor;
                        }
                    }
                }
                // Legs/Pants (15% to 45%)
                else if (normalizedY < 0.45f)
                {
                    float centerX = 0.5f;
                    float distFromCenter = Mathf.Abs(normalizedX - centerX);
                    float legWidth = 0.25f;
                    
                    if (distFromCenter < legWidth)
                    {
                        pixels[index] = pantsColor;
                        if (distFromCenter > legWidth - 0.05f)
                        {
                            pixels[index] = outlineColor;
                        }
                    }
                }
                // Body/Torso (45% to 70%)
                else if (normalizedY < 0.7f)
                {
                    float centerX = 0.5f;
                    float distFromCenter = Mathf.Abs(normalizedX - centerX);
                    float bodyWidth = 0.4f;
                    
                    if (distFromCenter < bodyWidth)
                    {
                        bool isEdge = distFromCenter > bodyWidth - 0.08f;
                        if (isEdge)
                        {
                            pixels[index] = outlineColor;
                        }
                        else
                        {
                            pixels[index] = bodyColor;
                            
                            // Add highlight on left side
                            if (normalizedX < 0.5f && distFromCenter < bodyWidth * 0.6f)
                            {
                                pixels[index] = Color.Lerp(bodyColor, highlightColor, 0.4f);
                            }
                        }
                    }
                }
                // Head (top 30%)
                else
                {
                    float headY = (normalizedY - 0.7f) / 0.3f; // 0 to 1 within head area
                    float centerX = 0.5f;
                    float distFromCenter = Mathf.Abs(normalizedX - centerX);
                    
                    // Head shape - circular/rounded
                    float headRadius = 0.28f;
                    float headDist = Mathf.Sqrt(distFromCenter * distFromCenter + (1f - headY) * (1f - headY) * 0.6f);
                    
                    if (headDist < headRadius)
                    {
                        // Head outline
                        if (headDist > headRadius - 0.06f)
                        {
                            pixels[index] = outlineColor;
                        }
                        else
                        {
                            pixels[index] = headColor;
                            
                            // Eyes (more visible)
                            if (headY > 0.4f && headY < 0.7f)
                            {
                                float eyeDist1 = Mathf.Abs(normalizedX - 0.42f);
                                float eyeDist2 = Mathf.Abs(normalizedX - 0.58f);
                                if (eyeDist1 < 0.04f || eyeDist2 < 0.04f)
                                {
                                    pixels[index] = eyeColor;
                                }
                            }
                            
                            // Mouth (small smile)
                            if (headY > 0.75f && headY < 0.85f && distFromCenter < 0.08f)
                            {
                                pixels[index] = new Color(0.3f, 0.2f, 0.2f);
                            }
                            
                            // Highlight on head
                            if (normalizedX < 0.5f && headY > 0.3f && headY < 0.6f)
                            {
                                pixels[index] = Color.Lerp(headColor, Color.white, 0.3f);
                            }
                        }
                    }
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        
        // Use 32 pixels per unit to match block size
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f);
    }

    void Update()
    {
        // Read input directly from Keyboard (new Input System)
        float horizontal = 0f;
        bool jumpPressed = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                horizontal = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                horizontal = 1f;
            
            jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
        }

        // Basic horizontal movement
        // Reduce speed by half when in air
        float currentMoveSpeed = isGrounded ? moveSpeed : moveSpeed * airMovementMultiplier;
        
        // Prevent getting stuck against walls - check if we're pushing against a wall
        if (horizontal != 0)
        {
            // Check if there's a wall in the direction we're trying to move
            BoxCollider2D playerCollider = GetComponent<BoxCollider2D>();
            if (playerCollider != null)
            {
                Vector2 rayOrigin = new Vector2(
                    transform.position.x,
                    transform.position.y
                );
                float rayDistance = playerCollider.size.x * 0.5f + 0.1f;
                RaycastHit2D wallCheck = Physics2D.Raycast(rayOrigin, new Vector2(horizontal, 0f), rayDistance);
                
                // If we're pushing against a wall, allow sliding up/down
                if (wallCheck.collider != null && 
                    (wallCheck.collider.GetComponent<FloorBlock>() != null ||
                     wallCheck.collider.GetComponent<BoxBlock>() != null ||
                     wallCheck.collider.name.Contains("Boundary")))
                {
                    // Allow vertical movement even when pushing against wall
                    // Don't zero out horizontal velocity, but allow it to be reduced
                    // This prevents getting completely stuck
                    rb.linearVelocity = new Vector2(horizontal * currentMoveSpeed * 0.3f, rb.linearVelocity.y);
                }
                else
                {
                    // Normal movement - try to step up if grounded
                    if (horizontal != 0 && isGrounded)
                    {
                        TryStepUp(horizontal, currentMoveSpeed);
                    }
                    else
                    {
                        rb.linearVelocity = new Vector2(horizontal * currentMoveSpeed, rb.linearVelocity.y);
                    }
                }
            }
        }
        else
        {
            // No input - maintain vertical velocity but reduce horizontal
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.9f, rb.linearVelocity.y);
        }
        
        // Jump - only when grounded (can't jump in mid-air)
        if (jumpPressed && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
        
        lastPosition = transform.position;
    }
    
    /// <summary>
    /// Try to step up when encountering a block that's 1 block height difference
    /// Allows player to walk up steps automatically without jumping
    /// </summary>
    void TryStepUp(float horizontal, float moveSpeed)
    {
        BoxCollider2D playerCollider = GetComponent<BoxCollider2D>();
        if (playerCollider == null) return;
        
        // Calculate player bounds
        float playerHalfWidth = playerCollider.size.x * 0.5f;
        float playerHalfHeight = playerCollider.size.y * 0.5f;
        float playerBottom = transform.position.y - playerHalfHeight;
        
        // Cast multiple rays forward at different heights to detect steps
        for (float rayHeight = 0.05f; rayHeight <= maxStepHeight; rayHeight += 0.1f)
        {
            Vector2 rayOrigin = new Vector2(
                transform.position.x + (horizontal * (playerHalfWidth + 0.05f)),
                playerBottom + rayHeight
            );
            Vector2 rayDirection = new Vector2(horizontal, 0f);
            float rayDistance = 0.15f;
            
            RaycastHit2D hitForward = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance);
            
            if (hitForward.collider != null)
            {
                // Check if it's a block we can step on
                if (hitForward.collider.GetComponent<FloorBlock>() != null ||
                    hitForward.collider.GetComponent<BoxBlock>() != null)
                {
                    // Get the top of the block
                    float blockTop = hitForward.collider.bounds.max.y;
                    float heightDiff = blockTop - playerBottom;
                    
                    // If the step is exactly 1 block (or slightly less), step up
                    if (heightDiff > 0.05f && heightDiff <= maxStepHeight)
                    {
                        // Check if there's space above the block for the player
                        Vector2 checkPosition = new Vector2(
                            hitForward.point.x + (horizontal * playerHalfWidth * 0.3f),
                            blockTop + playerHalfHeight + 0.02f
                        );
                        
                        // Use OverlapBox to check if player fits
                        Collider2D overlap = Physics2D.OverlapBox(
                            checkPosition,
                            playerCollider.size * 0.95f,
                            0f
                        );
                        
                        // If no overlap (or only overlapping the block we're stepping on), we can step up
                        if (overlap == null || overlap == hitForward.collider)
                        {
                            // Step up - smoothly move player to top of block
                            Vector3 newPosition = new Vector3(
                                transform.position.x + (horizontal * moveSpeed * Time.deltaTime),
                                blockTop + playerHalfHeight + 0.01f,
                                transform.position.z
                            );
                            
                            transform.position = newPosition;
                            rb.linearVelocity = new Vector2(horizontal * moveSpeed, 0f);
                            isGrounded = true; // We're now on top of the block
                            return;
                        }
                    }
                }
            }
        }
        
        // Normal movement if can't step up
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle sliding when hitting blocks from the side
        HandleCollisionSliding(collision);
        
        // Check if grounded - check if collision is from below
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // If the contact point is below the player center, we're grounded
            if (contact.normal.y > 0.5f) // Normal pointing up means we hit something below
            {
                if (collision.gameObject.CompareTag("Ground") || 
                    collision.gameObject.GetComponent<FloorBlock>() != null ||
                    collision.gameObject.GetComponent<BoxBlock>() != null ||
                    collision.gameObject.name.Contains("Boundary")) // Check boundaries by name
                {
                    isGrounded = true;
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Handle sliding when player hits blocks from the side
    /// Prevents getting stuck in blocks - allows sliding along walls
    /// Works even when pressing buttons against walls
    /// </summary>
    void HandleCollisionSliding(Collision2D collision)
    {
        // Only slide if hitting a block or boundary
        if (collision.gameObject.GetComponent<FloorBlock>() == null &&
            collision.gameObject.GetComponent<BoxBlock>() == null &&
            !collision.gameObject.name.Contains("Boundary"))
        {
            return;
        }
        
        foreach (ContactPoint2D contact in collision.contacts)
        {
            Vector2 normal = contact.normal;
            
            // If hitting from the side (horizontal collision)
            if (Mathf.Abs(normal.x) > 0.3f)
            {
                // Get current input direction
                float moveInput = 0f;
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                        moveInput = -1f;
                    if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                        moveInput = 1f;
                }
                
                // If player is pressing against the wall, allow sliding up/down
                // This prevents getting stuck when pressing buttons against walls
                if (Mathf.Sign(moveInput) == Mathf.Sign(normal.x))
                {
                    // Player is pressing into the wall - allow vertical sliding
                    // Don't block vertical movement
                    Vector2 velocity = rb.linearVelocity;
                    
                    // Reduce horizontal velocity into the wall
                    float wallComponent = Vector2.Dot(velocity, normal);
                    if (wallComponent < 0) // Moving into wall
                    {
                        velocity -= normal * wallComponent * 0.8f; // Reduce but don't eliminate
                        rb.linearVelocity = velocity;
                    }
                    
                    // Allow sliding along the wall (vertical movement)
                    // Don't interfere with vertical velocity
                    return;
                }
                
                // If not pressing into wall, allow normal sliding
                // Reduce velocity component into the wall
                Vector2 vel = rb.linearVelocity;
                float wallComp = Vector2.Dot(vel, normal);
                if (wallComp < 0) // Moving into wall
                {
                    vel -= normal * wallComp;
                    rb.linearVelocity = vel;
                }
            }
        }
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        // Handle sliding continuously to prevent getting stuck
        HandleCollisionSliding(collision);
        
        // Keep checking if grounded while colliding
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                if (collision.gameObject.CompareTag("Ground") || 
                    collision.gameObject.GetComponent<FloorBlock>() != null ||
                    collision.gameObject.GetComponent<BoxBlock>() != null ||
                    collision.gameObject.name.Contains("Boundary"))
                {
                    isGrounded = true;
                    break;
                }
            }
        }
    }
    
    void OnCollisionExit2D(Collision2D collision)
    {
        // When leaving ground, mark as not grounded
        // This prevents double jumping
        if (collision.gameObject.CompareTag("Ground") || 
            collision.gameObject.GetComponent<FloorBlock>() != null ||
            collision.gameObject.GetComponent<BoxBlock>() != null ||
            collision.gameObject.name.Contains("Boundary"))
        {
            // Only set to false if we're moving away (not just touching)
            if (rb.linearVelocity.y > 0.1f) // Moving up
            {
                isGrounded = false;
            }
        }
    }
    
    void FixedUpdate()
    {
        // Additional check: if falling and not touching anything, not grounded
        if (rb.linearVelocity.y < -0.1f && !isGrounded)
        {
            // Already not grounded, this is fine
        }
    }
}

