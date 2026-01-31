using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CheckPoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && GameManager.Instance != null)
        {
            Vector3 respawnPos = transform.position;
            Debug.Log($"[CheckPoint] 设置重生点: {respawnPos}");
            GameManager.Instance.SetRespawnPoint(respawnPos);
            // TODO: Play effect/sound
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}
