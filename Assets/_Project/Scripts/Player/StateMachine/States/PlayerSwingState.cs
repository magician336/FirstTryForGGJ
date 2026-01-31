using UnityEngine;

public class PlayerSwingState : IPlayerState
{
    private PlayerController player;
    private SwingController swing;

    public PlayerSwingState(PlayerController player)
    {
        this.player = player;
        this.swing = player.GetComponent<SwingController>();
    }

    public void Enter()
    {
        // No-op: swing starts via controller logic for now
        // But we should ensure we are consistent if Enter was called manually
    }

    public void HandleInput()
    {
        // Swing cancellation handled in PlayerController.OnSwingButtonDown

        // Jump to cancel swing
        if (player.ConsumeJumpInput())
        {
            // Give a little boost?
            player.ExecuteJump(0.5f);
            player.ChangeState(player.JumpState);
        }
    }

    public void LogicUpdate()
    {
        // Cancel if grounded and not effectively moving up
        if (player.IsGrounded && player.VerticalVelocity <= 0.1f)
        {
            player.ChangeState(player.IdleState);
        }
    }

    public void PhysicsUpdate()
    {
        if (swing != null)
        {
            swing.UpdateSwing(player.HorizontalInput, player.VerticalInput);
        }
    }

    public void Exit()
    {
        if (swing != null)
        {
            swing.StopSwing();
        }
    }

    public PlayerStates GetState()
    {
        return PlayerStates.Swing;
    }
}
