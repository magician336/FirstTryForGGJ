using UnityEngine;

[CreateAssetMenu(menuName = "Data/Player/Input Settings", fileName = "PlayerInputSettings")]
public class InputSettings : ScriptableObject
{
    [Header("Axis Settings")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";
    [SerializeField] private bool useRawAxis = true;

    [Header("Action Keys")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    [SerializeField] private KeyCode nextFormKey = KeyCode.E;
    [SerializeField] private KeyCode previousFormKey = KeyCode.Q;
    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

    public string HorizontalAxis => horizontalAxis;
    public string VerticalAxis => verticalAxis;
    public bool UseRawAxis => useRawAxis;

    public KeyCode JumpKey => jumpKey;
    public KeyCode InteractKey => interactKey;
    public KeyCode NextFormKey => nextFormKey;
    public KeyCode PreviousFormKey => previousFormKey;
    public KeyCode FireKey => fireKey;
    public KeyCode DashKey => dashKey;

    [ContextMenu("Reset To Defaults")]
    public void ResetToDefaults()
    {
        horizontalAxis = "Horizontal";
        verticalAxis = "Vertical";
        useRawAxis = true;
        jumpKey = KeyCode.Space;
        interactKey = KeyCode.F;
        nextFormKey = KeyCode.E;
        previousFormKey = KeyCode.Q;
        fireKey = KeyCode.Mouse0;
        dashKey = KeyCode.LeftShift;
    }
}
