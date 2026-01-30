using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Player/Settings/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    [Header("Form Settings")]
    public NormalHeadFormSettings normalHeadForm;
    public NormalHeadFormSettings fishForm;
    public SuperJumpFormSettings superJumpForm;

    [Header("Interaction Settings")]
    public PlayerInteractionSettings interactionSettings;

    [Header("Combat Settings")]
    public PlayerCombatSettings combatSettings;

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