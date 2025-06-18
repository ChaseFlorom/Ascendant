using UnityEngine;

public enum InteractionType
{
    OnInteraction,
    AutoRun,
    EnteredScene
}

public class InteractionPoint : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    public ActionGraph actionGraph;
    public InteractionType interactionType = InteractionType.OnInteraction;
    
    [Header("Settings")]
    public bool isOneTimeUse = false;
    public bool hasBeenUsed = false;
    
    [Header("Visual")]
    public Color gizmoColor = Color.yellow;
    public float gizmoSize = 0.5f;
    
    public bool CanInteract()
    {
        if (isOneTimeUse && hasBeenUsed)
            return false;
        return true;
    }
    
    public void Interact()
    {
        if (CanInteract() && actionGraph != null)
        {
            actionGraph.Run();
            if (isOneTimeUse)
            {
                hasBeenUsed = true;
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!CanInteract())
        {
            Gizmos.color = Color.gray;
        }
        else
        {
            Gizmos.color = gizmoColor;
        }
        
        Gizmos.DrawWireCube(transform.position, Vector3.one * gizmoSize);
        
        // Draw a small indicator for the interaction point
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one * (gizmoSize + 0.1f));
    }
} 