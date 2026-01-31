using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerFormUnlockSettings", menuName = "Player/Settings/Form Unlock Settings")]
public class PlayerFormUnlockSettings : ScriptableObject
{
    [System.Serializable]
    public class FormSkinUnlockData
    {
        public PlayerFormType formType;
        public List<int> unlockedIndices = new() { 0 }; // 默认解锁第一个索引
    }

    [SerializeField] private List<PlayerFormType> unlockedForms = new() { PlayerFormType.NormalHead };
    [SerializeField] private List<FormSkinUnlockData> unlockedSkins = new();

    public IReadOnlyList<PlayerFormType> UnlockedForms => unlockedForms;

    public bool IsFormUnlocked(PlayerFormType formType)
    {
        if (unlockedForms == null)
        {
            return false;
        }

        return unlockedForms.Contains(formType);
    }

    public bool IsSkinUnlocked(PlayerFormType formType, int skinIndex)
    {
        // 索引 0 默认永远解锁
        if (skinIndex == 0) return true;

        var data = unlockedSkins.Find(d => d.formType == formType);
        return data != null && data.unlockedIndices.Contains(skinIndex);
    }

    public void EnsureFormUnlocked(PlayerFormType formType) => EnsureUnlocked(formType);

    public void EnsureUnlocked(PlayerFormType formType)
    {
        // ...existing code...
        if (!unlockedForms.Contains(formType))
        {
            unlockedForms.Add(formType);

            // 同时确保皮肤解锁数据中也有该形态记录
            if (unlockedSkins.Find(d => d.formType == formType) == null)
            {
                unlockedSkins.Add(new FormSkinUnlockData { formType = formType });
            }

            Debug.Log($"[PlayerFormUnlockSettings] 资产 '{name}' 已添加新形态: {formType}");

            MarkDirty();
        }
        // ...existing code...
    }

    public void EnsureSkinUnlocked(PlayerFormType formType, int skinIndex)
    {
        var data = unlockedSkins.Find(d => d.formType == formType);
        if (data == null)
        {
            data = new FormSkinUnlockData { formType = formType };
            unlockedSkins.Add(data);
        }

        if (!data.unlockedIndices.Contains(skinIndex))
        {
            data.unlockedIndices.Add(skinIndex);
            Debug.Log($"[PlayerFormUnlockSettings] 资产 '{name}' 已为形态 {formType} 解锁新皮肤索引: {skinIndex}");
            MarkDirty();
        }
    }

    private void MarkDirty()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
