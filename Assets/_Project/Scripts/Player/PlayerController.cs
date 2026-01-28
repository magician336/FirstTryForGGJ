using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("组件")]
    [SerializeField] private MovementController movementController;
    [SerializeField] private GroundChecker groundChecker;
    [SerializeField] private InteractionController interactionController;
    [SerializeField] private PlayerSettings playerSettings;

    private PlayerStateMachine stateMachine;
    private Rigidbody2D body;
    private PlayerInputHandler cachedInputHandler;

    private PlayerIdleState idleState;
    private PlayerRunState runState;
    private PlayerJumpState jumpState;
    private PlayerFallState fallState;
    private PlayerInteractState interactState;

    private float movementInput;
    private bool jumpRequested;
    private bool interactRequested;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        cachedInputHandler = GetComponent<PlayerInputHandler>();

        if (movementController == null)
        {
            movementController = GetComponent<MovementController>();
        }

        if (groundChecker == null)
        {
            groundChecker = GetComponentInChildren<GroundChecker>();
        }

        if (interactionController == null)
        {
            interactionController = GetComponent<InteractionController>();
        }

        ApplySettings();

        stateMachine = new PlayerStateMachine();
        InitializeStates();
    }

    void Update()
    {
        CaptureFallbackInput();
        UpdateGroundState();

        if (stateMachine?.CurrentState == null)
        {
            return;
        }

        stateMachine.CurrentState.HandleInput();
        stateMachine.CurrentState.LogicUpdate();
    }

    private void ApplySettings()
    {
        if (playerSettings == null)
        {
            return;
        }

        if (movementController != null)
        {
            movementController.moveSpeed = playerSettings.moveSpeed;
            movementController.jumpForce = playerSettings.jumpForce;
        }

        if (interactionController != null)
        {
            interactionController.interactKey = playerSettings.interactKey;
            interactionController.interactRange = playerSettings.interactRange;
        }

        if (cachedInputHandler != null)
        {
            cachedInputHandler.interactKey = playerSettings.interactKey;
        }
    }

    private void InitializeStates()
    {
        idleState = new PlayerIdleState(this);
        runState = new PlayerRunState(this);
        jumpState = new PlayerJumpState(this);
        fallState = new PlayerFallState(this);
        interactState = new PlayerInteractState(this);

        stateMachine.Initialize(idleState);
    }

    private void CaptureFallbackInput()
    {
        if (cachedInputHandler != null && cachedInputHandler.isActiveAndEnabled)
        {
            return;
        }

        SetMovementInput(Input.GetAxisRaw("Horizontal"));

        if (Input.GetButtonDown("Jump"))
        {
            QueueJumpInput();
        }

        KeyCode interactKey = playerSettings != null ? playerSettings.interactKey : KeyCode.E;
        if (Input.GetKeyDown(interactKey))
        {
            QueueInteractInput();
        }
    }

    private void UpdateGroundState()
    {
        if (movementController != null && groundChecker != null)
        {
            movementController.SetGrounded(groundChecker.IsGrounded);
            if (groundChecker.IsGrounded) Debug.Log("已经踩在地面上了");
        }
    }

    public void SetMovementInput(float value)
    {
        movementInput = Mathf.Clamp(value, -1f, 1f);
    }

    public void QueueJumpInput()
    {
        if (stateMachine.CurrentState.GetState() == PlayerStates.Idle || stateMachine.CurrentState.GetState() == PlayerStates.Run)
            jumpRequested = true;
    }

    public void QueueInteractInput()
    {
        interactRequested = true;
    }

    // Backwards compatibility for existing callers
    public void TryJump() => QueueJumpInput();
    public void TryInteract() => QueueInteractInput();

    public float HorizontalInput => movementInput;

    public bool ConsumeJumpInput()
    {
        if (!jumpRequested)
        {
            return false;
        }

        jumpRequested = false;
        return true;
    }

    public bool ConsumeInteractInput()
    {
        if (!interactRequested)
        {
            return false;
        }

        interactRequested = false;
        return true;
    }

    public bool IsGrounded => groundChecker != null && groundChecker.IsGrounded;

    public float VerticalVelocity => body != null ? body.velocity.y : 0f;

    public void Move(float normalizedInput)
    {
        movementController?.Move(normalizedInput);
    }

    public void ExecuteJump()
    {
        movementController?.Jump();
    }

    public bool PerformInteraction()
    {
        return interactionController != null && interactionController.TryInteract();
    }

    public void ChangeState(IPlayerState newState)
    {
        stateMachine?.ChangeState(newState);
    }

    public IPlayerState IdleState => idleState;
    public IPlayerState RunState => runState;
    public IPlayerState JumpState => jumpState;
    public IPlayerState FallState => fallState;
    public IPlayerState InteractState => interactState;
}