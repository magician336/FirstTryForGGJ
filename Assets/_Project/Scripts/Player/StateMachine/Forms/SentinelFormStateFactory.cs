using System.Collections.Generic;

public class SentinelFormStateFactory : PlayerFormStateFactory
{
    public override PlayerFormType FormType => PlayerFormType.Sentinel;

    public override PlayerFormStateBundle CreateStateBundle(PlayerController controller)
    {
        var map = BuildDefaultStateMap(controller);
        return new PlayerFormStateBundle(map[PlayerStates.Idle], map);
    }

    public override void ApplyFormSettings(PlayerController controller)
    {
        base.ApplyFormSettings(controller);
        controller?.ApplyMovementProfile(0.85f, 1.15f);
        controller?.ApplyGravityMultiplier(1.2f);
    }
}
