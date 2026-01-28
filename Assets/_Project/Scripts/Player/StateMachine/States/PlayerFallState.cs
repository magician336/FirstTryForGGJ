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
        if (player.ConsumeInteractInput())
        {
            player.ChangeState(player.InteractState);
        }
    }

    public void LogicUpdate()
    {
        player.Move(player.HorizontalInput);

        if (player.IsGrounded)
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
}