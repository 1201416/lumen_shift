using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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
    private Sprite lastSprite = null; // Track sprite changes to avoid unnecessary updates
    
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
        
        // Ensure we have a collider based on sprite shape (PolygonCollider2D)
        // This creates a hitbox that matches the actual sprite pixels, not a rectangle
        Collider2D existingCollider = GetComponent<Collider2D>();
        
        // Remove existing BoxCollider2D or CapsuleCollider2D if present (we want PolygonCollider2D)
        if (existingCollider != null && (existingCollider is BoxCollider2D || existingCollider is CapsuleCollider2D))
        {
            #if UNITY_EDITOR
            DestroyImmediate(existingCollider);
            #else
            Destroy(existingCollider);
            #endif
            existingCollider = null;
        }
        
        PolygonCollider2D polygonCol = GetComponent<PolygonCollider2D>();
        if (polygonCol == null)
        {
            // No polygon collider exists, add one
            polygonCol = gameObject.AddComponent<PolygonCollider2D>();
        }
        
        polygonCol.isTrigger = false; // Must be solid for physics
        polygonCol.usedByEffector = false; // Don't use physics materials that might affect collision
        
        // CRITICAL: Generate polygon collider from sprite shape
        // This will create a hitbox that matches the actual sprite pixels
        if (sr.sprite != null)
        {
            SetupPolygonColliderFromSprite(polygonCol, sr.sprite);
        }
        
        // Ensure transform rotation is correct (facing right, not sideways)
        transform.rotation = Quaternion.identity;
        
        // Add collider visualizer for debugging (only in editor)
        #if UNITY_EDITOR
        if (GetComponent<ColliderVisualizer>() == null)
        {
            ColliderVisualizer visualizer = gameObject.AddComponent<ColliderVisualizer>();
            visualizer.colliderColor = new Color(1f, 0f, 0f, 0.8f); // Red for collider
            visualizer.spriteBoundsColor = new Color(0f, 1f, 0f, 0.8f); // Green for sprite
            visualizer.showInGameView = false; // Only show in Scene view
            visualizer.showInSceneView = true;
        }
        #endif
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
                    // BUT: Don't step up if we're underneath a block (check first)
                    // CRITICAL: Also check if there's a block directly ahead and above
                    bool canStepUp = horizontal != 0 && isGrounded && !IsUnderneathBlock() && !IsBlockAheadAndAbove(horizontal);
                    if (canStepUp)
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
    /// Check if there's a block ahead and above the player (prevents stepping up into blocks)
    /// </summary>
    bool IsBlockAheadAndAbove(float horizontal)
    {
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null) return false;
        
        float playerTop = playerCollider.bounds.max.y;
        float playerBottom = playerCollider.bounds.min.y;
        float playerHeight = playerCollider.bounds.size.y;
        
        // Check if there's a block ahead and above player's current position
        Vector2 rayOrigin = new Vector2(
            transform.position.x + (horizontal * playerCollider.bounds.size.x * 0.6f),
            playerTop + 0.1f // Check slightly above player
        );
        
        // Cast upward to see if there's a block
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, playerHeight * 0.5f);
        if (hit.collider != null)
        {
            if (hit.collider.GetComponent<BoxBlock>() != null || 
                hit.collider.GetComponent<FloorBlock>() != null)
            {
                // There's a block above - don't step up
                return true;
            }
        }
        
        return false;
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
                        
                        // CRITICAL: Check if there's a block above the step-up position
                        // Cast upward from the step-up position
                        RaycastHit2D blockAbove = Physics2D.Raycast(
                            checkPosition + Vector2.up * playerHalfHeight * 0.5f,
                            Vector2.up,
                            playerHalfHeight * 0.8f
                        );
                        
                        // If there's a block above, don't step up
                        if (blockAbove.collider != null && 
                            (blockAbove.collider.GetComponent<BoxBlock>() != null || 
                             blockAbove.collider.GetComponent<FloorBlock>() != null))
                        {
                            // Block above - can't step up
                            return;
                        }
                        
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
    /// Check if player is currently underneath any block (using physics overlap)
    /// More aggressive detection to prevent teleportation
    /// </summary>
    bool IsUnderneathBlock()
    {
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null) return false;
        
        // Check if there's a block above the player using multiple methods
        Vector2 playerTop = new Vector2(transform.position.x, playerCollider.bounds.max.y);
        float checkDistance = 1f; // Check up to 1 unit above player (more aggressive)
        
        // Method 1: Raycast upward
        RaycastHit2D hit = Physics2D.Raycast(playerTop, Vector2.up, checkDistance);
        if (hit.collider != null)
        {
            // Check if it's a block
            if (hit.collider.GetComponent<BoxBlock>() != null)
            {
                return true; // There's a block above
            }
        }
        
        // Method 2: Overlap check at player's top
        Collider2D[] overlaps = Physics2D.OverlapPointAll(playerTop);
        foreach (Collider2D col in overlaps)
        {
            if (col != null && col.GetComponent<BoxBlock>() != null && col != playerCollider)
            {
                return true; // There's a block at player's top
            }
        }
        
        // Method 3: Check slightly above player's top
        Vector2 checkAbove = playerTop + Vector2.up * 0.1f;
        overlaps = Physics2D.OverlapPointAll(checkAbove);
        foreach (Collider2D col in overlaps)
        {
            if (col != null && col.GetComponent<BoxBlock>() != null && col != playerCollider)
            {
                return true; // There's a block above player
            }
        }
        
        return false;
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
        
        bool playerIsBelowBlock = playerCenterY < blockCenterY;
        bool playerTopTouchingBlockBottom = playerTop >= blockBottom - 0.2f && playerTop <= blockBottom + 0.2f;
        
        if (!playerIsBelowBlock || !playerTopTouchingBlockBottom) return false;
        
        // Check contact normals - if they point down (negative Y), we're hitting from below
        bool hasDownwardNormal = false;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Normal pointing down (negative Y) means we hit the bottom of the block
            if (contact.normal.y < -0.3f)
            {
                hasDownwardNormal = true;
                break;
            }
        }
        
        // If we have a downward normal OR player is clearly underneath, it's a head collision
        if (hasDownwardNormal || (playerTopTouchingBlockBottom && playerIsBelowBlock))
        {
            return true; // Player is hitting block from below
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
        
        // CRITICAL: Cancel ALL upward velocity immediately - prevent any jumping up
        if (velocity.y > 0f)
        {
            velocity.y = -0.2f; // Force downward velocity to prevent sticking and teleportation
        }
        
        // Calculate desired Y position: block bottom minus half player height minus gap
        float desiredY = blockBottom - (playerHeight * 0.5f) - 0.08f; // Larger gap to prevent sticking
        
        // CRITICAL: If player's top is at or above block's bottom, IMMEDIATELY reposition
        // This prevents Unity's physics from pushing the player up
        if (playerTop >= blockBottom - 0.05f)
        {
            // Immediately set position (no lerping) to prevent teleportation
            transform.position = new Vector3(transform.position.x, desiredY, transform.position.z);
            
            // Force strong downward velocity to prevent any upward movement
            velocity.y = -0.3f; // Stronger downward velocity
        }
        // If player is close but not penetrating, adjust quickly
        else if (playerTop > blockBottom - 0.2f)
        {
            // Only adjust if significantly above desired position
            if (transform.position.y > desiredY + 0.05f)
            {
                // Very fast correction but preserve horizontal position
                float newY = Mathf.Lerp(transform.position.y, desiredY, Time.fixedDeltaTime * 60f);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
            
            // Ensure downward velocity (never upward)
            velocity.y = Mathf.Min(velocity.y, -0.1f); // Always downward
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
        // If so, prevent upward movement/teleportation - check this FIRST
        if (IsHittingBlockFromBelow(collision))
        {
            // Player is underneath a block - prevent being pushed up
            PreventUpwardPush(collision);
            return; // Don't process as normal collision
        }
        
        // Handle sliding continuously to prevent getting stuck
        HandleCollisionSliding(collision);
        
        // Keep checking if grounded while colliding (but only if not underneath a block)
        if (!IsUnderneathBlock())
        {
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
        
        // CRITICAL: Continuously check if we're underneath a block and prevent upward movement
        // This prevents Unity's physics from pushing us up
        if (IsUnderneathBlock())
        {
            // Force downward velocity to prevent any upward movement
            if (rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -0.2f);
            }
        }
    }
    
    /// <summary>
    /// Setup PolygonCollider2D to match the sprite's actual shape (pixels)
    /// This creates a hitbox that follows the sprite image, not a rectangle
    /// </summary>
    void SetupPolygonColliderFromSprite(PolygonCollider2D polygonCol, Sprite sprite)
    {
        if (sprite == null || polygonCol == null) return;
        
        // Unity can automatically generate polygon collider paths from sprite physics shape
        // First, try to use the sprite's physics shape if available (best option)
        if (sprite.GetPhysicsShapeCount() > 0)
        {
            // Use the sprite's physics shape directly - this is the most accurate
            polygonCol.pathCount = sprite.GetPhysicsShapeCount();
            for (int i = 0; i < sprite.GetPhysicsShapeCount(); i++)
            {
                List<Vector2> path = new List<Vector2>();
                sprite.GetPhysicsShape(i, path);
                polygonCol.SetPath(i, path);
            }
        }
        else
        {
            // If no physics shape, generate polygon from sprite texture pixels
            // This creates a hitbox that matches the actual visible pixels
            GeneratePolygonFromSpritePixels(polygonCol, sprite);
        }
    }
    
    /// <summary>
    /// Generate a polygon collider from sprite pixels (alpha channel)
    /// Creates a hitbox that matches only the visible parts of the sprite
    /// </summary>
    void GeneratePolygonFromSpritePixels(PolygonCollider2D polygonCol, Sprite sprite)
    {
        if (sprite == null || polygonCol == null) return;
        
        Texture2D texture = sprite.texture;
        if (texture == null) return;
        
        // Check if texture is readable (some textures are compressed and not readable)
        try
        {
            // Try to read pixels - this will fail if texture is not readable
            texture.GetPixel(0, 0);
        }
        catch
        {
            // Texture is not readable, use sprite bounds as fallback
            Bounds bounds = sprite.bounds;
            Vector2[] points = new Vector2[4];
            points[0] = new Vector2(bounds.min.x, bounds.min.y);
            points[1] = new Vector2(bounds.max.x, bounds.min.y);
            points[2] = new Vector2(bounds.max.x, bounds.max.y);
            points[3] = new Vector2(bounds.min.x, bounds.max.y);
            polygonCol.pathCount = 1;
            polygonCol.SetPath(0, points);
            return;
        }
        
        // Get sprite rect in texture coordinates
        Rect spriteRect = sprite.textureRect;
        int width = (int)spriteRect.width;
        int height = (int)spriteRect.height;
        
        // Read pixels from sprite region
        Color[] pixels = texture.GetPixels(
            (int)spriteRect.x,
            (int)spriteRect.y,
            width,
            height
        );
        
        // Find the bounding box of non-transparent pixels
        int minX = width, maxX = 0, minY = height, maxY = 0;
        bool foundPixel = false;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (pixels[index].a > 0.1f) // Non-transparent pixel
                {
                    foundPixel = true;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }
        
        if (!foundPixel)
        {
            // Fallback to sprite bounds if no pixels found
            Bounds bounds = sprite.bounds;
            Vector2[] points = new Vector2[4];
            points[0] = new Vector2(bounds.min.x, bounds.min.y);
            points[1] = new Vector2(bounds.max.x, bounds.min.y);
            points[2] = new Vector2(bounds.max.x, bounds.max.y);
            points[3] = new Vector2(bounds.min.x, bounds.max.y);
            polygonCol.pathCount = 1;
            polygonCol.SetPath(0, points);
            return;
        }
        
        // Convert pixel coordinates to world coordinates
        // Sprite pivot is at center (0.5, 0.5) based on sprite creation
        float pixelToWorld = sprite.pixelsPerUnit;
        float spriteWidthWorld = width / pixelToWorld;
        float spriteHeightWorld = height / pixelToWorld;
        
        // Calculate world positions relative to sprite center
        float left = (minX - width * 0.5f) / pixelToWorld;
        float right = (maxX - width * 0.5f) / pixelToWorld;
        float bottom = (minY - height * 0.5f) / pixelToWorld;
        float top = (maxY - height * 0.5f) / pixelToWorld;
        
        // Create a tight-fitting polygon (can be simplified to 4 points for performance)
        // For better accuracy, we could trace the outline, but 4 points is good for performance
        Vector2[] points2 = new Vector2[4];
        points2[0] = new Vector2(left, bottom);   // Bottom-left
        points2[1] = new Vector2(right, bottom);  // Bottom-right
        points2[2] = new Vector2(right, top);      // Top-right
        points2[3] = new Vector2(left, top);       // Top-left
        
        polygonCol.pathCount = 1;
        polygonCol.SetPath(0, points2);
    }
    
    void LateUpdate()
    {
        // CRITICAL: Ensure polygon collider matches sprite shape after sprite is loaded
        // This runs after all updates to ensure sprite is set
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        PolygonCollider2D polygonCol = GetComponent<PolygonCollider2D>();
        
        if (sr != null && sr.sprite != null && polygonCol != null)
        {
            // Only regenerate polygon collider if sprite changed (optimization)
            if (sr.sprite != lastSprite)
            {
                SetupPolygonColliderFromSprite(polygonCol, sr.sprite);
                lastSprite = sr.sprite;
            }
        }
    }
}

