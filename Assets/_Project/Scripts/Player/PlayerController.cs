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
    [SerializeField] private SwingController swingController;
    [SerializeField] private PlayerPresentationBinder presentationBinder;
    [SerializeField] private PlayerSettings playerSettings;
    [SerializeField] private PlayerFormType startingForm = PlayerFormType.NormalHead;
    [SerializeField] private HealthController healthController;

    // Public accessor for settings
    public PlayerSettings Settings => playerSettings;

    private PlayerStateMachine stateMachine;
    private Rigidbody2D body;
    private PlayerInputHandler cachedInputHandler;
    private float baseGravityScale = 1f;

    private readonly Dictionary<PlayerFormType, PlayerFormStateBundle> formBundles = new();
    private PlayerFormType currentFormType = PlayerFormType.NormalHead;
    private PlayerFormStateFactory currentFormFactory;
    private SuperJumpFormSettings SuperJumpSettings => playerSettings != null ? playerSettings.superJumpForm : null;
    private PlayerCombatSettings CombatSettings => playerSettings != null ? playerSettings.combatSettings : null;
    private PlayerFormUnlockSettings FormUnlockSettings => playerSettings != null ? playerSettings.formUnlockSettings : null;
    private InputSettings InputSettingsAsset => playerSettings != null ? playerSettings.inputSettings : null;
    private readonly List<PlayerFormType> unlockedFormBuffer = new();

    private IPlayerState idleState;
    private IPlayerState runState;
    private IPlayerState jumpState;
    private IPlayerState fallState;
    private IPlayerState interactState;
    private IPlayerState superJumpState;
    private IPlayerState swingState;
    private IPlayerState onLadderState;
    private IPlayerState deadState;
    private IPlayerState swimIdleState;
    private IPlayerState swimRunState;

    private float movementInput;
    private float verticalInput;
    private bool jumpRequested;
    private bool interactRequested;
    private bool isChargingSuperJump;
    private float superJumpChargeTimer;
    private bool hasPendingSuperJump;
    private float pendingSuperJumpMultiplier;
    private int currentHealth;

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

        if (presentationBinder == null)
        {
            presentationBinder = GetComponentInChildren<PlayerPresentationBinder>();
        }

        if (swingController == null)
        {
            swingController = GetComponent<SwingController>();
        }

        if (healthController == null)
        {
            healthController = GetComponent<HealthController>();
        }
        if (healthController != null)
        {
            healthController.OnDie += OnDie_Handler;
        }

        ApplySettings();
        ResetSuperJumpCharge();
        InitializeCombatStats();

        stateMachine = new PlayerStateMachine();
        InitializeFormSystem();

        Debug.Log($"[PlayerController] Jump: {GetJumpKey()}, Interact: {GetInteractKey()}, Next: {GetNextFormKey()}, Prev: {GetPreviousFormKey()}");
    }

    void Update()
    {
        CaptureFallbackInput();
        UpdateGroundState();
        UpdateLadderState();
        UpdateWaterState();

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
        if (interactionController != null)
        {
            interactionController.ApplyInputSettings(InputSettingsAsset);

            if (interactionSettings != null)
            {
                interactionController.interactRange = interactionSettings.interactRange;
            }
        }

        if (cachedInputHandler != null)
        {
            cachedInputHandler.ApplyInputSettings(InputSettingsAsset);
            cachedInputHandler.jumpKey = GetJumpKey();
            cachedInputHandler.interactKey = GetInteractKey();
            cachedInputHandler.nextFormKey = GetNextFormKey();
            cachedInputHandler.previousFormKey = GetPreviousFormKey();
            cachedInputHandler.swingKey = GetSwingKey();
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
        Debug.Log("SwitchForm");
        if (!force && currentFormType == newForm)
        {
            return;
        }

        if (!force && !IsFormUnlocked(newForm))
        {
            Debug.LogWarning($"Attempted to switch to locked form {newForm}");
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
        Debug.Log($"[FormSwitch] 正在切换到形态: {newForm}");

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
        swingState = bundle.GetStateOrDefault(PlayerStates.Swing);

        // OnLadder is a shared state, usually not in the bundle specific to a form, 
        // but we can look for it or create a default instance if missing.
        // For now, let's assume it might be in the bundle, or we fallback to a new instance.
        onLadderState = bundle.GetStateOrDefault(PlayerStates.OnLadder) ?? new PlayerOnLadderState(this);

        deadState = bundle.GetStateOrDefault(PlayerStates.Dead) ?? new PlayerDeadState(this);

        // Swim states - only available in Fish form
        swimIdleState = bundle.GetStateOrDefault(PlayerStates.SwimIdle);
        swimRunState = bundle.GetStateOrDefault(PlayerStates.SwimRun);

        ApplyPresentationForCurrentForm();
    }

    private void OnDie_Handler()
    {
        ChangeState(deadState);
    }

    public void Teleport(Vector3 position)
    {
        transform.position = position;
        if (body != null)
        {
            body.velocity = Vector2.zero;
        }
    }

    public void Revive()
    {
        // Reset necessary flags
        ResetSuperJumpCharge();

        // Return to Idle
        ChangeState(idleState);
        Debug.Log("Player Revived!");
    }

    private void CaptureFallbackInput()
    {
        if (cachedInputHandler != null && cachedInputHandler.isActiveAndEnabled)
        {
            return;
        }

        SetMovementInput(Input.GetAxisRaw("Horizontal"));
        SetVerticalInput(Input.GetAxisRaw("Vertical"));

        var jumpKey = GetJumpKey();
        if (Input.GetKeyDown(jumpKey))
        {
            OnJumpButtonDown();
        }

        if (Input.GetKey(jumpKey))
        {
            OnJumpButtonHeld(Time.deltaTime);
        }

        if (Input.GetKeyUp(jumpKey))
        {
            OnJumpButtonUp();
        }

        if (Input.GetKeyDown(GetInteractKey()))
        {
            QueueInteractInput();
        }

        if (Input.GetKeyDown(GetNextFormKey()))
        {
            RequestNextForm();
        }

        if (Input.GetKeyDown(GetPreviousFormKey()))
        {
            RequestPreviousForm();
        }

        if (Input.GetKeyDown(GetSwingKey()))
        {
            OnSwingButtonDown();
        }
    }

    private void UpdateGroundState()
    {
        if (movementController != null && groundChecker != null)
        {
            movementController.SetGrounded(groundChecker.IsGrounded);
        }
    }

    private void UpdateLadderState()
    {
        if (LadderSettings == null) return;

        IsTouchingLadder = Physics2D.OverlapCircle(transform.position, 0.5f, LadderSettings.ladderLayer);
    }

    private void UpdateWaterState()
    {
        var fishSettings = FishSettings;
        if (fishSettings == null)
        {
            IsInWater = false;
            return;
        }

        bool wasInWater = IsInWater;
        IsInWater = Physics2D.OverlapCircle(transform.position, 0.5f, fishSettings.WaterLayer);

        if (IsInWater)
        {
            // 在水中时关闭重力，防止下沉
            SetGravityScale(0f);

            // 如果刚进入水域且是鱼形态，强制切换到游泳状态
            if (!wasInWater && CanSwim)
            {
                Debug.Log("[Player] 进入水域，切换至游泳状态");
                ChangeState(swimIdleState);
            }
        }
        else if (wasInWater)
        {
            // 离开水域，恢复基础重力
            SetGravityScale(baseGravityScale);
        }
    }

    public void MoveVertical(float amount)
    {
        if (body == null || LadderSettings == null) return;

        float targetVelocityY = amount * LadderSettings.climbSpeed;
        body.velocity = new Vector2(0f, targetVelocityY);
    }

    public void SetMovementInput(float value)
    {
        movementInput = Mathf.Clamp(value, -1f, 1f);
    }

    public void SetVerticalInput(float value)
    {
        verticalInput = Mathf.Clamp(value, -1f, 1f);
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
        Debug.Log("[PlayerController] 交互键按下，设置 interactRequested = true");
        interactRequested = true;
    }

    public bool ConsumeInteractInput()
    {
        if (!interactRequested)
        {
            return false;
        }

        Debug.Log("[PlayerController] 正在消耗交互输入");
        interactRequested = false;
        return true;
    }

    public void OnJumpButtonDown()
    {
        if (IsSuperJumpFormActive())
        {
            BeginSuperJumpCharge();
            return;
        }

        // 如果处于可以游泳的状态（鱼形态+在水中），按下跳跃键向上冲刺
        if (CanSwim)
        {
            SwimUp();
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

    public void OnSwingButtonDown()
    {
        if (swingController == null) return;

        // 如果当前形态没有 swingState（即不是蜘蛛形态），则不允许触发
        if (swingState == null)
        {
            Debug.Log("[Player] 当前形态无法开启摆荡（仅限蜘蛛形态）。");
            return;
        }

        bool isSwinging = stateMachine.CurrentState == swingState;

        if (isSwinging)
        {
            // Exit swing
            ChangeState(idleState);
        }
        else
        {
            // Enter swing
            // Use movementInput as direction, or fallback to transform scale/logic if needed to know facing direction
            // For now, assuming HorizontalInput determines facing, or defaulting to right
            float dir = HorizontalInput;
            if (Mathf.Abs(dir) < 0.01f) dir = 1f; // Default Right

            if (swingController.TryStartSwing(dir))
            {
                ChangeState(swingState);
            }
        }
    }

    // Backwards compatibility for existing callers
    public void TryJump() => QueueJumpInput();
    public void TryInteract() => QueueInteractInput();
    public void RequestNextForm() => CycleForm(1);
    public void RequestPreviousForm() => CycleForm(-1);

    public IPlayerState IdleState => idleState;
    public IPlayerState RunState => runState;
    public IPlayerState JumpState => jumpState;
    public IPlayerState FallState => fallState;
    public IPlayerState InteractState => interactState;
    public IPlayerState SuperJumpState => superJumpState;
    public IPlayerState SwingState => swingState;
    public IPlayerState OnLadderState => onLadderState;
    public IPlayerState SwimIdleState => swimIdleState;
    public IPlayerState SwimRunState => swimRunState;

    public bool IsGrounded => movementController != null && movementController.IsGrounded();
    public bool IsInWater { get; private set; }
    public bool CanSwim => currentFormType == PlayerFormType.Fish && IsInWater && swimIdleState != null;
    public float HorizontalInput => movementInput;
    public float VerticalInput => verticalInput;
    public float VerticalVelocity => body != null ? body.velocity.y : 0f;

    public PlayerLadderSettings LadderSettings => playerSettings != null ? playerSettings.ladderSettings : null;
    public FishFormSettings FishSettings => playerSettings != null ? playerSettings.fishForm : null;
    public bool IsTouchingLadder { get; private set; }

    /// <summary>
    /// 水中移动（水平+垂直）
    /// </summary>
    public void Swim(float horizontalInput, float verticalInput)
    {
        if (body == null) return;

        var fishSettings = FishSettings;
        float swimSpeed = fishSettings != null ? fishSettings.swimMoveSpeed : 4f;

        Vector2 swimVelocity = new Vector2(horizontalInput, verticalInput).normalized * swimSpeed;
        body.velocity = swimVelocity;
    }

    /// <summary>
    /// 水中向上冲刺
    /// </summary>
    public void SwimUp()
    {
        if (body == null) return;

        var fishSettings = FishSettings;
        float jumpForce = fishSettings != null ? fishSettings.jumpForce : 8f;
        body.velocity = new Vector2(body.velocity.x, jumpForce * 0.7f);
    }

    /// <summary>
    /// 水中阻力效果
    /// </summary>
    public void ApplySwimDrag()
    {
        if (body == null) return;
        body.velocity *= 0.95f; // 缓慢减速
    }
    public bool ConsumeJumpInput()
    {
        if (!jumpRequested)
        {
            return false;
        }

        jumpRequested = false;
        return true;
    }


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

        var baseMoveSpeed = GetFormMoveSpeed(formSettings);
        movementController.moveSpeed = baseMoveSpeed * Mathf.Max(0.1f, moveMultiplier);
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

        movementController.moveSpeed = GetFormMoveSpeed(formSettings);
        movementController.jumpForce = formSettings.jumpForce;
        ApplyGravityMultiplier(formSettings.gravityMultiplier);
        ApplyPresentationForCurrentForm();
    }

    private bool IsSuperJumpFormActive()
    {
        return currentFormType == PlayerFormType.SuperJump && superJumpState != null && SuperJumpSettings != null;
    }

    private float GetFormMoveSpeed(NormalHeadFormSettings formSettings)
    {
        if (formSettings == null)
        {
            return movementController != null ? movementController.moveSpeed : 0f;
        }

        if (formSettings is FishFormSettings fishSettings)
        {
            return Mathf.Max(0.1f, fishSettings.swimMoveSpeed);
        }

        return Mathf.Max(0.1f, formSettings.moveSpeed);
    }

    private void ApplyPresentationForCurrentForm()
    {
        if (playerSettings == null || presentationBinder == null)
        {
            return;
        }

        var formSettings = playerSettings.GetFormSettings(currentFormType);
        presentationBinder.ApplyPresentation(formSettings != null ? formSettings.presentation : null);
    }

    private KeyCode GetJumpKey()
    {
        var inputSettings = InputSettingsAsset;
        return inputSettings != null ? inputSettings.JumpKey : KeyCode.Space;
    }

    private KeyCode GetInteractKey()
    {
        var inputSettings = InputSettingsAsset;
        return inputSettings != null ? inputSettings.InteractKey : KeyCode.F;
    }

    private KeyCode GetNextFormKey()
    {
        var inputSettings = InputSettingsAsset;
        return inputSettings != null ? inputSettings.NextFormKey : KeyCode.E;
    }

    private KeyCode GetPreviousFormKey()
    {
        var inputSettings = InputSettingsAsset;
        return inputSettings != null ? inputSettings.PreviousFormKey : KeyCode.Q;
    }

    private KeyCode GetSwingKey()
    {
        var inputSettings = InputSettingsAsset;
        return inputSettings != null ? inputSettings.SwingKey : KeyCode.J;
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
        pendingSuperJumpMultiplier = GetSuperJumpMinMultiplier();
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
        superJumpChargeTimer = Mathf.Min(superJumpChargeTimer, GetSuperJumpMaxChargeTime());
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
        pendingSuperJumpMultiplier = GetSuperJumpMinMultiplier();
    }

    private void ClearSuperJumpData()
    {
        isChargingSuperJump = false;
        hasPendingSuperJump = false;
        ResetSuperJumpCharge();
    }

    private float CalculateSuperJumpMultiplier()
    {
        var maxChargeTime = GetSuperJumpMaxChargeTime();
        if (maxChargeTime <= 0f)
        {
            return GetSuperJumpMaxMultiplier();
        }

        var normalized = Mathf.Clamp01(superJumpChargeTimer / maxChargeTime);
        return Mathf.Lerp(GetSuperJumpMinMultiplier(), GetSuperJumpMaxMultiplier(), normalized);
    }

    public bool TryConsumeSuperJumpCharge(out float multiplier)
    {
        if (!hasPendingSuperJump)
        {
            multiplier = GetSuperJumpMinMultiplier();
            return false;
        }

        hasPendingSuperJump = false;
        multiplier = Mathf.Max(0.1f, pendingSuperJumpMultiplier);
        pendingSuperJumpMultiplier = GetSuperJumpMinMultiplier();
        return true;
    }

    private void InitializeCombatStats()
    {
        currentHealth = GetMaxHealth();
    }

    private float GetSuperJumpMinMultiplier()
    {
        var settings = SuperJumpSettings;
        var rawValue = settings != null ? settings.minChargeMultiplier : 1f;
        return Mathf.Max(0.1f, rawValue);
    }

    private float GetSuperJumpMaxMultiplier()
    {
        var settings = SuperJumpSettings;
        var minValue = GetSuperJumpMinMultiplier();
        if (settings == null)
        {
            return minValue;
        }

        return Mathf.Max(minValue, settings.maxChargeMultiplier);
    }

    private float GetSuperJumpMaxChargeTime()
    {
        var settings = SuperJumpSettings;
        if (settings == null)
        {
            return 0f;
        }

        return Mathf.Max(0f, settings.maxChargeTime);
    }

    private PlayerCombatSettings RequireCombatSettings()
    {
        return CombatSettings;
    }

    private int GetMaxHealth()
    {
        var settings = RequireCombatSettings();
        var configured = settings != null ? settings.maxHealth : 1;
        return Mathf.Max(1, configured);
    }

    private int GetAttackPower()
    {
        var settings = RequireCombatSettings();
        var configured = settings != null ? settings.attackPower : 1;
        return Mathf.Max(0, configured);
    }

    private float EvaluateDamageFalloff(float normalizedInput)
    {
        var settings = RequireCombatSettings();
        if (settings == null || settings.damageFalloff == null)
        {
            return 1f;
        }

        var clamped = Mathf.Clamp01(normalizedInput);
        return Mathf.Max(0f, settings.damageFalloff.Evaluate(clamped));
    }

    public void ApplyDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Min(GetMaxHealth(), currentHealth + amount);
    }

    public int GetAttackDamage(float normalizedFactor = 1f)
    {
        var falloffMultiplier = EvaluateDamageFalloff(normalizedFactor);
        var scaledDamage = Mathf.RoundToInt(GetAttackPower() * falloffMultiplier);
        return Mathf.Max(0, scaledDamage);
    }

    private void CycleForm(int direction)
    {
        var forms = GetUnlockedFormsBuffer();
        if (forms.Count <= 1)
        {
            return;
        }

        var currentIndex = forms.IndexOf(currentFormType);
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        var targetIndex = (currentIndex + direction) % forms.Count;
        if (targetIndex < 0)
        {
            targetIndex = forms.Count - 1;
        }

        var targetForm = forms[targetIndex];
        SwitchForm(targetForm);
    }

    private bool IsFormUnlocked(PlayerFormType formType)
    {
        if (formType == PlayerFormType.NormalHead)
        {
            return true;
        }

        if (FormUnlockSettings == null)
        {
            return false;
        }

        return FormUnlockSettings.IsFormUnlocked(formType);
    }

    private List<PlayerFormType> GetUnlockedFormsBuffer()
    {
        unlockedFormBuffer.Clear();

        if (FormUnlockSettings == null)
        {
            return unlockedFormBuffer;
        }

        unlockedFormBuffer.AddRange(FormUnlockSettings.UnlockedForms);

        return unlockedFormBuffer;
    }

    public void ForceUnlockForm(PlayerFormType formType)
    {
        if (FormUnlockSettings == null)
        {
            return;
        }

        FormUnlockSettings.EnsureUnlocked(formType);
        Debug.Log($"Form {formType} unlocked!");

        // Optional: Refresh buffer immediately if cached locally, but we clear it in GetUnlockedFormsBuffer anyway.
    }

    public void DebugDumpState()
    {
        Debug.Log($"[PlayerController] Current Form: {currentFormType}, State: {stateMachine?.CurrentState?.GetState()}");
    }

    private void OnDrawGizmosSelected()
    {
        if (LadderSettings != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }

    public void JumpOffLadder()
    {
        if (body == null || LadderSettings == null) return;
        body.velocity = new Vector2(body.velocity.x, LadderSettings.ladderJumpForce);
    }
    public void SetGravityScale(float scale)
    {
        body.gravityScale = scale;
    }
}

