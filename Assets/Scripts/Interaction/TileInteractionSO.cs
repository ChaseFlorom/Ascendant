using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Interaction/Tile Interaction")]
public class TileInteractionSO : ScriptableObject
{
    [Header("Tile Reference")]
    public TileBase tile;
    
    [Header("Interaction")]
    public ActionGraph actionGraph;
    
    [Header("Metadata")]
    [TextArea(2, 5)]
    public string description;
    public string tileName;
    
    [Header("Conditions")]
    public bool requiresItem;
    public string requiredItemName;
    public bool isOneTimeUse;
    public bool hasBeenUsed;
    
    public bool CanInteract()
    {
        if (isOneTimeUse && hasBeenUsed)
            return false;
            
        // Add more conditions here (item checks, etc.)
        return true;
    }
    
    public void MarkAsUsed()
    {
        if (isOneTimeUse)
            hasBeenUsed = true;
    }
} 