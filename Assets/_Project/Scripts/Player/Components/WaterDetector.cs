using System;
using UnityEngine;

public class WaterDetector : MonoBehaviour
{
    [Header("检测配置")]
    [SerializeField] private float checkRadius = 0.5f;
    [SerializeField] private Vector3 checkOffset = Vector3.zero;

    public bool IsInWater { get; private set; }

    // 使用事件解耦，方便 PlayerController 订阅，而不需要每帧去轮询 previousState
    public event Action OnWaterEnter;
    public event Action OnWaterExit;

    public void Detect(LayerMask waterLayer)
    {
        bool wasInWater = IsInWater;

        // 执行物理检测
        IsInWater = Physics2D.OverlapCircle(transform.position + checkOffset, checkRadius, waterLayer);

        // 状态变化处理
        if (IsInWater && !wasInWater)
        {
            OnWaterEnter?.Invoke();
        }
        else if (!IsInWater && wasInWater)
        {
            OnWaterExit?.Invoke();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 蓝色可视化圈，方便调试
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position + checkOffset, checkRadius);
    }
}