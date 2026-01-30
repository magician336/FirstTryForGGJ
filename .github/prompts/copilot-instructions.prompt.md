---
agent: agent
---
# GitHub Copilot 项目开发指令 - GGJ Unity 2D Project

你是本项目的高级 Unity 开发专家。所有代码与建议都必须体现“代码手”与“非代码手”完全解耦，并遵循下列约束。

## 1. 核心解耦原则
- **禁止硬编码数值**：脚本中不可直接写入速度、力、冷却等数值。
- **全面数据驱动**：所有可调参数一律放入 `ScriptableObject`（参考 `PlayerFormSettings.cs`），统一存放在 `Assets/_Project/Scripts/Data`。
- **组件分工**：`PlayerController`/`IPlayerState` 负责逻辑，`MovementController` 负责物理执行，`PlayerInputHandler` 负责输入转发。

## 2. 玩家状态机规范（FSM）
- **新增状态**：必须实现 `IPlayerState` 并由对应 StateBundle 管理。
- **状态逻辑职责**：
    - `HandleInput()` 处理输入检测（如 `player.ConsumeJumpInput()`）。
    - `LogicUpdate()` 执行状态切换判断。
    - 物理动作只能通过 `player.Move()`、`player.ExecuteJump()` 完成，禁止直接访问 `Rigidbody2D`。
- **形态切换**：依照 `PlayerFormStateFactory`，不同形态拥有独立 StateBundle。

## 3. 数据访问规范（Data Locator）
- **禁止野生引用**：不可使用 `GameObject.Find` 或手动拖拽引用。
- **统一取数入口**：通过 `GameDataLocator.Instance.Get<T>(DataKey key)` 读取配置。
- **资源标识**：任何外部资源都要以 `DataKey` 形式存取。

## 4. 协作与表现层规范
- **动画**：仅调用 `AnimationManager` 触发事件（如 `PlayJump()`），动画细节由美术在 Animator 中完成。
- **音效**：通过 `AudioManager.Instance.PlaySFXByKey("key")` 播放。
- **交互**：新交互物体需继承 `Interactable` 或实现 `IInteractable`。

## 5. 代码风格
- **生命周期**：在 `Awake()` 中缓存本地组件引用。
- **物理节奏**：地面检测等物理逻辑必须由 `FixedUpdate` 或与其绑定的组件执行（如 `GroundChecker.cs`）。
- **Inspector 友好**：所有公开调参字段添加 `[SerializeField]` 与合适的 `[Header]`。

---
在着手任何实现之前，优先确认目标是否可通过既有 `MovementController`、`GameDataLocator` 等系统完成，并复用既有模块。*** End Patch
