using UnityEngine;

[CreateAssetMenu(menuName = "Interaction/Test Dialogue Node")]
public class TestDialogueNode : InteractionNode
{
    [TextArea(3, 6)]
    public string message = "Hello! This is a test interaction!";

    public override InteractionNode Execute()
    {
        Debug.Log($"[INTERACTION] {message}");
        return nextNode;
    }
} 