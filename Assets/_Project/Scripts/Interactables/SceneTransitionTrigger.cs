using UnityEngine;

/// <summary>
/// 统一的场景跳转触发器。
/// 支持“在同一大场景中切换摄像机与玩家位置”的跳转逻辑。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("大场景跳转配置")]
    [Tooltip("摄像机要移动到的目标点")]
    [SerializeField] private Transform cameraTargetMarker;

    [Tooltip("玩家要传送到的目标点")]
    [SerializeField] private Transform playerSpawnMarker;

    [SerializeField] private bool isAutomatic = true;
    [SerializeField] private bool instantCameraMove = false;

    private bool _isPlayerInZone = false;

    private void Awake()
    {
        // 确保 Collider 被设为 Trigger
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (isAutomatic) DoTransition();
            else _isPlayerInZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInZone = false;
        }
    }

    private void Update()
    {
        if (!isAutomatic && _isPlayerInZone && Input.GetKeyDown(KeyCode.F))
        {
            DoTransition();
        }
    }

    private void DoTransition()
    {
        // 1. 移动摄像机
        if (cameraTargetMarker != null && CameraManager.Instance != null)
        {
            CameraManager.Instance.TransitionTo(cameraTargetMarker.position, instantCameraMove);
        }

        // 2. 传送玩家到新区域
        if (playerSpawnMarker != null && GameManager.Instance != null)
        {
            var player = GameManager.Instance.Player;
            if (player != null)
            {
                // 注意：如果传送后需要立即改变重生点，可以顺便调用 SetRespawnPoint
                player.transform.position = playerSpawnMarker.position;
                GameManager.Instance.SetRespawnPoint(playerSpawnMarker.position);
            }
        }

        Debug.Log($"[Transition] 已切换至新区域: {gameObject.name}");
    }

    private void OnDrawGizmos()
    {
        // 在编辑器中绘制一个透明的蓝色方块代表跳转区域
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        var col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.DrawCube((Vector2)transform.position + col.offset, col.size);
        }
        else
        {
            Gizmos.DrawSphere(transform.position, 0.5f);
        }

        // 绘制连线到目标点，方便视觉化跳转关系
        if (cameraTargetMarker != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, cameraTargetMarker.position);
            Gizmos.DrawWireSphere(cameraTargetMarker.position, 0.5f);
        }

        if (playerSpawnMarker != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, playerSpawnMarker.position);
            Gizmos.DrawWireSphere(playerSpawnMarker.position, 0.4f);
        }
    }
}
