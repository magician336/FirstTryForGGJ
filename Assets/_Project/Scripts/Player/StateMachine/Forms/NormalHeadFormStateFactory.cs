using System;
using System.Collections.Generic;
using UnityEngine;
public class NormalHeadFormStateFactory : PlayerFormStateFactory
{
    public override PlayerFormType FormType => PlayerFormType.NormalHead;

    public override PlayerFormStateBundle CreateStateBundle(PlayerController controller)
    {
        var map = BuildDefaultStateMap(controller);
        return new PlayerFormStateBundle(map[PlayerStates.Idle], map);
    }

    public override void ApplyFormSettings(PlayerController controller)
    {
        base.ApplyFormSettings(controller);
        Debug.Log("切换NormalHead形态");
    }
}
