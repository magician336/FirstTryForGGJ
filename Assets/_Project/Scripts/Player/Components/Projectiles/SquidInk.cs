using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SquidInk : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField][Min(0.01f)] private float travelSpeed = 1f;
    [SerializeField][Min(0.1f)] private float lifetimeSeconds = 2f;
    [SerializeField] private Vector2 defaultDirection = Vector2.right;

    private Rigidbody2D rb;
    private float spawnTime;
    private Vector2 currentDirection;

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
}
