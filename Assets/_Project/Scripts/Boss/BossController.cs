using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(HealthController))]
public class BossController : MonoBehaviour
{
    [Header("设置")]
    public BossSettings settings;
    
    [Header("组件引用")]
    [SerializeField] private BossFist fist;

    public BossStateMachine StateMachine { get; private set; }
    
    public Rigidbody2D Rb { get; private set; }
    public HealthController Health { get; private set; }
    public Transform PlayerTransform => GameManager.Instance.Player.transform;

    private void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        Health = GetComponent<HealthController>();
        StateMachine = new BossStateMachine();
    }

    private void Start()
    {
        if (fist != null)
        {
            fist.Initialize(this);
        }

        // 初始状态可以在此处初始化，例如进入追逐状态
        StateMachine.Initialize(new BossChaseState(this));
        Health.OnDie += HandleDeath;
    }

    private void Update() => StateMachine.CurrentState.LogicUpdate();
    private void FixedUpdate() => StateMachine.CurrentState.PhysicsUpdate();

    private void HandleDeath()
    {
        Debug.Log("Boss 已被击败!");
        Destroy(gameObject);
    }
}