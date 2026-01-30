using UnityEngine;

[CreateAssetMenu(fileName = "SuperJumpFormSettings", menuName = "Player/Settings/SuperJumpFormSettings")]
public class SuperJumpFormSettings : NormalHeadFormSettings
{
    [Header("Super Jump Charge")]
    public float minChargeMultiplier = 1f;
    public float maxChargeMultiplier = 2.5f;
    public float maxChargeTime = 1.5f;
}
