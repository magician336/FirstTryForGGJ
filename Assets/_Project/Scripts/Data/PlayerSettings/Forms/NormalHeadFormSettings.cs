using UnityEngine;

[CreateAssetMenu(fileName = "NormalHeadFormSettings", menuName = "Player/Settings/Form Settings")]
public class NormalHeadFormSettings : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Physics")]
    public float gravityMultiplier = 1f;
}
