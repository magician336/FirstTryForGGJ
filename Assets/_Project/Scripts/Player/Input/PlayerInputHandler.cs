using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Settings")]
    public string horizontalAxis = "Horizontal";
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode interactKey = KeyCode.J;

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
            playerController.OnJumpButtonDown();
        }

        if (Input.GetKey(jumpKey))
        {
            playerController.OnJumpButtonHeld(Time.deltaTime);
        }

        if (Input.GetKeyUp(jumpKey))
        {
            playerController.OnJumpButtonUp();
        }

        if (Input.GetKeyDown(interactKey))
        {
            playerController.QueueInteractInput();
        }
    }
}