using UnityEngine;

public class PlayerJumpState : IPlayerState
{
    private readonly PlayerController player;

    public PlayerJumpState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.ExecuteJump();
    }

    public void HandleInput()
    {
        if (player.ConsumeInteractInput())
        {
            player.ChangeState(player.InteractState);
        }
    }

    public void LogicUpdate()
    {
        player.Move(player.HorizontalInput);

        if (player.VerticalVelocity <= 0f)
        {
            player.ChangeState(player.FallState);
        }
    }

    public void Exit()
    {
    }

    public PlayerStates GetState()
    {
        return PlayerStates.Jump;
    }
}