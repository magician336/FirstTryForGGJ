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
        Vector2 checkPos = transform.position;
        Debug.Log($"[Interaction] 按下交互键！位置: {checkPos}, 范围: {interactRange}, 目标层级: {LayerMask.LayerToName(Mathf.RoundToInt(Mathf.Log(interactLayer.value, 2)))} ({interactLayer.value})");

        Collider2D hit = Physics2D.OverlapCircle(checkPos, interactRange, interactLayer);
        if (hit == null)
        {
            // 如果没扫到，尝试在大一点的范围内扫一下，告诉用户周围有什么
            Collider2D nearby = Physics2D.OverlapCircle(checkPos, interactRange * 2f);
            string nearbyTag = nearby != null ? nearby.name : "空";
            Debug.LogWarning($"[Interaction] 范围内没有匹配层级的物体。提示：检测到最近的物体是 '{nearbyTag}'，请检查该物体是否在正确的 Layer 上。");
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