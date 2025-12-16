using UnityEngine;

/// <summary>
/// MonsterAI - handles monster movement behavior (patrol and chase)
/// </summary>
public class MonsterAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public bool canPatrol = false;
    public float patrolStartX = 0f;
    public float patrolEndX = 0f;
    public float patrolSpeed = 1.5f;
    
    [Header("Chase Settings")]
    public bool canChasePlayer = false;
    public float chaseSpeed = 2.5f;
    public float chaseRange = 5f;
    
    private Rigidbody2D rb;
    private bool movingRight = true;
    private Transform playerTransform;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Find player
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }
    
    void FixedUpdate()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Check if should chase player
        if (canChasePlayer && distanceToPlayer <= chaseRange)
        {
            ChasePlayer();
        }
        // Otherwise patrol if enabled
        else if (canPatrol)
        {
            Patrol();
        }
        else
        {
            // Stop moving
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }
    }
    
    void Patrol()
    {
        float currentX = transform.position.x;
        
        // Check if reached patrol boundaries
        if (movingRight && currentX >= patrolEndX)
        {
            movingRight = false;
        }
        else if (!movingRight && currentX <= patrolStartX)
        {
            movingRight = true;
        }
        
        // Move in patrol direction
        float direction = movingRight ? 1f : -1f;
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction * patrolSpeed, rb.linearVelocity.y);
        }
    }
    
    void ChasePlayer()
    {
        if (playerTransform == null) return;
        
        float direction = Mathf.Sign(playerTransform.position.x - transform.position.x);
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
        }
    }
}

