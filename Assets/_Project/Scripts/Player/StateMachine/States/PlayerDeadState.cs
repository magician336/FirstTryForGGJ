using UnityEngine;

public class PlayerDeadState : IPlayerState
{
    private readonly PlayerController player;

    public PlayerDeadState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
        Debug.Log("死了");
        player.SetMovementInput(0f);
        player.Move(0f);
        player.SetGravityScale(1f); // Ensure they fall if in air
        // TODO: Play Death Animation via PresentationBinder
    }

    public void HandleInput()
    {
        // No input allowed
    }

    public void LogicUpdate()
    {
        // Physics apply (falling) but no control
    }

    public void Exit()
    {
        // Cleanup if needed
    }

    public PlayerStates GetState()
    {
        // We need to add Dead to PlayerStates enum first, assuming it's done or I will do it.
        return PlayerStates.Dead;
    }
}
