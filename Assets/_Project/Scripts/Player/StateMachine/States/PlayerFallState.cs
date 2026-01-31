using UnityEngine;

public class PlayerFallState : IPlayerState
{
    private readonly PlayerController player;

    public PlayerFallState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
    }

    public void HandleInput()
    {
        if (player.IsTouchingLadder && Mathf.Abs(player.VerticalInput) > 0.1f)
        {
            player.ChangeState(player.OnLadderState);
            return;
        }

        if (player.ConsumeInteractInput())
        {
            player.ChangeState(player.InteractState);
        }
    }

    public void LogicUpdate()
    {
        player.Move(player.HorizontalInput);

        // 增加垂直速度阈值判定（例如 -0.1f），防止角色刚开始跳跃或离开平台时产生错误的瞬间着陆
        if (player.IsGrounded && player.VerticalVelocity <= 0.01f)
        {
            if (Mathf.Abs(player.HorizontalInput) > 0.01f)
            {
                player.ChangeState(player.RunState);
            }
            else
            {
                player.ChangeState(player.IdleState);
            }
        }
    }

    public void Exit()
    {
    }

    public PlayerStates GetState()
    {
        return PlayerStates.Fall;
    }
}