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

    [SerializeField] private WaterDetector waterDetector;

    // Public accessor for settings
    public PlayerSettings Settings => playerSettings;

    private PlayerStateMachine stateMachine;
    private Rigidbody2D body;
    private PlayerInputHandler cachedInputHandler;
    private float baseGravityScale = 1f;

    private readonly Dictionary<PlayerFormType, PlayerFormStateBundle> formBundles = new();
    private PlayerFormType currentFormType = PlayerFormType.NormalHead;
    private PlayerFormStateFactory currentFormFactory;
    private SuperJumpFormSettings SuperJumpSettings => playerSettings != null ? playerSettings.GetFormSettings(PlayerFormType.SuperJump, currentSkinIndex) as SuperJumpFormSettings : null;
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
    private float nextInkFireTime;
    private int currentSkinIndex = 0;

    // [新增] 溺水逻辑变量
    private bool isDrowning = false;
    private float drowningTimer = 0f;
    private const float DROWNING_TIME_LIMIT = 1.0f; // 1秒后死亡

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
        // 获取组件（如果没有在 Inspector 赋值）
        if (waterDetector == null) waterDetector = GetComponentInChildren<WaterDetector>();

        // [新增] 订阅进出水域的事件，处理重力和状态切换
        if (waterDetector != null)
        {
            waterDetector.OnWaterEnter += HandleWaterEnter;
            waterDetector.OnWaterExit += HandleWaterExit;
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


        // [新增] 溺水核心逻辑
        if (isDrowning)
        {
            HandleDrowningLogic();

            // 如果正在溺水，强制不执行状态机的 Input 处理 (防止跳跃/攻击)
            // 但允许 LogicUpdate 运行以保持动画状态 (如 Idle)
            stateMachine?.CurrentState?.LogicUpdate();
            return;
        }

        if (stateMachine?.CurrentState == null)
        {
            return;
        }

        stateMachine.CurrentState.HandleInput();
        stateMachine.CurrentState.LogicUpdate();
    }

    // [新增] 专门处理溺水时的状态锁定和处死
    private void HandleDrowningLogic()
    {
        // 1. 计时
        drowningTimer += Time.deltaTime;

        // 2. 强制锁定输入和移动 (禁止移动)
        movementInput = 0f;
        verticalInput = 0f;

        // 3. 强制锁定物理速度 (防止惯性滑动)
        if (body != null)
        {
            body.velocity = Vector2.zero;
        }

        // 4. 检查是否死亡
        if (drowningTimer >= DROWNING_TIME_LIMIT)
        {
            PerformDrowningDeath();
        }
    }

    private void PerformDrowningDeath()
    {
        Debug.Log("溺水时间到，玩家死亡。");
        isDrowning = false; // 避免重复调用
        drowningTimer = 0f;

        if (healthController != null)
        {
            // 直接造成最大生命值的伤害以确保死亡
            healthController.TakeDamage(9999);
        }
        else
        {
            // 如果没有血量组件，直接切换到死亡状态
            ChangeState(deadState);
        }
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
            cachedInputHandler.fireKey = GetFireKey();
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

        if (!force && !IsFormUnlocked(newForm))
        {
            Debug.LogWarning($"Attempted to switch to locked form {newForm}");
            return;
        }

        // 切换形态时，如果当前皮肤索引不在新形态的已解锁列表中，则重置为 0
        if (!IsSkinUnlocked(newForm, currentSkinIndex))
        {
            currentSkinIndex = 0;
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

        // [新增] 确保复活时清除溺水标记
        isDrowning = false;
        drowningTimer = 0f;
        SetGravityScale(baseGravityScale); // 恢复重力，防止复活后飘在天上

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

        if (Input.GetKeyDown(GetFireKey()))
        {
            OnFireButtonDown();
        }

        if (Input.GetKeyDown(GetSkinKey()))
        {
            CycleSkin();
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
        if (waterDetector == null) return;

        // 获取当前的水层级配置
        var fishSettings = FishSettings;
        LayerMask targetLayer = fishSettings != null ? fishSettings.WaterLayer : (LayerMask)0;

        // 驱动检测逻辑
        waterDetector.Detect(targetLayer);
    }

    private void HandleWaterEnter()
    {
        // 1. 如果是鱼形态：执行原有游泳逻辑
        if (currentFormType == PlayerFormType.Fish)
        {
            isDrowning = false;
            SetGravityScale(0f); // 关闭重力

            if (CanSwim)
            {
                Debug.Log("[Player] 鱼形态进入水域，切换至游泳状态");
                ChangeState(swimIdleState);
            }
        }
        // 2. 其他形态：执行溺水逻辑
        else
        {
            Debug.Log($"[Player] 非鱼形态 ({currentFormType}) 落水！无法移动，{DROWNING_TIME_LIMIT}秒后死亡...");

            isDrowning = true;
            drowningTimer = 0f;

            // 选择 A: 悬浮在水中等死 (重力设为 0)
            SetGravityScale(0f);

            // 立即停止物理惯性
            if (body != null) body.velocity = Vector2.zero;
        }
    }

    private void HandleWaterExit()
    {
        // 恢复重力
        SetGravityScale(baseGravityScale);

        // [新增] 重置溺水状态
        isDrowning = false;
        drowningTimer = 0f;
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

    public void OnFireButtonDown()
    {
        Debug.Log($"[PlayerController] OnFireButtonDown called. Current Form: {currentFormType}");
        if (currentFormType == PlayerFormType.Fish)
        {
            FireSquidInk();
        }
    }

    private void FireSquidInk()
    {
        var settings = FishSettings;
        if (settings == null)
        {
            Debug.LogError("[PlayerController] FishSettings is NULL!");
            return;
        }

        if (settings.squidInkPrefab == null)
        {
            Debug.LogError("[PlayerController] SquidInkPrefab is NOT assigned in FishSettings asset!");
            return;
        }

        if (Time.time < nextInkFireTime)
        {
            Debug.Log($"[PlayerController] Ink fire on cooldown. Wait {nextInkFireTime - Time.time:F2}s");
            return;
        }

        // 确定发射方向
        bool faceRight = transform.localScale.x > 0;
        Vector2 spawnPos = transform.position;

        Debug.Log($"[PlayerController] Instantiating Ink at {spawnPos}. FaceRight: {faceRight}");
        SquidInk ink = Instantiate(settings.squidInkPrefab, spawnPos, Quaternion.identity);
        if (ink != null)
        {
            ink.InitializeFrom(settings, faceRight);
        }
        else
        {
            Debug.LogError("[PlayerController] Failed to Instantiate SquidInk!");
        }

        nextInkFireTime = Time.time + settings.inkCooldown;
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
    public bool IsInWater => waterDetector != null && waterDetector.IsInWater;
    public bool CanSwim => currentFormType == PlayerFormType.Fish && IsInWater && swimIdleState != null;
    public float HorizontalInput => movementInput;
    public float VerticalInput => verticalInput;
    public float VerticalVelocity => body != null ? body.velocity.y : 0f;

    public PlayerLadderSettings LadderSettings => playerSettings != null ? playerSettings.ladderSettings : null;
    public FishFormSettings FishSettings => playerSettings != null ? playerSettings.GetFormSettings(PlayerFormType.Fish, currentSkinIndex) as FishFormSettings : null;
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

        var formSettings = playerSettings.GetFormSettings(formType, currentSkinIndex);
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
            return 0.1f;
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

        var formSettings = playerSettings.GetFormSettings(currentFormType, currentSkinIndex);
        presentationBinder.ApplyPresentation(formSettings != null ? formSettings.presentation : null);
    }

    private KeyCode GetSkinKey()
    {
        var inputSettings = InputSettingsAsset;
        return inputSettings != null ? inputSettings.SkinKey : KeyCode.R;
    }

    private KeyCode GetFireKey()
    {
        var inputSettings = InputSettingsAsset;
        return inputSettings != null ? inputSettings.FireKey : KeyCode.Mouse0;
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
        Debug.Log("CycleForm");
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
            Debug.LogWarning("[PlayerController] FormUnlockSettings 为空，无法检查解锁状态。");
            return false;
        }

        return FormUnlockSettings.IsFormUnlocked(formType);
    }

    // 将 IsFormUnlocked 暴露给公共访问
    public bool CheckFormUnlocked(PlayerFormType formType) => IsFormUnlocked(formType);

    private List<PlayerFormType> GetUnlockedFormsBuffer()
    {
        unlockedFormBuffer.Clear();

        // 1. 基础形态永远默认解锁并放在首位
        unlockedFormBuffer.Add(PlayerFormType.NormalHead);

        // 2. 从配置中读取其他已解锁形态
        if (FormUnlockSettings != null)
        {
            foreach (var form in FormUnlockSettings.UnlockedForms)
            {
                if (form != PlayerFormType.NormalHead && !unlockedFormBuffer.Contains(form))
                {
                    unlockedFormBuffer.Add(form);
                }
            }
        }

        // 3. 详细日志，方便调试看清具体内容
        if (unlockedFormBuffer.Count > 0)
        {
            string log = "[PlayerController] 当前已解锁形态列表: ";
            foreach (var f in unlockedFormBuffer) log += f.ToString() + ", ";
            Debug.Log(log.TrimEnd(' ', ','));
        }

        return unlockedFormBuffer;
    }

    public void ForceUnlockForm(PlayerFormType formType)
    {
        if (FormUnlockSettings == null)
        {
            Debug.LogError("[PlayerController] 无法解锁形态：PlayerSettings 中未分配 FormUnlockSettings 资源！");
            return;
        }

        Debug.Log($"[PlayerController] 正在修改解锁清单资产: <color=cyan>{FormUnlockSettings.name}</color>");
        FormUnlockSettings.EnsureUnlocked(formType);
        Debug.Log($"<color=green>[PlayerController] 强制解锁形态成功: {formType}</color>");

        // 立即刷新一次缓冲区
        GetUnlockedFormsBuffer();
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
    public void CycleSkin()
    {
        if (playerSettings == null || FormUnlockSettings == null) return;

        int skinCount = playerSettings.GetSkinCount(currentFormType);
        if (skinCount <= 1) return;

        // 寻找下一个已解锁的皮肤索引
        int nextIndex = currentSkinIndex;
        for (int i = 0; i < skinCount; i++)
        {
            nextIndex = (nextIndex + 1) % skinCount;
            if (FormUnlockSettings.IsSkinUnlocked(currentFormType, nextIndex))
            {
                currentSkinIndex = nextIndex;
                break;
            }
        }

        // 重新应用当前形态的设置（包括外貌和物理参数）
        ApplyFormSettings(currentFormType);

        Debug.Log($"[PlayerController] 已切换皮肤索引: {currentSkinIndex}");
    }

    public void ForceUnlockSkin(PlayerFormType formType, int skinIndex)
    {
        if (FormUnlockSettings == null) return;

        // 确保形态也标记为解锁（逻辑兼容）
        FormUnlockSettings.EnsureFormUnlocked(formType);
        FormUnlockSettings.EnsureSkinUnlocked(formType, skinIndex);

        Debug.Log($"<color=green>[PlayerController] 强制解锁皮肤成功: 形态 {formType}, 索引 {skinIndex}</color>");
    }

    public void SwitchFormWithSkin(PlayerFormType formType, int skinIndex, bool force = false)
    {
        currentSkinIndex = skinIndex;
        SwitchForm(formType, force);
        // 如果 SwitchForm 内部因为已经相同形态而跳过，这里手动重新应用一次设置以刷新皮肤
        ApplyFormSettings(formType);
    }

    private bool IsSkinUnlocked(PlayerFormType formType, int skinIndex)
    {
        return FormUnlockSettings != null && FormUnlockSettings.IsSkinUnlocked(formType, skinIndex);
    }
}

