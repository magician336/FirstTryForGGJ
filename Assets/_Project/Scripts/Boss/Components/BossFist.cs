using UnityEngine;

/// <summary>
/// Boss 的拳套组件，具有独立的圆弧移动逻辑和弹跳功能。
/// </summary>
public class BossFist : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private Transform pivot;

    private BossController boss;
    private float currentAngle;
    private Vector3 startOffset;

    public void Initialize(BossController controller)
    {
        boss = controller;
        if (pivot == null) pivot = boss.transform;
        startOffset = transform.localPosition;
    }

    private void Update()
    {
        if (boss == null || boss.settings == null) return;

        HandleArcMovement();
    }

    private void HandleArcMovement()
    {
        // 简单的简谐运动实现圆弧摆动
        // 使用 Sin 函数让拳套在一定角度范围内摆动
        currentAngle = Mathf.Sin(Time.time * boss.settings.fistArcSpeed);

        float radius = boss.settings.fistArcRadius;

        // 计算相对坐标 (在 X-Y 平面上的圆弧)
        float x = Mathf.Cos(currentAngle) * radius;
        float y = Mathf.Sin(currentAngle) * radius;

        // 更新位置（相对于 Pivot）
        transform.position = pivot.position + new Vector3(x, y, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyBounce(other);
        }
    }

    private void ApplyBounce(Collider2D playerCollider)
    {
        Rigidbody2D playerRb = playerCollider.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            // 计算弹跳方向：从拳套中心指向玩家
            Vector2 bounceDir = (playerCollider.transform.position - transform.position).normalized;

            // 如果玩家在拳套上方，稍微向上偏移增加弹跳感
            if (bounceDir.y < 0.2f) bounceDir.y += 0.5f;
            bounceDir.Normalize();

            // 应用弹跳力
            playerRb.velocity = Vector2.zero; // 先清空速度，让弹跳效果更明显
            playerRb.AddForce(bounceDir * boss.settings.fistBounceForce, ForceMode2D.Impulse);

            Debug.Log($"[BossFist] 击中玩家，施加弹跳力: {bounceDir * boss.settings.fistBounceForce}");
        }
    }
}
