using UnityEngine;

[CreateAssetMenu(menuName = "Interaction/Debug Log Node")]
public class DebugLogNode : InteractionNode
{
    public string message = "Debug!";
    public override InteractionNode Execute()
    {
        Debug.Log($"[ActionGraph] {message}");
        return nextNode;
    }
} 