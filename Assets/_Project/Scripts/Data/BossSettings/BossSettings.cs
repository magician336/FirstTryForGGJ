using UnityEngine;

[CreateAssetMenu(fileName = "NewBossSettings", menuName = "Boss/Settings")]
public class BossSettings : ScriptableObject
{
    public float moveSpeed = 3f;
    public float attackRange = 2f;
    public float chaseRange = 10f;
    public int touchDamage = 1;

    [Header("拳套配置")]
    public float fistArcRadius = 2f;
    public float fistArcSpeed = 2f;
    public float fistBounceForce = 5f;
}