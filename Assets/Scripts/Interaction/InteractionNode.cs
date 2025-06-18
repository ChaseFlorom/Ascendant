using UnityEngine;

public abstract class InteractionNode : ScriptableObject
{
    [HideInInspector]
    public string guid = System.Guid.NewGuid().ToString();
    [HideInInspector]
    public Vector2 editorPosition;

    [Tooltip("The next node in the interaction chain.")]
    public InteractionNode nextNode;

    // Returns the next node to execute, or null if finished
    public abstract InteractionNode Execute();
} 