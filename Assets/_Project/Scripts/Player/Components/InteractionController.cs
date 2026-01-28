using UnityEngine;

public class InteractionController : MonoBehaviour
{
    [Header("交互设置")]
    public KeyCode interactKey = KeyCode.E;
    public float interactRange = 1.5f;
    public LayerMask interactLayer;

    public bool TryInteract()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRange, interactLayer);
        if (hit == null)
        {
            return false;
        }

        IInteractable target = hit.GetComponent<IInteractable>();
        if (target == null)
        {
            return false;
        }

        target.TriggerInteract();
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}