using UnityEngine;

/// <summary>
/// 统一的场景跳转触发器。
/// 支持“走进区域自动跳转”或“在区域内按交互键跳转”。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("跳转配置")]
    [Tooltip("目标场景的字符串名称（必须已在 Build Settings 中添加）")]
    [SerializeField] private string targetSceneName;

    [Tooltip("是否为自动触发（走进区域即跳转）。如果不勾选，则需要走进区域并按下交互键。")]
    [SerializeField] private bool isAutomatic = true;

    [Header("出生点配置 (可选)")]
    [Tooltip("跳转到新场景后，玩家应该在哪个 Tag 处生成。如果不填，则使用 GameManager 默认配置。")]
    [SerializeField] private string targetSpawnTag = "";

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
            if (isAutomatic)
            {
                DoTransition();
            }
            else
            {
                _isPlayerInZone = true;
                // 这里可以扩展显示“按 F [交互]”的提示 UI
            }
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
        // 如果不是自动触发，且玩家在区域内，检测交互键
        if (!isAutomatic && _isPlayerInZone)
        {
            // 这里我们手动检测按键，默认遵循 InteractionController 的 F 键
            // 以后可以重构为从统一输入设置中获取
            if (Input.GetKeyDown(KeyCode.F))
            {
                DoTransition();
            }
        }
    }

    private void DoTransition()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning($"[SceneTransitionTrigger] 目标场景为空: {gameObject.name}");
            return;
        }

        // 如果设置了特定的出生点 Tag，将其告知 GameManager
        if (!string.IsNullOrEmpty(targetSpawnTag) && GameManager.Instance != null)
        {
            GameManager.Instance.SetNextSpawnTag(targetSpawnTag);
        }

        if (SceneManager.Instance != null)
        {
            SceneManager.Instance.LoadScene(targetSceneName);
        }
        else
        {
            // 如果场景中没有 SceneManager，回退到原生加载
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        }
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
    }
}
