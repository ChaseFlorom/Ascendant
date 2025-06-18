using UnityEngine;

[CreateAssetMenu(menuName = "Interaction/Action Graph")]
public class ActionGraph : ScriptableObject
{
    public InteractionNode startNode;

    public void Run()
    {
        InteractionNode currentNode = startNode;
        while (currentNode != null)
        {
            currentNode = currentNode.Execute();
        }
    }
} 