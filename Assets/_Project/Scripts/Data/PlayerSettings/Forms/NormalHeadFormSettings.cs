using UnityEngine;

[CreateAssetMenu(fileName = "NormalHeadFormSettings", menuName = "Player/Settings/NormalHeadFormSettings")]
public class NormalHeadFormSettings : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Physics")]
    public float gravityMultiplier = 1f;

    [Header("Presentation")]
    public PlayerFormPresentation presentation;
}
