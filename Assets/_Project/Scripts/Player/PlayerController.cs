using System.Collections.Generic;
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
    [SerializeField] private PlayerFormType startingForm = PlayerFormType.NormalHead;
    [Header("Super Jump Settings")]
    [SerializeField] private float superJumpMinMultiplier = 1f;
    [SerializeField] private float superJumpMaxMultiplier = 2.5f;
    [SerializeField] private float superJumpMaxChargeTime = 1.5f;

    private PlayerStateMachine stateMachine;
    private Rigidbody2D body;
    private PlayerInputHandler cachedInputHandler;
    private float baseGravityScale = 1f;

    private readonly Dictionary<PlayerFormType, PlayerFormStateBundle> formBundles = new();
    private PlayerFormType currentFormType = PlayerFormType.NormalHead;
    private PlayerFormStateFactory currentFormFactory;

    private IPlayerState idleState;
    private IPlayerState runState;
    private IPlayerState jumpState;
    private IPlayerState fallState;
    private IPlayerState interactState;
    private IPlayerState superJumpState;

    private float movementInput;
    private bool jumpRequested;
    private bool interactRequested;
    private bool isChargingSuperJump;
    private float superJumpChargeTimer;
    private bool hasPendingSuperJump;
    private float pendingSuperJumpMultiplier;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        baseGravityScale = body != null ? body.gravityScale : 1f;
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
        pendingSuperJumpMultiplier = superJumpMinMultiplier;

        stateMachine = new PlayerStateMachine();
        InitializeFormSystem();
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

        ApplyFormSettings(currentFormType);

        var interactionSettings = playerSettings.interactionSettings;
        if (interactionSettings != null)
        {
            if (interactionController != null)
            {
                interactionController.interactKey = interactionSettings.interactKey;
                interactionController.interactRange = interactionSettings.interactRange;
            }

            if (cachedInputHandler != null)
            {
                cachedInputHandler.interactKey = interactionSettings.interactKey;
            }
        }
    }

    private void InitializeFormSystem()
    {
        formBundles.Clear();
        currentFormFactory = null;
        SwitchForm(startingForm, true);
    }

    public void SwitchForm(PlayerFormType newForm)
    {
        SwitchForm(newForm, false);
    }

    public void SwitchForm(PlayerFormType newForm, bool force)
    {
        if (!force && currentFormType == newForm)
        {
            return;
        }

        var factory = PlayerFormFactoryRegistry.GetFactory(newForm);
        if (factory == null)
        {
            Debug.LogError($"No PlayerFormStateFactory registered for form {newForm}");
            return;
        }

        currentFormFactory = factory;
        currentFormType = newForm;

        if (newForm != PlayerFormType.SuperJump)
        {
            ClearSuperJumpData();
        }

        currentFormFactory.ApplyFormSettings(this);

        if (!formBundles.TryGetValue(newForm, out var bundle))
        {
            bundle = currentFormFactory.CreateStateBundle(this);
            formBundles[newForm] = bundle;
        }

        BindStateBundle(bundle);

        var desiredStateId = stateMachine.CurrentState?.GetState() ?? PlayerStates.Idle;
        var targetState = bundle.GetStateOrDefault(desiredStateId) ?? bundle.DefaultState;

        if (stateMachine.CurrentState == null)
        {
            stateMachine.Initialize(targetState);
        }
        else
        {
            stateMachine.ChangeState(targetState);
        }
    }

    private void BindStateBundle(PlayerFormStateBundle bundle)
    {
        idleState = bundle.GetStateOrDefault(PlayerStates.Idle);
        runState = bundle.GetStateOrDefault(PlayerStates.Run);
        jumpState = bundle.GetStateOrDefault(PlayerStates.Jump);
        fallState = bundle.GetStateOrDefault(PlayerStates.Fall);
        interactState = bundle.GetStateOrDefault(PlayerStates.Interact);
        superJumpState = bundle.GetStateOrDefault(PlayerStates.SuperJump);
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
            OnJumpButtonDown();
        }

        if (Input.GetButton("Jump"))
        {
            OnJumpButtonHeld(Time.deltaTime);
        }

        if (Input.GetButtonUp("Jump"))
        {
            OnJumpButtonUp();
        }

        KeyCode interactKey = playerSettings != null && playerSettings.interactionSettings != null
            ? playerSettings.interactionSettings.interactKey
            : KeyCode.E;
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
        }
    }

    public void SetMovementInput(float value)
    {
        movementInput = Mathf.Clamp(value, -1f, 1f);
    }

    public void QueueJumpInput()
    {
        var currentStateId = stateMachine?.CurrentState?.GetState();
        if (currentStateId == PlayerStates.Idle || currentStateId == PlayerStates.Run)
        {
            jumpRequested = true;
        }
    }

    public void QueueInteractInput()
    {
        interactRequested = true;
    }

    public void OnJumpButtonDown()
    {
        if (IsSuperJumpFormActive())
        {
            BeginSuperJumpCharge();
            return;
        }

        QueueJumpInput();
    }

    public void OnJumpButtonHeld(float deltaTime)
    {
        if (IsSuperJumpFormActive())
        {
            ChargeSuperJump(deltaTime);
        }
    }

    public void OnJumpButtonUp()
    {
        if (IsSuperJumpFormActive())
        {
            ReleaseSuperJumpCharge();
        }
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

    public void ExecuteJump(float forceMultiplier = 1f)
    {
        movementController?.Jump(forceMultiplier);
    }

    public bool PerformInteraction()
    {
        return interactionController != null && interactionController.TryInteract();
    }

    public void ChangeState(IPlayerState newState)
    {
        stateMachine?.ChangeState(newState);
    }

    public PlayerFormType CurrentForm => currentFormType;

    public void ApplyMovementProfile(float moveMultiplier, float jumpMultiplier)
    {
        if (movementController == null || playerSettings == null)
        {
            return;
        }

        var formSettings = playerSettings.GetFormSettings(currentFormType);
        if (formSettings == null)
        {
            return;
        }

        movementController.moveSpeed = formSettings.moveSpeed * Mathf.Max(0.1f, moveMultiplier);
        movementController.jumpForce = formSettings.jumpForce * Mathf.Max(0.1f, jumpMultiplier);
    }

    public void ApplyGravityMultiplier(float gravityMultiplier)
    {
        if (body == null)
        {
            return;
        }

        body.gravityScale = baseGravityScale * Mathf.Max(0f, gravityMultiplier);
    }

    public void ApplyFormSettings(PlayerFormType formType)
    {
        if (playerSettings == null)
        {
            return;
        }

        var formSettings = playerSettings.GetFormSettings(formType);
        if (formSettings == null || movementController == null)
        {
            return;
        }

        movementController.moveSpeed = formSettings.moveSpeed;
        movementController.jumpForce = formSettings.jumpForce;
        ApplyGravityMultiplier(formSettings.gravityMultiplier);
    }

    private bool IsSuperJumpFormActive()
    {
        return currentFormType == PlayerFormType.SuperJump && superJumpState != null;
    }

    private bool IsInGroundedLocomotionState()
    {
        var stateId = stateMachine?.CurrentState?.GetState();
        return stateId == PlayerStates.Idle || stateId == PlayerStates.Run;
    }

    private void BeginSuperJumpCharge()
    {
        if (isChargingSuperJump || !IsGrounded || !IsInGroundedLocomotionState())
        {
            return;
        }

        isChargingSuperJump = true;
        superJumpChargeTimer = 0f;
        pendingSuperJumpMultiplier = superJumpMinMultiplier;
    }

    private void ChargeSuperJump(float deltaTime)
    {
        if (!isChargingSuperJump)
        {
            return;
        }

        if (!IsGrounded || !IsInGroundedLocomotionState())
        {
            CancelSuperJumpCharge();
            return;
        }

        superJumpChargeTimer += Mathf.Max(0f, deltaTime);
        superJumpChargeTimer = Mathf.Min(superJumpChargeTimer, superJumpMaxChargeTime);
        pendingSuperJumpMultiplier = CalculateSuperJumpMultiplier();
    }

    private void ReleaseSuperJumpCharge()
    {
        if (!isChargingSuperJump)
        {
            return;
        }

        isChargingSuperJump = false;

        if (!IsGrounded || superJumpState == null || !IsInGroundedLocomotionState())
        {
            CancelSuperJumpCharge();
            return;
        }

        hasPendingSuperJump = true;
        pendingSuperJumpMultiplier = CalculateSuperJumpMultiplier();
        stateMachine?.ChangeState(superJumpState);
        ResetSuperJumpCharge();
    }

    private void CancelSuperJumpCharge()
    {
        isChargingSuperJump = false;
        ResetSuperJumpCharge();
    }

    private void ResetSuperJumpCharge()
    {
        superJumpChargeTimer = 0f;
        pendingSuperJumpMultiplier = superJumpMinMultiplier;
    }

    private void ClearSuperJumpData()
    {
        isChargingSuperJump = false;
        hasPendingSuperJump = false;
        ResetSuperJumpCharge();
    }

    private float CalculateSuperJumpMultiplier()
    {
        if (superJumpMaxChargeTime <= 0f)
        {
            return superJumpMaxMultiplier;
        }

        var normalized = Mathf.Clamp01(superJumpChargeTimer / superJumpMaxChargeTime);
        return Mathf.Lerp(superJumpMinMultiplier, superJumpMaxMultiplier, normalized);
    }

    public bool TryConsumeSuperJumpCharge(out float multiplier)
    {
        if (!hasPendingSuperJump)
        {
            multiplier = superJumpMinMultiplier;
            return false;
        }

        hasPendingSuperJump = false;
        multiplier = Mathf.Max(0.1f, pendingSuperJumpMultiplier);
        pendingSuperJumpMultiplier = superJumpMinMultiplier;
        return true;
    }

    public IPlayerState IdleState => idleState;
    public IPlayerState RunState => runState;
    public IPlayerState JumpState => jumpState;
    public IPlayerState FallState => fallState;
    public IPlayerState InteractState => interactState;
    public IPlayerState SuperJumpState => superJumpState;
}