using UnityEngine;

public static class PhysicsLayers
{
    public const int Player = 1 << 8; // Player layer
    public const int Ground = 1 << 9; // Ground layer
    public const int Interactable = 1 << 10; // Interactable layer
}