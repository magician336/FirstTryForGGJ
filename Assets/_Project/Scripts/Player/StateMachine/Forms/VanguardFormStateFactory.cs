using System;
using System.Collections.Generic;
using UnityEngine;
public class VanguardFormStateFactory : PlayerFormStateFactory
{
    public override PlayerFormType FormType => PlayerFormType.Vanguard;

    public override PlayerFormStateBundle CreateStateBundle(PlayerController controller)
    {
        var map = BuildDefaultStateMap(controller);
        return new PlayerFormStateBundle(map[PlayerStates.Idle], map);
    }

    public override void ApplyFormSettings(PlayerController controller)
    {
        base.ApplyFormSettings(controller);
        controller?.ApplyMovementProfile(1f, 1f);
        controller?.ApplyGravityMultiplier(1f);
        Debug.Log("切换Van形态");
    }
}
