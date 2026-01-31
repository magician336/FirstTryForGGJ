using UnityEngine;

public class InteractionController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputSettings inputSettings;

    public KeyCode InteractKey => inputSettings != null ? inputSettings.InteractKey : KeyCode.F;

    public float interactRange = 1.5f;
    public LayerMask interactLayer;

    public void ApplyInputSettings(InputSettings settings)
    {
        inputSettings = settings;
    }

    public bool TryInteract()
    {
        Debug.Log($"[Interaction] 尝试在范围 {interactRange} 内进行检测，层级 Mask: {interactLayer.value}");
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRange, interactLayer);
        if (hit == null)
        {
            Debug.Log("[Interaction] 未检测到任何碰撞体。");
            return false;
        }

        Debug.Log($"[Interaction] 检测到物体: {hit.name}，正在检查 IInteractable 接口...");
        IInteractable target = hit.GetComponent<IInteractable>();
        if (target == null)
        {
            // 有些时候接口在父物体上
            target = hit.GetComponentInParent<IInteractable>();
        }

        if (target == null)
        {
            Debug.Log($"[Interaction] 物体 {hit.name} 未挂载 IInteractable 接口。");
            return false;
        }

        Debug.Log($"[Interaction] 成功触发交互: {hit.name}");
        target.TriggerInteract();
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}