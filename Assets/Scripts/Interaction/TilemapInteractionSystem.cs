using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TilemapInteractionSystem : MonoBehaviour
{
    [Header("Tilemap Settings")]
    public Tilemap[] tilemapLayers; // Array of tilemap layers, ordered by priority (top to bottom)
    public Transform playerTransform;
    public ExplorationMovement explorationMovement; // Reference to movement script
    public KeyCode interactKey = KeyCode.Return;

    private const float POSITION_EPSILON = 0.05f;

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteractWithAdjacentPoint();
        }
    }

    void TryInteractWithAdjacentPoint()
    {
        if (explorationMovement == null)
        {
            Debug.LogWarning("TilemapInteractionSystem: ExplorationMovement reference not set!");
            return;
        }
        Vector3Int playerTilePos = explorationMovement.CurrentCell;
        Vector2Int facingDirection = explorationMovement.CurrentFacingDirection;

        // Check all four adjacent tiles
        Vector3Int[] adjacentPositions = {
            playerTilePos + Vector3Int.up,    // Above
            playerTilePos + Vector3Int.down,  // Below
            playerTilePos + Vector3Int.left,  // Left
            playerTilePos + Vector3Int.right  // Right
        };

        foreach (var adjacentPos in adjacentPositions)
        {
            // Check if this position is in the direction the player is facing
            Vector3Int directionToTile = adjacentPos - playerTilePos;
            if (directionToTile.x == facingDirection.x && directionToTile.y == facingDirection.y)
            {
                // Find all InteractionPoints in the scene
                var allPoints = GameObject.FindObjectsByType<InteractionPoint>(FindObjectsSortMode.None);
                foreach (var point in allPoints)
                {
                    Vector3Int pointCell = tilemapLayers[0].WorldToCell(point.transform.position);
                    if (pointCell == adjacentPos)
                    {
                        if (point.CanInteract())
                        {
                            point.Interact();
                            return;
                        }
                    }
                }
            }
        }
    }
}

[System.Serializable]
public class TileInteractionData
{
    public TileBase tile;
    public InteractionNode rootNode;
    [TextArea(2, 5)]
    public string description; // For editor organization
} 