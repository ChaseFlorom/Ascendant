using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap floorTilemap;      // Tilemap for "Floor" tiles
    public Tilemap solidsTilemap;     // Tilemap for "Solids" (blocking) tiles
    public Tilemap highlightTilemap;  // (Optional) Tilemap used to display reachable tiles

    [Header("Tiles & Settings")]
    public Tile highlightTile;        // A tile to use for highlighting reachable cells
    public int movementRange = 5;

    private Vector3Int currentCell;

    void Update()
    {
        // Example: Left-click to highlight all possible moves
        if (Input.GetMouseButtonDown(0))
        {
            currentCell = floorTilemap.WorldToCell(transform.position);

            // Clear any old highlight tiles
            ClearHighlights();

            // Get all reachable tiles
            List<Vector3Int> reachableTiles = GetReachableTiles(currentCell, movementRange);

            // Highlight them
            foreach (var cellPos in reachableTiles)
            {
                highlightTilemap.SetTile(cellPos, highlightTile);
            }
        }

        // Example: Right-click to move to a highlighted tile
        if (Input.GetMouseButtonDown(1))
        {
            MoveToClickedHighlight();
        }
    }

    void ClearHighlights()
    {
        highlightTilemap.ClearAllTiles();
    }

    /// <summary>
    /// Returns all tiles within a BFS range that have a Floor tile and are not blocked by a Solid tile.
    /// </summary>
    List<Vector3Int> GetReachableTiles(Vector3Int startCell, int range)
    {
        List<Vector3Int> results = new List<Vector3Int>();
        Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        Dictionary<Vector3Int, int> distanceMap = new Dictionary<Vector3Int, int>();

        frontier.Enqueue(startCell);
        distanceMap[startCell] = 0;

        // 4 directions to explore (N, S, E, W)
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int( 1,  0, 0),
            new Vector3Int(-1,  0, 0),
            new Vector3Int( 0,  1, 0),
            new Vector3Int( 0, -1, 0),
        };

        while (frontier.Count > 0)
        {
            Vector3Int current = frontier.Dequeue();
            int currentDist = distanceMap[current];

            // If within range, we consider this tile reachable
            if (currentDist <= range)
            {
                results.Add(current);
            }

            // If we haven't reached the maximum range, continue exploring neighbors
            if (currentDist < range)
            {
                foreach (var dir in directions)
                {
                    Vector3Int neighbor = current + dir;

                    // Skip if visited
                    if (distanceMap.ContainsKey(neighbor))
                        continue;

                    // Check passability
                    if (IsCellPassable(neighbor))
                    {
                        frontier.Enqueue(neighbor);
                        distanceMap[neighbor] = currentDist + 1;
                    }
                }
            }
        }

        return results;
    }

    /// <summary>
    /// A cell is passable if it has a floor tile and does not have a solid tile.
    /// </summary>
    bool IsCellPassable(Vector3Int cellPos)
    {
        TileBase floorTile = floorTilemap.GetTile(cellPos);
        TileBase solidTile = solidsTilemap.GetTile(cellPos);

        // Must have a floor tile to walk on
        if (floorTile == null) return false;
        // If there's a solid tile on top, it's blocked
        if (solidTile != null) return false;

        return true;
    }

    /// <summary>
    /// Moves the player to a highlighted tile on right-click (if valid).
    /// </summary>
    void MoveToClickedHighlight()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector3Int clickedCell = floorTilemap.WorldToCell(mouseWorldPos);

        // If the highlightTilemap has a tile at this position, it's reachable
        if (highlightTilemap.GetTile(clickedCell) != null)
        {
            // Move the player directly (or run an animation, path, etc.)
            Vector3 targetWorld = floorTilemap.CellToWorld(clickedCell);
            transform.position = targetWorld;

            // Clear highlights if you'd like, so they have to re-click to see new moves
            ClearHighlights();
        }
    }
}
