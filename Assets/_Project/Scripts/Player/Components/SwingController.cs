using UnityEngine;

public class SwingController : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private float swingForce = 15f;

    [Header("组件引用")]
    [SerializeField] private DistanceJoint2D joint;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Rigidbody2D rb;

    private Vector2 anchorPoint;
    public bool IsSwinging { get; private set; }

    void Awake()
    {
        if (joint == null) joint = GetComponent<DistanceJoint2D>();
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (joint != null) joint.enabled = false;
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 2;
        }
    }

    public bool TryStartSwing(float facingDirection)
    {
        // 1. 计算 45 度发射方向
        // 如果 facingDirection > 0 (右): (1, 1) -> 45度
        // 如果 facingDirection < 0 (左): (-1, 1) -> 135度
        float sign = facingDirection >= 0 ? 1f : -1f;

        // 如果输入为0（静止），默认向右或者保留最后朝向需要上层传入，这里暂时默认向右
        if (Mathf.Abs(facingDirection) < 0.01f) sign = 1f;

        Vector2 shootDirection = new Vector2(sign, 1f).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, shootDirection, maxDistance, grappleLayer);

        if (hit.collider != null)
        {
            anchorPoint = hit.point;
            SetupJoint(hit.point);
            IsSwinging = true;
            return true;
        }
        return false;
    }

    private void SetupJoint(Vector2 point)
    {
        if (joint == null || lineRenderer == null) return;

        joint.enabled = true;
        joint.connectedAnchor = point;
        joint.distance = Vector2.Distance(transform.position, point);

        lineRenderer.enabled = true;
    }

    public void UpdateSwing(float moveInput, float verticalInput)
    {
        if (!IsSwinging || joint == null || rb == null || lineRenderer == null) return;

        // 2. W/S 控制长度 - W(>0)缩短, S(<0)伸长
        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            joint.distance -= verticalInput * climbSpeed * Time.deltaTime;
        }

        // 3. A/D 控制摆动 (施加水平力)
        rb.AddForce(new Vector2(moveInput * swingForce, 0));

        // 4. 更新视觉线段
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, anchorPoint);
    }

    public void StopSwing()
    {
        IsSwinging = false;
        if (joint != null) joint.enabled = false;
        if (lineRenderer != null) lineRenderer.enabled = false;
    }

    public void ApplySettings(SwingFormSettings settings)
    {
        if (settings == null) return;

        grappleLayer = settings.grappleLayer;
        maxDistance = settings.maxWebDistance;
        climbSpeed = settings.climbSpeed;
        swingForce = settings.swingForce;
    }
}
