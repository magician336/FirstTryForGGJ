using UnityEngine;

[CreateAssetMenu(fileName = "PlayerFormPresentation", menuName = "Player/Settings/Form Presentation")]
public class PlayerFormPresentation : ScriptableObject
{
    [Header("Visuals")]
    public Sprite bodySprite;
    public GameObject prefabOverride;
    public Vector3 prefabOffset;

    [Header("Animation")]
    public AnimatorOverrideController animatorOverride;

    [Header("Effects")]
    public string switchSfxKey;
    public GameObject switchVfxPrefab;
    public Vector3 vfxOffset;
}
