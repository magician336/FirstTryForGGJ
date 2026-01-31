using UnityEngine;

public class PlayerOnLadderState : IPlayerState
{
    private readonly PlayerController player;

    public PlayerOnLadderState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.SetGravityScale(0f);
        // Maybe snap to ladder x center? For now, we just stop horizontal momentum in MoveVertical implicitly
    }

    public void HandleInput()
    {
        // Jump off ladder
        if (player.ConsumeJumpInput())
        {
            if (player.LadderSettings != null)
            {
                player.SetGravityScale(1f); // Restore gravity
                player.JumpOffLadder();
                player.ChangeState(player.FallState);
                return;
            }
        }
    }

    public void LogicUpdate()
    {
        // Apply vertical movement on the ladder
        if (player.VerticalInput != 0)
        {
            player.MoveVertical(player.VerticalInput);
        }
        else
        {
            // Stop vertical movement if no input (MoveVertical logic handles velocity set)
            player.MoveVertical(0f);
        }

        // Also stop horizontal movement explicitly if accessed via player.Move(0)
        // Check if existing Move method supports 0 input to stop
        // player.Move(0f); // Assuming this method exists on PlayerController that delegates to MovementController

        // If we are no longer touching the ladder (e.g. climbed past top), exit to idle/fall
        if (!player.IsTouchingLadder)
        {
            // If we are grounded, Idle/Run. If in air, Fall.
            if (player.IsGrounded)
            {
                player.ChangeState(player.IdleState);
            }
            else
            {
                player.ChangeState(player.FallState);
            }
        }
    }

    public void Exit()
    {
        player.SetGravityScale(1f);
    }

    public PlayerStates GetState()
    {
        return PlayerStates.OnLadder;
    }
}
