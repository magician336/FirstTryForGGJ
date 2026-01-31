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
        // 注意：由于皮肤系统重构，现在需要通过 GetFormSettings 获取当前激活的皮肤配置
        // 这里我们默认取 index 0，如果要支持 Factory 级别的动态皮肤，需要从 Controller 获取 skinIndex
        // 但目前 PlayerController.ApplyFormSettings 已经负责了基础应用，这里主要是为了给 SwingController 传参
        var swingSettings = controller.Settings.GetFormSettings(PlayerFormType.Spider) as SwingFormSettings;
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
