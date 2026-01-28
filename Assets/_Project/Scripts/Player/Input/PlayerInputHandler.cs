using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Settings")]
    public string horizontalAxis = "Horizontal";
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode interactKey = KeyCode.E;

    private PlayerController playerController;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (playerController == null)
        {
            return;
        }

        playerController.SetMovementInput(Input.GetAxisRaw(horizontalAxis));

        if (Input.GetKeyDown(jumpKey))
        {
            playerController.QueueJumpInput();
        }

        if (Input.GetKeyDown(interactKey))
        {
            playerController.QueueInteractInput();
        }
    }
}