using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SkinPickup : Interactable
{
    [Header("皮肤解锁配置")]
    [SerializeField] private PlayerFormType targetForm;
    [SerializeField] private int skinIndex;
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
        if (player == null) return;

        Debug.Log($"<color=cyan>[SkinPickup] 触发解锁: 形态 {targetForm}, 索引 {skinIndex}</color>");

        // 1. 解锁皮肤
        player.ForceUnlockSkin(targetForm, skinIndex);

        // 2. 如果需要，立即切换
        if (switchImmediately)
        {
            // 如果形态不匹配，先换形态，再换脸挂皮
            // 但目前的系统 SwitchForm 会重置 currentSkinIndex 吗？
            // 我们需要确保 PlayerController 有一个能同时指定形态和皮肤的方法
            player.SwitchFormWithSkin(targetForm, skinIndex);
        }

        consumed = true;
        PlayFeedback();

        if (disableAfterUnlock)
        {
            DisablePickup();
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
        if (cachedCollider != null) cachedCollider.enabled = false;
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
    }

    protected override void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }
}
