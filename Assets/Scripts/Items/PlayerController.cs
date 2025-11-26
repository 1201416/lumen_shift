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
    
    // Public getters for animation controller
    public float GetHorizontalVelocity() => rb != null ? rb.linearVelocity.x : 0f;
    public float GetVerticalVelocity() => rb != null ? rb.linearVelocity.y : 0f;
    public bool IsGrounded() => isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Configure Rigidbody2D for platformer
        rb.bodyType = RigidbodyType2D.Dynamic; // Must be Dynamic for physics collisions
        rb.freezeRotation = true; // Prevent rotation (character should stay upright)
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
        rb.gravityScale = 3f; // Set gravity if not already set
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Smooth movement
        
        // Ensure we have a sprite renderer for visibility
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Only create and set default sprite if no sprite is already assigned
        // This allows manually assigned sprites (like Virtual Guy) to be preserved
        if (sr.sprite == null)
        {
            Sprite playerSprite = CreateDefaultPlayerSprite();
            sr.sprite = playerSprite;
        }
        
        sr.color = Color.white; // Use white to show sprite colors properly
        sr.sortingOrder = 1; // Ensure character is above ground
        sr.drawMode = SpriteDrawMode.Simple;
        
        // Ensure we have a collider - check for existing collider first
        Collider2D existingCollider = GetComponent<Collider2D>();
        if (existingCollider == null)
        {
            // No collider exists, add BoxCollider2D
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            // Size based on sprite: 28/32 = 0.875, 18/32 = 0.5625
            col.size = new Vector2(0.875f, 0.5625f); // Proportional to sprite size
            col.isTrigger = false; // Must be solid for physics
            col.usedByEffector = false; // Don't use physics materials that might affect collision
        }
        else
        {
            // Collider exists (like CapsuleCollider2D), ensure it's configured correctly
            existingCollider.isTrigger = false; // Must be solid for physics
            
            // Auto-size collider to match sprite EXACTLY if sprite exists
            if (sr.sprite != null)
            {
                Vector2 spriteSize = sr.sprite.bounds.size;
                if (existingCollider is BoxCollider2D boxCol)
                {
                    boxCol.size = spriteSize; // Exact match to sprite size
                    boxCol.usedByEffector = false;
                }
                else if (existingCollider is CapsuleCollider2D capsuleCol)
                {
                    // For capsule, match sprite dimensions exactly
                    capsuleCol.size = spriteSize; // Exact match
                    capsuleCol.usedByEffector = false;
                }
            }
        }
        
        // Ensure transform rotation is correct (facing right, not sideways)
        transform.rotation = Quaternion.identity;
    }
    
    /// <summary>
    /// Create a detailed character sprite - looks like an actual character
    /// 3x2 ratio: width is 3/4 of a block, height is 1/2 of a block
    /// </summary>
    Sprite CreateDefaultPlayerSprite()
    {
        // Player should be 3x2: width is 3/4, height is 1/2 of a block
        // Make it 75% of original size for better visibility
        // Original: 24x16, but we'll use a slightly larger base for better detail
        // Using 32x20 as base, then 75% would be 24x15, but let's use 28x18 for better detail
        // If block is 32 pixels (at 32 pixels per unit):
        int width = 28;  // Good balance: visible but not too big
        int height = 18; // Maintains 3x2 ratio approximately
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
        texture.filterMode = FilterMode.Point; // Use Point filter for pixel art clarity
        texture.wrapMode = TextureWrapMode.Clamp;
        
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
            
            // Jump with space bar OR up arrow
            jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame || 
                         Keyboard.current.upArrowKey.wasPressedThisFrame;
        }

        // Basic horizontal movement
        // Reduce speed by half when in air
        float currentMoveSpeed = isGrounded ? moveSpeed : moveSpeed * airMovementMultiplier;
        
        // Prevent getting stuck against walls - check if we're pushing against a wall
        if (horizontal != 0)
        {
            // Check if there's a wall in the direction we're trying to move
            Collider2D playerCollider = GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                Vector2 rayOrigin = new Vector2(
                    transform.position.x,
                    transform.position.y
                );
                
                // Get collider bounds for ray distance calculation
                float colliderWidth = playerCollider.bounds.size.x;
                float rayDistance = colliderWidth * 0.5f + 0.1f;
                
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
            else
            {
                // No collider - just move normally
                rb.linearVelocity = new Vector2(horizontal * currentMoveSpeed, rb.linearVelocity.y);
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
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null) return;
        
        // Calculate player bounds using bounds (works with any collider type)
        float playerHalfWidth = playerCollider.bounds.size.x * 0.5f;
        float playerHalfHeight = playerCollider.bounds.size.y * 0.5f;
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
                        
                        // Use OverlapBox to check if player fits (use bounds size)
                        Collider2D overlap = Physics2D.OverlapBox(
                            checkPosition,
                            playerCollider.bounds.size * 0.95f,
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
        // CRITICAL: Check if player is hitting the bottom of a block (head collision)
        // If so, prevent upward movement/teleportation
        if (IsHittingBlockFromBelow(collision))
        {
            // Player is underneath a block - prevent being pushed up
            PreventUpwardPush(collision);
            return; // Don't process as normal collision
        }
        
        // Handle sliding when hitting blocks from the side
        HandleCollisionSliding(collision);
        
        // Check if grounded - check if collision is from below
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // If the contact point is below the player center, we're grounded
            if (contact.normal.y > 0.5f) // Normal pointing up means we hit something below
            {
                if ( 
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
    /// Check if player is hitting a block from below (head hitting bottom of block)
    /// </summary>
    bool IsHittingBlockFromBelow(Collision2D collision)
    {
        // Only check for blocks (not floor or boundaries)
        if (collision.gameObject.GetComponent<BoxBlock>() == null)
        {
            return false;
        }
        
        // Get block bounds
        Collider2D blockCollider = collision.collider;
        if (blockCollider == null) return false;
        
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null) return false;
        
        float blockBottom = blockCollider.bounds.min.y;
        float blockTop = blockCollider.bounds.max.y;
        float playerTop = playerCollider.bounds.max.y;
        float playerBottom = playerCollider.bounds.min.y;
        float playerCenterY = transform.position.y;
        float blockCenterY = (blockBottom + blockTop) * 0.5f;
        
        // Player is hitting from below if:
        // 1. Player's center is clearly below the block's center
        // 2. Player's top is touching or very close to the block's bottom
        // 3. Player is not jumping (vertical velocity is not strongly upward)
        
        bool playerIsBelowBlock = playerCenterY < blockCenterY;
        bool playerTopTouchingBlockBottom = playerTop >= blockBottom - 0.15f && playerTop <= blockBottom + 0.15f;
        bool notJumpingUp = rb.linearVelocity.y <= 1f; // Allow small upward velocity for smooth movement
        
        if (!playerIsBelowBlock || !playerTopTouchingBlockBottom) return false;
        
        // Check contact normals - if they point down (negative Y), we're hitting from below
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Normal pointing down (negative Y) means we hit the bottom of the block
            if (contact.normal.y < -0.5f)
            {
                return true; // Definitely hitting block from below
            }
        }
        
        // Also check if player's top is clearly below block's bottom but touching
        if (playerTopTouchingBlockBottom && playerIsBelowBlock && notJumpingUp)
        {
            return true; // Player is underneath and touching
        }
        
        return false;
    }
    
    /// <summary>
    /// Prevent player from being pushed upward when hitting block from below
    /// Allows player to continue moving horizontally underneath
    /// </summary>
    void PreventUpwardPush(Collision2D collision)
    {
        Collider2D blockCollider = collision.collider;
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (blockCollider == null || playerCollider == null) return;
        
        float blockBottom = blockCollider.bounds.min.y;
        float playerTop = playerCollider.bounds.max.y;
        float playerHeight = playerCollider.bounds.size.y;
        float playerCenterY = transform.position.y;
        
        // Get current velocity
        Vector2 velocity = rb.linearVelocity;
        
        // CRITICAL: Cancel ALL upward velocity immediately
        if (velocity.y > 0f)
        {
            velocity.y = 0f; // Completely stop upward movement
        }
        
        // Calculate desired Y position: block bottom minus half player height minus small gap
        float desiredY = blockBottom - (playerHeight * 0.5f) - 0.02f; // Small gap to prevent sticking
        
        // If player's top is at or above block's bottom, immediately reposition
        if (playerTop >= blockBottom - 0.01f)
        {
            // Immediately set position (no lerping) to prevent teleportation
            transform.position = new Vector3(transform.position.x, desiredY, transform.position.z);
            
            // Force zero or negative velocity
            velocity.y = -0.1f; // Small downward velocity to prevent sticking
        }
        // If player is close but not penetrating, gently adjust
        else if (playerTop > blockBottom - 0.1f)
        {
            // Only adjust if significantly above desired position
            if (transform.position.y > desiredY + 0.02f)
            {
                // Fast correction but preserve horizontal position
                float newY = Mathf.Lerp(transform.position.y, desiredY, Time.fixedDeltaTime * 30f);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
            
            // Ensure downward or zero velocity
            velocity.y = Mathf.Min(velocity.y, 0f);
        }
        
        // Preserve horizontal velocity so player can continue moving underneath
        rb.linearVelocity = velocity;
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
        // CRITICAL: Check if player is hitting the bottom of a block (head collision)
        // If so, prevent upward movement/teleportation
        if (IsHittingBlockFromBelow(collision))
        {
            // Player is underneath a block - prevent being pushed up
            PreventUpwardPush(collision);
            return; // Don't process as normal collision
        }
        
        // Handle sliding continuously to prevent getting stuck
        HandleCollisionSliding(collision);
        
        // Keep checking if grounded while colliding
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                if ( 
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
        if ( 
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

