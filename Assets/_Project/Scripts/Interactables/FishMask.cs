using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FishMask : Interactable
{
    [Header("解锁配置")]
    [SerializeField] private PlayerFormType targetForm = PlayerFormType.Fish; // 默认设为鱼形态
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
            Debug.LogError("[FishMask] 未找到 Player 实例！");
            return;
        }

        Debug.Log($"<color=cyan>[FishMask] 成功通过 'F' 键触发！形态: {targetForm}</color>");

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

    protected override void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
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
