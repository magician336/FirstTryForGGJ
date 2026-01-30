using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInteractionSettings", menuName = "Player/Settings/PlayerInteractionSettings")]
public class PlayerInteractionSettings : ScriptableObject
{
    public KeyCode interactKey = KeyCode.E;
    public float interactRange = 1.5f;
}
