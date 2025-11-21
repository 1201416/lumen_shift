using UnityEngine;

/// <summary>
/// Basic player controller - placeholder for player movement and interaction.
/// Lightning bolts will check for this component when collecting.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    
    private Rigidbody2D rb;
    private bool isGrounded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
    }

    void Update()
    {
        // Basic horizontal movement
        float horizontal = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);
        
        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if grounded
        if (collision.gameObject.CompareTag("Ground") || 
            collision.gameObject.GetComponent<FloorBlock>() != null ||
            collision.gameObject.GetComponent<BoxBlock>() != null)
        {
            isGrounded = true;
        }
    }
}

