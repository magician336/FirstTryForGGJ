using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("地面检测")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("交互设置")]
    public KeyCode interactKey = KeyCode.E;
    public float interactRange = 1.5f;
    public LayerMask interactLayer;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. 获取水平输入
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. 地面检测（修改部分：检测落地瞬间）
        if (groundCheck != null)
        {
            // A. 先把现在的状态存为“旧状态”
            bool wasGrounded = isGrounded;

            // B. 获取最新的状态
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            // C. 如果“之前没在地面”且“现在在地面”，说明刚刚落地
            if (!wasGrounded && isGrounded)
            {
                rb.velocity = Vector2.zero; // 速度清零
                // Debug.Log("落地！速度已重置"); // 可选：调试用
            }
        }

        // 3. 跳跃输入
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // 4. 交互输入
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }

        // 5. 角色翻转
        if (horizontalInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(horizontalInput), 1, 1);
        }
    }

    void FixedUpdate()
    {
        // 6. 物理移动
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    void TryInteract()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRange, interactLayer);
        if (hit != null)
        {
            Interactable target = hit.GetComponent<Interactable>();
            if (target != null)
            {
                target.TriggerInteract();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);

        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}