using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Settings")]
    public string horizontalAxis = "Horizontal";
    [SerializeField] private KeyCode fallbackJumpKey = KeyCode.Space;
    [SerializeField] private KeyCode fallbackInteractKey = KeyCode.F;
    [SerializeField] private KeyCode fallbackNextFormKey = KeyCode.E;
    [SerializeField] private KeyCode fallbackPreviousFormKey = KeyCode.Q;
    [SerializeField] private InputSettings inputSettings;

    private PlayerController playerController;

    public KeyCode jumpKey { set { fallbackJumpKey = value; } }
    public KeyCode interactKey { set { fallbackInteractKey = value; } }
    public KeyCode nextFormKey { set { fallbackNextFormKey = value; } }
    public KeyCode previousFormKey { set { fallbackPreviousFormKey = value; } }

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void ApplyInputSettings(InputSettings settings)
    {
        inputSettings = settings;
    }

    private KeyCode GetJumpKey()
    {
        return inputSettings != null ? inputSettings.JumpKey : fallbackJumpKey;
    }

    private KeyCode GetInteractKey()
    {
        return inputSettings != null ? inputSettings.InteractKey : fallbackInteractKey;
    }

    private KeyCode GetNextFormKey()
    {
        return inputSettings != null ? inputSettings.NextFormKey : fallbackNextFormKey;
    }

    private KeyCode GetPreviousFormKey()
    {
        return inputSettings != null ? inputSettings.PreviousFormKey : fallbackPreviousFormKey;
    }

    void Update()
    {
        if (playerController == null)
        {
            return;
        }

        string hAxis = (inputSettings != null && !string.IsNullOrEmpty(inputSettings.HorizontalAxis))
             ? inputSettings.HorizontalAxis : horizontalAxis;

        playerController.SetMovementInput(Input.GetAxisRaw(hAxis));

        var resolvedJumpKey = GetJumpKey();
        if (Input.GetKeyDown(resolvedJumpKey))
        {
            playerController.OnJumpButtonDown();
        }

        if (Input.GetKey(resolvedJumpKey))
        {
            playerController.OnJumpButtonHeld(Time.deltaTime);
        }

        if (Input.GetKeyUp(resolvedJumpKey))
        {
            playerController.OnJumpButtonUp();
        }

        if (Input.GetKeyDown(GetInteractKey()))
        {
            playerController.QueueInteractInput();
        }

        if (Input.GetKeyDown(GetNextFormKey()))
        {
            playerController.RequestNextForm();
        }

        if (Input.GetKeyDown(GetPreviousFormKey()))
        {
            playerController.RequestPreviousForm();
        }
    }
}