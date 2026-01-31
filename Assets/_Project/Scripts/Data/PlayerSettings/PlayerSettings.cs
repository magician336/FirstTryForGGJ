using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Player/Settings/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    [Header("Form Skins")]
    public List<NormalHeadFormSettings> normalHeadSkins = new();
    public List<FishFormSettings> fishSkins = new();
    public List<SuperJumpFormSettings> superJumpSkins = new();
    public List<SwingFormSettings> swingSkins = new();

    [Header("Global Settings")]
    public InputSettings inputSettings;
    public PlayerLadderSettings ladderSettings;
    public PlayerInteractionSettings interactionSettings;
    public PlayerCombatSettings combatSettings;
    public PlayerFormUnlockSettings formUnlockSettings;

    public int GetSkinCount(PlayerFormType formType)
    {
        return formType switch
        {
            PlayerFormType.NormalHead => normalHeadSkins.Count,
            PlayerFormType.Fish => fishSkins.Count,
            PlayerFormType.SuperJump => superJumpSkins.Count,
            PlayerFormType.Spider => swingSkins.Count,
            _ => 0
        };
    }

    public NormalHeadFormSettings GetFormSettings(PlayerFormType formType, int skinIndex = 0)
    {
        return formType switch
        {
            PlayerFormType.NormalHead => GetFromList(normalHeadSkins, skinIndex),
            PlayerFormType.Fish => GetFromList(fishSkins, skinIndex),
            PlayerFormType.SuperJump => GetFromList(superJumpSkins, skinIndex),
            PlayerFormType.Spider => GetFromList(swingSkins, skinIndex),
            _ => null
        };
    }

    private T GetFromList<T>(List<T> list, int index) where T : ScriptableObject
    {
        if (list == null || list.Count == 0) return null;
        int safeIndex = Mathf.Clamp(index, 0, list.Count - 1);
        return list[safeIndex];
    }
}