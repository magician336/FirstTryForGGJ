using System.Collections.Generic;

public class FishFormStateFactory : PlayerFormStateFactory
{
    public override PlayerFormType FormType => PlayerFormType.Fish;

    public override PlayerFormStateBundle CreateStateBundle(PlayerController controller)
    {
        var map = BuildDefaultStateMap(controller);
        return new PlayerFormStateBundle(map[PlayerStates.Idle], map);
    }

    public override void ApplyFormSettings(PlayerController controller)
    {
        base.ApplyFormSettings(controller);
    }
}
