using UnityEngine;

public class PlayerFlightState : IPlayerState
{
    private readonly PlayerController player;

    public PlayerFlightState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
        // 使用与 Jump 相同的起跳动作，但由于 Cry 形态降低了重力，将呈现滑翔/飞行效果
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
        // 在飞行中允许水平控制，重力由 Cry 形态设定的倍率影响下提供更长的滞空
        player.Move(player.HorizontalInput);

        // 当上升结束（速度<=0）则进入下落，落地后由 Fall 状态处理返还 Idle/Run
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
        return PlayerStates.Flight;
    }
}