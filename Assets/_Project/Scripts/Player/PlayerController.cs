using UnityEngine;

// 强制要求物体上有 Rigidbody2D，防止你忘记加
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;

    [Header("交互设置")]
    public KeyCode interactKey = KeyCode.E; // 按 E 交互
    public float interactRange = 1.5f;      // 交互范围
    public LayerMask interactLayer;         // 只有这一层的物体能被交互

    private Rigidbody2D rb;
    private Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. 获取输入 (WASD 或 方向键)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 2. 检测交互输入
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }

        // 3. 处理角色朝向翻转 (视觉优化)
        if (movement.x != 0)
        {
            // 如果向左走(x<0)，Scale设为-1；向右走(x>0)，Scale设为1
            transform.localScale = new Vector3(Mathf.Sign(movement.x), 1, 1);
        }
    }

    void FixedUpdate()
    {
        // 4. 物理移动 (推荐在 FixedUpdate 中移动刚体)
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    // 尝试触发附近的 Interactable
    void TryInteract()
    {
        // 在角色周围画一个圆圈，看看有没有碰到 "Interactable" 层的东西
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRange, interactLayer);

        if (hit != null)
        {
            // 如果碰到了，尝试获取上面的 Interactable 脚本
            Interactable target = hit.GetComponent<Interactable>();
            if (target != null)
            {
                target.TriggerInteract();
            }
        }
    }

    // 在编辑器里画出交互范围（方便调试）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}