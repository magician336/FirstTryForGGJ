using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Player/Settings/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    [Header("Form Settings")]
    public NormalHeadFormSettings normalHeadForm;
    public FishFormSettings fishForm;
    public SuperJumpFormSettings superJumpForm;

    [Header("Input Settings")]
    public InputSettings inputSettings;

    [Header("Interaction Settings")]
    public PlayerInteractionSettings interactionSettings;

    [Header("Combat Settings")]
    public PlayerCombatSettings combatSettings;

    [Header("Form Unlock Settings")]
    public PlayerFormUnlockSettings formUnlockSettings;

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