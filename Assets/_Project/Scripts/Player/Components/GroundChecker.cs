using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [Header("地面检测设置")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Vector3 groundOffset = new Vector3(0f, -0.5f, 0f);
    public LayerMask groundLayer;
    [SerializeField] private bool ignoreTriggers = true;

    private bool isGrounded;

    public bool IsGrounded => isGrounded;

    void FixedUpdate()
    {

        Debug.Log("GroundCheck");
        CheckGround();
    }

    private void CheckGround()
    {
        var checkPosition = transform.position + groundOffset;
        var query = ignoreTriggers ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide;
        isGrounded = Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        var checkPosition = Application.isPlaying
            ? transform.position + groundOffset
            : transform.position + groundOffset;
        Gizmos.DrawWireSphere(checkPosition, groundCheckRadius);
    }
}