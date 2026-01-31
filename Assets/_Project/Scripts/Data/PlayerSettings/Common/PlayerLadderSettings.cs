using UnityEngine;

[System.Serializable]
public class PlayerLadderSettings
{
    [Header("Climbing")]
    public float climbSpeed = 3f;
    public float ladderJumpForce = 5f;
    public LayerMask ladderLayer;
}
