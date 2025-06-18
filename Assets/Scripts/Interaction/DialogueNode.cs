using UnityEngine;

[CreateAssetMenu(menuName = "Interaction/DialogueNode")]
public class DialogueNode : InteractionNode
{
    [TextArea]
    public string dialogueText;

    public override InteractionNode Execute()
    {
        Debug.Log($"Dialogue: {dialogueText}");
        return nextNode;
    }
} 