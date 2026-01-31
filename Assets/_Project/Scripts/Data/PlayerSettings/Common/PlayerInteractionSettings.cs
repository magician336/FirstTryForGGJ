using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInteractionSettings", menuName = "Player/Settings/PlayerInteractionSettings")]
public class PlayerInteractionSettings : ScriptableObject
{
    [Tooltip("Maximum distance at which interactions are allowed.")]
    public float interactRange = 1.5f;
}
