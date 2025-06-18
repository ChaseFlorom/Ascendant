using UnityEngine;

[CreateAssetMenu(menuName = "Interaction/Start Node")]
public class StartNode : InteractionNode
{
    public override InteractionNode Execute()
    {
        return nextNode;
    }
} 