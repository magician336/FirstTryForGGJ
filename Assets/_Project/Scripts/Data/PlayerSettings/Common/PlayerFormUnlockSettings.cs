using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerFormUnlockSettings", menuName = "Player/Settings/Form Unlock Settings")]
public class PlayerFormUnlockSettings : ScriptableObject
{
    [SerializeField] private List<PlayerFormType> unlockedForms = new() { PlayerFormType.NormalHead };

    public IReadOnlyList<PlayerFormType> UnlockedForms => unlockedForms;

    public bool IsFormUnlocked(PlayerFormType formType)
    {
        if (unlockedForms == null)
        {
            return false;
        }

        return unlockedForms.Contains(formType);
    }

    public void EnsureUnlocked(PlayerFormType formType)
    {
        if (unlockedForms == null)
        {
            unlockedForms = new List<PlayerFormType>();
        }

        if (!unlockedForms.Contains(formType))
        {
            unlockedForms.Add(formType);
        }
    }
}
