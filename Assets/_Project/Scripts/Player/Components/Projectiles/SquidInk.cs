using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SquidInk : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private Vector2 defaultDirection = Vector2.right;

    private Rigidbody2D rb;
    private float spawnTime;
    private Vector2 currentDirection;
    private float travelSpeed = 1f;
    private float lifetimeSeconds = 2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        spawnTime = Time.time;
        currentDirection = defaultDirection != Vector2.zero ? defaultDirection.normalized : Vector2.right;
        ApplyVelocity();
    }

    void Update()
    {
        if (Time.time - spawnTime >= lifetimeSeconds)
        {
            Destroy(gameObject);
        }
    }

    public void Configure(float speed, float lifetime)
    {
        travelSpeed = Mathf.Max(0.01f, speed);
        lifetimeSeconds = Mathf.Max(0.1f, lifetime);
        ApplyVelocity();
    }

    public void InitializeFrom(FishFormSettings settings, bool faceRight)
    {
        if (settings == null)
        {
            return;
        }

        Configure(settings.inkSpeed, settings.inkLifetime);
        Flip(faceRight);
    }

    public void Fire(Vector2 worldDirection)
    {
        currentDirection = worldDirection.sqrMagnitude > 0.0001f ? worldDirection.normalized : Vector2.right;
        ApplyVelocity();
    }

    public void Flip(bool faceRight)
    {
        currentDirection = faceRight ? Vector2.right : Vector2.left;
        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        if (rb == null)
        {
            return;
        }

        rb.velocity = currentDirection * travelSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 遇到碰撞体销毁，可以根据需要排除触发器或特定层级
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
