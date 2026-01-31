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
        Vector2 direction = (boss.PlayerTransform.position - boss.transform.position).normalized;
        boss.Rb.velocity = direction * boss.settings.moveSpeed;
    }
}