using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    private Rigidbody2D rb;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Move(float horizontalInput)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb == null) return;

        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    public void Jump(float forceMultiplier = 1f)
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * (jumpForce * Mathf.Max(0.1f, forceMultiplier)), ForceMode2D.Impulse);
            isGrounded = false;
        }
    }

    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }
}