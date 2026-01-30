using System.Collections.Generic;

public class SuperJumpFormStateFactory : PlayerFormStateFactory
{
    public override PlayerFormType FormType => PlayerFormType.SuperJump;

    public override PlayerFormStateBundle CreateStateBundle(PlayerController controller)
    {
        var superJumpState = new PlayerSuperJumpState(controller);

        // SuperJump 形态的状态集：Idle, Run, Interact, SuperJump

        var map = new Dictionary<PlayerStates, IPlayerState>
        {
            { PlayerStates.Idle, new PlayerIdleState(controller) },
            { PlayerStates.Run, new PlayerRunState(controller) },
            { PlayerStates.Interact, new PlayerInteractState(controller) },
            { PlayerStates.SuperJump, superJumpState },
            { PlayerStates.Fall, new PlayerFallState(controller)}
        };

        return new PlayerFormStateBundle(map[PlayerStates.Idle], map);
    }

    public override void ApplyFormSettings(PlayerController controller)
    {
        base.ApplyFormSettings(controller);
    }
}
