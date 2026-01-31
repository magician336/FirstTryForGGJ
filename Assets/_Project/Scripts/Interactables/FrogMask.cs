using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FrogMask : Interactable
{
    [Header("解锁配置")]
    [SerializeField] private PlayerFormType targetForm = PlayerFormType.SuperJump;
    [SerializeField] private PlayerFormUnlockSettings unlockSettings;
    [SerializeField] private bool switchImmediately = true;
    [SerializeField] private bool disableAfterUnlock = true;

    [Header("表现")]
    [SerializeField] private GameObject unlockVfxPrefab;
    [SerializeField] private Vector3 vfxOffset = Vector3.zero;
    [SerializeField] private AudioClip unlockSfx;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

    private bool consumed;
    private Collider2D cachedCollider;

    void Awake()
    {
        cachedCollider = GetComponent<Collider2D>();
        if (cachedCollider != null)
        {
            cachedCollider.isTrigger = true;
        }
    }

    public override void TriggerInteract()
    {
        if (consumed) return;

        var player = GameManager.Instance?.Player;
        if (player == null)
        {
            Debug.LogWarning("[FrogMask] 未找到玩家实例，无法执行 TriggerInteract", this);
            return;
        }

        Debug.Log($"<color=yellow>[FrogMask] 执行解锁逻辑: {targetForm}</color>");

        // 统一使用 PlayerController 的解锁逻辑，不再依赖本地的 unlockSettings 引用
        if (!player.CheckFormUnlocked(targetForm))
        {
            player.ForceUnlockForm(targetForm);
        }

        if (switchImmediately)
        {
            player.SwitchForm(targetForm, true);
        }

        consumed = true;
        PlayFeedback();

        if (disableAfterUnlock)
        {
            DisablePickup();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed) return;

        // 检测触碰的是否是玩家
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            Debug.Log($"[FrogMask] 触碰检测成功，自动解锁: {targetForm}");
            TriggerInteract();
        }
    }

    private void PlayFeedback()
    {
        if (unlockVfxPrefab != null)
        {
            Instantiate(unlockVfxPrefab, transform.position + vfxOffset, Quaternion.identity);
        }

        if (unlockSfx != null)
        {
            AudioSource.PlayClipAtPoint(unlockSfx, transform.position, sfxVolume);
        }
    }

    private void DisablePickup()
    {
        if (cachedCollider != null)
        {
            cachedCollider.enabled = false;
        }

        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
    }
}