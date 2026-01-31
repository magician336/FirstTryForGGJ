using UnityEngine;

public class PlayerIdleState : IPlayerState
{
    private readonly PlayerController player;

    public PlayerIdleState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.Move(0f);
    }

    public void HandleInput()
    {
        // Fish 形态在水中时切换到游泳状态
        if (player.CanSwim)
        {
            player.ChangeState(player.SwimIdleState);
            return;
        }

        if (player.IsTouchingLadder && Mathf.Abs(player.VerticalInput) > 0.1f)
        {
            player.ChangeState(player.OnLadderState);
            return;
        }

        if (player.IsGrounded && player.ConsumeJumpInput())
        {
            player.ChangeState(player.JumpState);
            return;
        }

        if (player.ConsumeInteractInput())
        {
            player.ChangeState(player.InteractState);
            return;
        }

        if (Mathf.Abs(player.HorizontalInput) > 0.01f)
        {
            player.ChangeState(player.RunState);
        }
    }

    public void LogicUpdate()
    {
        player.Move(0f);

        if (!player.IsGrounded && player.VerticalVelocity < 0f)
        {
            player.ChangeState(player.FallState);
        }
    }

    public void Exit()
    {
    }

    public PlayerStates GetState()
    {
        return PlayerStates.Idle;
    }
}