using UnityEngine;

public class PlayerSuperJumpState : IPlayerState
{
    private readonly PlayerController player;
    private float appliedMultiplier = 1f;

    public PlayerSuperJumpState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
        if (player.TryConsumeSuperJumpCharge(out var multiplier))
        {
            appliedMultiplier = multiplier;
        }
        else
        {
            appliedMultiplier = 1f;
        }

        player.ExecuteJump(appliedMultiplier);
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
        return PlayerStates.SuperJump;
    }
}