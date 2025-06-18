using UnityEngine;

public class InteractableTile : MonoBehaviour, IInteractable
{
    public InteractionNode rootNode;
    private InteractionNode currentNode;

    public void Interact()
    {
        if (rootNode == null) return;
        currentNode = rootNode;
        while (currentNode != null)
        {
            currentNode = currentNode.Execute();
        }
    }
} 