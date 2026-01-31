using System.Collections.Generic;
using UnityEngine;

public class FishFormStateFactory : PlayerFormStateFactory
{
    public override PlayerFormType FormType => PlayerFormType.Fish;

    public override PlayerFormStateBundle CreateStateBundle(PlayerController controller)
    {
        var map = BuildDefaultStateMap(controller);

        // 注入 Fish 形态特有的游泳状态
        map[PlayerStates.SwimIdle] = new PlayerSwimIdleState(controller);
        map[PlayerStates.SwimRun] = new PlayerSwimRunState(controller);

        Debug.Log("Fish 形态：游泳状态已注册。");

        return new PlayerFormStateBundle(map[PlayerStates.Idle], map);
    }

    public override void ApplyFormSettings(PlayerController controller)
    {
        base.ApplyFormSettings(controller);
    }
}
