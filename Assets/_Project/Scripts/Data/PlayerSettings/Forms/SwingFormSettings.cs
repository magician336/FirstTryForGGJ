using UnityEngine;

[CreateAssetMenu(fileName = "SwingFormSettings", menuName = "Player/Settings/SwingFormSettings")]
public class SwingFormSettings : NormalHeadFormSettings
{
    [Header("Swing Settings")]
    public float maxWebDistance = 10f;
    public float climbSpeed = 3f;
    public float swingForce = 15f;
    public LayerMask grappleLayer;
}
