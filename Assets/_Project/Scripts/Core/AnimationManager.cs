using UnityEngine;

[DefaultExecutionOrder(-80)]
[DisallowMultipleComponent]
public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance { get; private set; }

    [Header("默认玩家 Animator 参数")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string groundedParam = "Grounded";
    [SerializeField] private string jumpTrigger = "Jump";
    [SerializeField] private string interactTrigger = "Interact";
    [SerializeField] private string pauseParam = "Paused";

    private Animator _playerAnimator;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerSpawned += HandlePlayerSpawned;
            GameManager.Instance.OnGamePaused += HandleGamePaused;
            GameManager.Instance.OnGameResumed += HandleGameResumed;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerSpawned -= HandlePlayerSpawned;
            GameManager.Instance.OnGamePaused -= HandleGamePaused;
            GameManager.Instance.OnGameResumed -= HandleGameResumed;
        }
    }

    private void HandlePlayerSpawned(PlayerController player)
    {
        _playerAnimator = player != null ? player.GetComponentInChildren<Animator>() : null;
    }

    private void HandleGamePaused()
    {
        SetBool(pauseParam, true);
    }

    private void HandleGameResumed()
    {
        SetBool(pauseParam, false);
    }

    public void SetFloat(string param, float value)
    {
        if (_playerAnimator == null || string.IsNullOrEmpty(param)) return;
        _playerAnimator.SetFloat(param, value);
    }

    public void SetBool(string param, bool value)
    {
        if (_playerAnimator == null || string.IsNullOrEmpty(param)) return;
        _playerAnimator.SetBool(param, value);
    }

    public void Trigger(string triggerName)
    {
        if (_playerAnimator == null || string.IsNullOrEmpty(triggerName)) return;
        _playerAnimator.SetTrigger(triggerName);
    }

    // 供状态机调用的便捷方法（可选）
    public void PlayJump() => Trigger(jumpTrigger);
    public void PlayInteract() => Trigger(interactTrigger);
}