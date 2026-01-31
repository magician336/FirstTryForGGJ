using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 秘密区域触发器。
/// 当玩家触碰此触发器时，可以隐藏虚假的墙壁并显示内部的秘密内容。
/// </summary>
public class SecretTrigger : MonoBehaviour
{
    [Header("设置")]
    [Tooltip("是否只触发一次")]
    [SerializeField] private bool triggerOnlyOnce = true;
    private bool _hasTriggered = false;

    [Header("要消失的物体")]
    [Tooltip("通常是遮挡视线的虚假墙壁或覆盖图块")]
    public GameObject[] objectsToHide;

    [Header("要显示的物体")]
    [Tooltip("隐藏房间里的宝箱、特殊收集物或光效")]
    public GameObject[] objectsToShow;

    [Header("高级自定义事件")]
    [Tooltip("触发时执行的额外操作，如播放音效、震屏、统计分数等")]
    public UnityEvent onTriggered;

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
        if (_hasTriggered && triggerOnlyOnce) return;

        if (other.CompareTag("Player"))
        {
            ExecuteTrigger();
        }
    }

    public void ExecuteTrigger()
    {
        _hasTriggered = true;

        // 1. 隐藏物体（墙壁消失）
        foreach (var obj in objectsToHide)
        {
            if (obj != null) obj.SetActive(false);
        }

        // 2. 显示物体（内容出现）
        foreach (var obj in objectsToShow)
        {
            if (obj != null) obj.SetActive(true);
        }

        // 3. 执行额外事件（音效、反馈）
        onTriggered?.Invoke();

        Debug.Log($"[Secret] 隐藏房间已被发现: {gameObject.name}");
    }

    // 可视化调试：在编辑器里画出关联线，方便关卡设计
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 myPos = transform.position;

        if (objectsToHide != null)
        {
            foreach (var obj in objectsToHide)
            {
                if (obj != null) Gizmos.DrawLine(myPos, obj.transform.position);
            }
        }

        Gizmos.color = Color.cyan;
        if (objectsToShow != null)
        {
            foreach (var obj in objectsToShow)
            {
                if (obj != null) Gizmos.DrawLine(myPos, obj.transform.position);
            }
        }
    }
}
