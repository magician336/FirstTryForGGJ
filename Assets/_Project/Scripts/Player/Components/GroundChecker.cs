using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [Header("地面检测设置")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;
    [SerializeField] private bool ignoreTriggers = true;

    private bool isGrounded;
    private ContactFilter2D _filter;
    private readonly Collider2D[] _results = new Collider2D[1];

    public bool IsGrounded => isGrounded;

    void Awake()
    {
        _filter = new ContactFilter2D();
        _filter.SetLayerMask(groundLayer);
        _filter.useLayerMask = true;
        _filter.useTriggers = !ignoreTriggers;
    }

    void FixedUpdate()
    {
        CheckGround();
    }

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        // 使用 ContactFilter2D 过滤掉触发器，防止角色碰到透明触发区域也认为接地
        int hitCount = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, _filter, _results);
        isGrounded = hitCount > 0;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}