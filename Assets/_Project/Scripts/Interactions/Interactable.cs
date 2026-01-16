using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Header("当交互发生时执行的操作")]
    public UnityEvent onInteract;

    // 这个方法可以由玩家的脚本触发
    public void TriggerInteract()
    {
        Debug.Log(gameObject.name + " 被触发了！");
        onInteract.Invoke(); // 执行在 Inspector 面板里拖进去的所有函数
    }
}