using UnityEngine;

[CreateAssetMenu(fileName = "FishFormSettings", menuName = "Player/Settings/FishFormSettings")]
public class FishFormSettings : NormalHeadFormSettings
{
    [Header("Movement Override")]
    [Min(0.1f)] public float swimMoveSpeed = 4f;

    [Header("Water Detection")]
    [SerializeField] private LayerMask waterLayer;
    public LayerMask WaterLayer => waterLayer;

    [Header("Squid Ink Projectile")]
    public SquidInk squidInkPrefab;
    [Min(0.1f)] public float inkSpeed = 1f;
    [Min(0.1f)] public float inkLifetime = 2f;
    [Min(0f)] public float inkCooldown = 0.5f;
}
