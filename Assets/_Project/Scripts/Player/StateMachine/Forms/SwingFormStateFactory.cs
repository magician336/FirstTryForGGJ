using System.Collections.Generic;
using UnityEngine;

public class SwingFormStateFactory : PlayerFormStateFactory
{
    // 对应 PlayerFormType.Spider (荡蛛丝形态)
    public override PlayerFormType FormType => PlayerFormType.Spider;

    public override PlayerFormStateBundle CreateStateBundle(PlayerController controller)
    {
        // 1. 获取默认状态集合 (Idle, Run, Jump, Fall, Interact)
        var map = BuildDefaultStateMap(controller);

        // 2. 注入 Spider 形态特有的 Swing 状态
        map[PlayerStates.Swing] = new PlayerSwingState(controller);

        Debug.Log("Swing (Spider) 形态：状态映射已构建完成。");

        // 3. 返回以 Idle 为起始状态的包
        return new PlayerFormStateBundle(map[PlayerStates.Idle], map);
    }

    public override void ApplyFormSettings(PlayerController controller)
    {
        base.ApplyFormSettings(controller);

        if (controller.Settings == null)
        {
            Debug.LogWarning("SwingFormStateFactory: PlayerSettings 缺失。");
            return;
        }

        // 获取并应用具体的 SwingSettings
        var swingSettings = controller.Settings.swingForm;
        var swingController = controller.GetComponent<SwingController>();

        if (swingController != null && swingSettings != null)
        {
            swingController.ApplySettings(swingSettings);
        }
        else
        {
            Debug.LogWarning($"SwingFormStateFactory: 缺少 {(swingController == null ? "SwingController 组件" : "SwingFormSettings 配置")}。");
        }
    }
}
