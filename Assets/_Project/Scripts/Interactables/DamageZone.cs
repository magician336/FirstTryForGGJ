using UnityEngine;

/// <summary>
/// 通用伤害区域。玩家接触后扣血。
/// 适用于：尖刺陷阱、火焰、毒雾、敌人攻击判定等。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DamageZone : MonoBehaviour
{
    [Header("伤害配置")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private bool destroyOnHit = false; // 是否造成伤害后销毁自身（如子弹、一次性陷阱）

    [Header("表现")]
    [SerializeField] private GameObject hitVfxPrefab;
    [SerializeField] private AudioClip hitSfx;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 0.7f;

    private Collider2D cachedCollider;

    private void Awake()
    {
        cachedCollider = GetComponent<Collider2D>();
        if (cachedCollider != null)
        {
            cachedCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var health = other.GetComponent<HealthController>();
        if (health != null)
        {
            health.TakeDamage(damageAmount);
            PlayFeedback(other.transform.position);

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }

    private void PlayFeedback(Vector3 position)
    {
        if (hitVfxPrefab != null)
        {
            Instantiate(hitVfxPrefab, position, Quaternion.identity);
        }

        if (hitSfx != null)
        {
            AudioSource.PlayClipAtPoint(hitSfx, position, sfxVolume);
        }
    }
}
