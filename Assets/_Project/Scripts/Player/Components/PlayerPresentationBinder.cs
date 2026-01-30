using UnityEngine;

public class PlayerPresentationBinder : MonoBehaviour
{
    [Header("Renderer References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    [Header("Prefab Overrides")]
    [SerializeField] private Transform prefabAnchor;

    [Header("Effects")]
    [SerializeField] private Transform vfxAnchor;

    private GameObject currentPrefabInstance;

    void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    public void ApplyPresentation(PlayerFormPresentation presentation)
    {
        if (presentation == null)
        {
            ClearPrefabInstance();
            return;
        }

        ApplySprite(presentation);
        ApplyAnimator(presentation);
        ApplyPrefabOverride(presentation);
        PlaySwitchEffects(presentation);
    }

    private void ApplySprite(PlayerFormPresentation presentation)
    {
        if (spriteRenderer != null && presentation.bodySprite != null)
        {
            spriteRenderer.sprite = presentation.bodySprite;
        }
    }

    private void ApplyAnimator(PlayerFormPresentation presentation)
    {
        if (animator != null && presentation.animatorOverride != null)
        {
            animator.runtimeAnimatorController = presentation.animatorOverride;
        }
    }

    private void ApplyPrefabOverride(PlayerFormPresentation presentation)
    {
        if (prefabAnchor == null)
        {
            return;
        }

        if (presentation.prefabOverride == null)
        {
            ClearPrefabInstance();
            return;
        }

        if (currentPrefabInstance != null && currentPrefabInstance.name.Contains(presentation.prefabOverride.name))
        {
            currentPrefabInstance.transform.localPosition = presentation.prefabOffset;
            return;
        }

        ClearPrefabInstance();
        currentPrefabInstance = Instantiate(presentation.prefabOverride, prefabAnchor);
        currentPrefabInstance.transform.localPosition = presentation.prefabOffset;
        currentPrefabInstance.transform.localRotation = Quaternion.identity;
        currentPrefabInstance.transform.localScale = Vector3.one;
    }

    private void ClearPrefabInstance()
    {
        if (currentPrefabInstance != null)
        {
            Destroy(currentPrefabInstance);
            currentPrefabInstance = null;
        }
    }

    private void PlaySwitchEffects(PlayerFormPresentation presentation)
    {
        if (!string.IsNullOrWhiteSpace(presentation.switchSfxKey))
        {
            AudioManager.Instance?.PlaySFXByKey(presentation.switchSfxKey);
        }

        if (presentation.switchVfxPrefab != null)
        {
            var anchor = vfxAnchor != null ? vfxAnchor : transform;
            var vfxInstance = Instantiate(presentation.switchVfxPrefab, anchor.position + presentation.vfxOffset, Quaternion.identity);
            Destroy(vfxInstance, 3f);
        }
    }
}
