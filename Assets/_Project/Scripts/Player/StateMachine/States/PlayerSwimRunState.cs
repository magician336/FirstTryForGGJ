using UnityEngine;

/// <summary>
/// 游泳移动状态 - 玩家在水中移动时的状态
/// 仅在 Fish 形态且处于水中时有效
/// </summary>
public class PlayerSwimRunState : IPlayerState
{
    private readonly PlayerController player;

    public PlayerSwimRunState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
        // 在水中减少重力影响
        player.SetGravityScale(0.3f);
    }

    public void HandleInput()
    {
        // 离开水域，切换回普通 Run 或 Idle
        if (!player.IsInWater)
        {
            if (Mathf.Abs(player.HorizontalInput) > 0.01f)
            {
                player.ChangeState(player.RunState);
            }
            else
            {
                player.ChangeState(player.IdleState);
            }
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

        // 无输入则切换到游泳静止状态
        if (Mathf.Abs(player.HorizontalInput) < 0.01f && Mathf.Abs(player.VerticalInput) < 0.01f)
        {
            player.ChangeState(player.SwimIdleState);
        }
    }

    public void LogicUpdate()
    {
        // 水中可以自由移动（水平 + 垂直）
        player.Swim(player.HorizontalInput, player.VerticalInput);
    }

    public void Exit()
    {
        // 恢复正常重力
        player.SetGravityScale(1f);
    }

    public PlayerStates GetState()
    {
        return PlayerStates.SwimRun;
    }
}
