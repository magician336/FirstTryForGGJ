using UnityEngine;

public class InteractionController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputSettings inputSettings;

    public KeyCode InteractKey => inputSettings != null ? inputSettings.InteractKey : KeyCode.F;

    public float interactRange = 1.5f;
    public LayerMask interactLayer;

    public void ApplyInputSettings(InputSettings settings)
    {
        inputSettings = settings;
    }

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