using UnityEngine;

public class DoorInteractable : Interactable
{
    [Header("门设置")]
    public Animator doorAnimator;
    public string openAnimationTrigger = "Open";
    public string closeAnimationTrigger = "Close";

    public override void TriggerInteract()
    {
        if (doorAnimator != null)
        {
            // 假设门是开关状态，使用一个简单的布尔值来控制开关
            bool isOpen = doorAnimator.GetCurrentAnimatorStateInfo(0).IsName(openAnimationTrigger);
            if (isOpen)
            {
                doorAnimator.SetTrigger(closeAnimationTrigger);
            }
            else
            {
                doorAnimator.SetTrigger(openAnimationTrigger);
            }
        }
    }
}