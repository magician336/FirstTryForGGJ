using UnityEngine;

[CreateAssetMenu(fileName = "SuperJumpFormSettings", menuName = "Player/Settings/Form Settings")]
public class SuperJumpFormSettings : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Physics")]
    public float gravityMultiplier = 1f;
}
