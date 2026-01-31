using UnityEngine;

public class PlayerRunState : IPlayerState
{
    private readonly PlayerController player;

    public PlayerRunState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
    }

    public void HandleInput()
    {
        // Fish 形态在水中时切换到游泳状态
        if (player.CanSwim)
        {
            player.ChangeState(player.SwimRunState);
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

        if (Mathf.Abs(player.HorizontalInput) < 0.01f)
        {
            player.ChangeState(player.IdleState);
        }
    }

    public void LogicUpdate()
    {
        player.Move(player.HorizontalInput);

        if (!player.IsGrounded && player.VerticalVelocity <= 0f)
        {
            player.ChangeState(player.FallState);
        }
    }

    public void Exit()
    {
    }

    public PlayerStates GetState()
    {
        return PlayerStates.Run;
    }
}