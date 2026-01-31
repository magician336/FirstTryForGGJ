using UnityEngine;

public class BossChaseState : IBossState
{
    private BossController boss;
    public BossChaseState(BossController controller) => boss = controller;

    public void Enter() { }
    public void Exit() => boss.Rb.velocity = Vector2.zero;

    public void LogicUpdate() { }

    public void PhysicsUpdate()
    {
        Transform target = boss.PlayerTransform;

        if (target == null)
        {
            // 只有每隔一秒打印一次，防止刷屏日志
            if (Time.frameCount % 60 == 0) Debug.Log("[BossChaseState] 正在等待玩家出现...");
            boss.Rb.velocity = Vector2.zero;
            return;
        }

        Vector2 direction = (target.position - boss.transform.position).normalized;
        float dist = Vector2.Distance(boss.transform.position, target.position);

        // 如果距离大于追逐范围，停止移动
        if (dist > boss.settings.chaseRange)
        {
            if (Time.frameCount % 60 == 0) Debug.Log($"[BossChaseState] 玩家太远了 ({dist:F2} > {boss.settings.chaseRange})，原地待命。");
            boss.Rb.velocity = Vector2.zero;
            return;
        }

        // 应用速度
        float moveSpeed = boss.settings != null ? boss.settings.moveSpeed : 3f;
        boss.Rb.velocity = direction * moveSpeed;

        if (Time.frameCount % 100 == 0)
            Debug.Log($"[BossChaseState] 正在追逐！方向: {direction}, 速度: {boss.Rb.velocity.magnitude}, 距离玩家: {dist:F2}");
    }
}
