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
            Debug.Log($"[PlayerFormUnlockSettings] 资产 '{name}' 已添加新形态: {formType}");

            // 关键修复：在编辑器环境下标记为“脏”，确保 Inspector 同步显示
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            // 强制保存一次（可选，防止 PlayMode 退出后丢失）
            // UnityEditor.AssetDatabase.SaveAssets(); 
#endif
        }
        else
        {
            Debug.Log($"[PlayerFormUnlockSettings] 形态 {formType} 已经存在于资产 '{name}' 中。");
        }
    }
}
