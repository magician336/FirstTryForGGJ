using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCombatSettings", menuName = "Player/Settings/PlayerCombatSettings")]
public class PlayerCombatSettings : ScriptableObject
{
    [Header("Attributes")]
    [Min(1)] public int maxHealth = 3;
    [Min(0)] public int attackPower = 1;

    [Header("Scaling Curves")]
    public AnimationCurve damageFalloff = AnimationCurve.Linear(0f, 1f, 1f, 1f);
}
