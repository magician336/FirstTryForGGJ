using UnityEngine;

/// <summary>
/// 游泳静止状态 - 玩家在水中不移动时的状态
/// 仅在 Fish 形态且处于水中时有效
/// </summary>
public class PlayerSwimIdleState : IPlayerState
{
    private readonly PlayerController player;

    public PlayerSwimIdleState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.Move(0f);
        // 在水中减少重力影响
        player.SetGravityScale(0.3f);
    }

    public void HandleInput()
    {
        // 离开水域，切换回普通 Idle
        if (!player.IsInWater)
        {
            player.ChangeState(player.IdleState);
            return;
        }

        // 跳跃输入 - 在水中可以向上游
        if (player.ConsumeJumpInput())
        {
            player.SwimUp();
            return;
        }

        // 交互输入
        if (player.ConsumeInteractInput())
        {
            player.ChangeState(player.InteractState);
            return;
        }

        // 有水平输入则切换到游泳移动状态
        if (Mathf.Abs(player.HorizontalInput) > 0.01f)
        {
            player.ChangeState(player.SwimRunState);
            return;
        }

        // 有垂直输入也切换到游泳移动状态
        if (Mathf.Abs(player.VerticalInput) > 0.01f)
        {
            player.ChangeState(player.SwimRunState);
        }
    }

    public void LogicUpdate()
    {
        player.Move(0f);
        // 在水中缓慢下沉或保持位置
        player.ApplySwimDrag();
    }

    public void Exit()
    {
        // 恢复正常重力
        player.SetGravityScale(1f);
    }

    public PlayerStates GetState()
    {
        return PlayerStates.SwimIdle;
    }
}
