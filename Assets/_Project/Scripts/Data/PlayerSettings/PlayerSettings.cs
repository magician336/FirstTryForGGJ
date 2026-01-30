using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Player/Settings")]
public class PlayerSettings : ScriptableObject
{
    [Header("Form Settings")]
    public NormalHeadFormSettings normalHeadForm;
    public NormalHeadFormSettings fishForm;
    public NormalHeadFormSettings superJumpForm;

    [Header("Interaction Settings")]
    public PlayerInteractionSettings interactionSettings;

    public NormalHeadFormSettings GetFormSettings(PlayerFormType formType)
    {
        return formType switch
        {
            PlayerFormType.NormalHead => normalHeadForm,
            PlayerFormType.Fish => fishForm,
            PlayerFormType.SuperJump => superJumpForm,
            _ => normalHeadForm
        };
    }
}