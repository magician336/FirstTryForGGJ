using UnityEngine;

public abstract class Interactable : MonoBehaviour, IInteractable
{
    public abstract void TriggerInteract();
    
    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f); // 可视化交互范围
    }
}