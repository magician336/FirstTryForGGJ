using System.Collections.Generic;

public class CryFormStateFactory : PlayerFormStateFactory
{
    public override PlayerFormType FormType => PlayerFormType.Cry;

    public override PlayerFormStateBundle CreateStateBundle(PlayerController controller)
    {
        var flightState = new PlayerFlightState(controller);

        // Cry 形态的状态集：Idle, Run, Interact, Flight
        // 同时为了兼容状态切换逻辑，将 Jump 和 Fall 都指向 Flight
        var map = new Dictionary<PlayerStates, IPlayerState>
        {
            { PlayerStates.Idle, new PlayerIdleState(controller) },
            { PlayerStates.Run, new PlayerRunState(controller) },
            { PlayerStates.Interact, new PlayerInteractState(controller) },
            { PlayerStates.Flight, flightState },
            { PlayerStates.Jump, flightState },
            { PlayerStates.Fall, flightState }
        };

        return new PlayerFormStateBundle(map[PlayerStates.Idle], map);
    }

    public override void ApplyFormSettings(PlayerController controller)
    {
        base.ApplyFormSettings(controller);
        // 飞行形态：提升水平机动，降低重力以便悬空/滑翔
        controller?.ApplyMovementProfile(1.15f, 1.0f);
        controller?.ApplyGravityMultiplier(0.4f);
    }
}
