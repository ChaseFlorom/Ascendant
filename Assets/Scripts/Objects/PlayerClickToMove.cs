using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class PlayerClickToMove : MonoBehaviour
{
    [Header("Tilemaps and Tiles")]
    public Tilemap floorTilemap;
    public Tilemap solidsTilemap;
    public Tilemap highlightTilemap;
    public Tile highlightTile;
    public Tile hoverTile;

    [Header("Movement Settings")]
    public int movementRange = 5;
    public float moveSpeed = 3f;
    public float xOffset = 0.5f;

    [Header("Animator Controller")]
    public CharacterAnimatorController characterAnimController;

    private bool isSelected;
    private List<Vector3Int> currentReachableTiles = new List<Vector3Int>();
    private Vector3Int lastHoverCell = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
    private TileBase lastHoverOriginalTile;

    void Update()
    {
        HandleLeftClick();
        HandleHoverTile();
    }

    void HandleLeftClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickPos.z = 0f;

            Collider2D hit = Physics2D.OverlapPoint(clickPos);
            if (hit != null && hit.transform.IsChildOf(transform))
            {
                // Clicked on the player
                if (!isSelected)
                {
                    isSelected = true;

                    // Recompute our BFS range from where we actually stand
                    Vector3Int startCell = GetCurrentCell();
                    currentReachableTiles = GetReachableTiles(startCell, movementRange);

                    // Show all reachable tiles
                    HighlightTiles(currentReachableTiles);
                }
            }
            else
            {
                // Clicked somewhere else
                if (isSelected)
                {
                    Vector3Int clickedCell = floorTilemap.WorldToCell(clickPos);
                    TileBase tileInCell = highlightTilemap.GetTile(clickedCell);

                    // If we clicked a highlighted cell (or hover cell), we move there
                    if (tileInCell == highlightTile || tileInCell == hoverTile)
                    {
                        // Stop showing the indicators and begin movement
                        isSelected = false;
                        ClearHoverData();
                        ClearHighlights();
                        StartCoroutine(MovePlayerRoutine(clickedCell));
                    }
                    else
                    {
                        // Clicked outside highlighted area, deselect
                        isSelected = false;
                        ClearHoverData();
                        ClearHighlights();
                    }
                }
            }
        }
    }

    void HandleHoverTile()
    {
        // Only show a hover tile if we've selected the player
        if (!isSelected) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3Int hoverCell = floorTilemap.WorldToCell(mousePos);

        // If we haven't changed hovered cells, do nothing
        if (hoverCell == lastHoverCell) return;

        // Restore the previous hover cell
        if (lastHoverCell.x != int.MaxValue)
        {
            highlightTilemap.SetTile(lastHoverCell, lastHoverOriginalTile);
        }

        // If the new cell is highlighted, temporarily swap in the hover tile
        TileBase existing = highlightTilemap.GetTile(hoverCell);
        if (existing == highlightTile)
        {
            lastHoverOriginalTile = existing;
            highlightTilemap.SetTile(hoverCell, hoverTile);
            lastHoverCell = hoverCell;
        }
        else
        {
            lastHoverCell = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
            lastHoverOriginalTile = null;
        }
    }

    IEnumerator MovePlayerRoutine(Vector3Int targetCell)
    {
        // Find the BFS path from current cell to target
        Vector3Int startCell = GetCurrentCell();
        List<Vector3Int> path = FindPath(startCell, targetCell);

        if (path == null || path.Count < 2)
        {
            yield break; // No valid path or trivial path
        }

        // Step through the path one cell at a time
        for (int i = 1; i < path.Count; i++)
        {
            Vector3Int fromCell = path[i - 1];
            Vector3Int toCell = path[i];

            Vector3 fromPos = CellToWorldSnapped(fromCell);
            Vector3 toPos = CellToWorldSnapped(toCell);

            // Determine our movement direction this step
            Vector3 direction = toPos - fromPos;
            ActivateRig(direction);

            if (characterAnimController)
            {
                characterAnimController.SetBool("isWalking", true);
            }

            float distance = Vector3.Distance(fromPos, toPos);
            float t = 0f;
            while (t < 1f)
            {
                t += (moveSpeed * Time.deltaTime) / distance;
                transform.position = Vector3.Lerp(fromPos, toPos, t);
                yield return null;
            }

            // Snap to the final position for this step
            transform.position = toPos;

            if (characterAnimController)
            {
                characterAnimController.SetBool("isWalking", false);
            }
        }

        // The movement is complete, but we don't need to set isSelected = true
        // since that will happen automatically when the player clicks on the character again
    }

    void ActivateRig(Vector3 dir)
    {
        if (!characterAnimController) return;

        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);

        // If moving horizontally more than vertically...
        // We'll say (dir.x > 0) => facingLeft = true to "reverse" the logic
        if (absX > absY)
        {
            bool facingLeft = (dir.x > 0);
            characterAnimController.ActivateSide(facingLeft);
        }
        else
        {
            if (dir.y > 0) characterAnimController.ActivateUp();
            else characterAnimController.ActivateDown();
        }
    }

    Vector3Int GetCurrentCell()
    {
        // We subtract xOffset to align transform.position with the cell's center
        Vector3 pos = transform.position;
        pos.x -= xOffset;

        // Convert to cell coordinates
        Vector3Int cell = floorTilemap.WorldToCell(pos);
        return cell;
    }

    Vector3 CellToWorldSnapped(Vector3Int cell)
    {
        // Convert to world space, then add xOffset to stand at cell center
        Vector3 wPos = floorTilemap.CellToWorld(cell);
        wPos.x += xOffset;
        return wPos;
    }

    List<Vector3Int> GetReachableTiles(Vector3Int startCell, int range)
    {
        // BFS just for "which tiles are in range" (highlight)
        List<Vector3Int> results = new List<Vector3Int>();
        Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        Dictionary<Vector3Int, int> distanceMap = new Dictionary<Vector3Int, int>();

        frontier.Enqueue(startCell);
        distanceMap[startCell] = 0;

        Vector3Int[] dirs = {
            new Vector3Int( 1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int( 0, 1, 0),
            new Vector3Int( 0,-1, 0)
        };

        while (frontier.Count > 0)
        {
            Vector3Int current = frontier.Dequeue();
            int d = distanceMap[current];

            if (d <= range)
            {
                results.Add(current);
            }

            if (d < range)
            {
                foreach (var dir in dirs)
                {
                    Vector3Int neighbor = current + dir;
                    if (!distanceMap.ContainsKey(neighbor) && IsCellPassable(neighbor))
                    {
                        frontier.Enqueue(neighbor);
                        distanceMap[neighbor] = d + 1;
                    }
                }
            }
        }
        return results;
    }

    List<Vector3Int> FindPath(Vector3Int startCell, Vector3Int endCell)
    {
        // BFS to find an actual path from startCell -> endCell
        if (!IsCellPassable(startCell) || !IsCellPassable(endCell))
            return null;

        Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> parent = new Dictionary<Vector3Int, Vector3Int>();

        frontier.Enqueue(startCell);
        parent[startCell] = startCell;

        Vector3Int[] dirs = {
            new Vector3Int( 1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int( 0, 1, 0),
            new Vector3Int( 0,-1, 0)
        };

        bool found = false;
        while (frontier.Count > 0 && !found)
        {
            Vector3Int current = frontier.Dequeue();
            if (current == endCell)
            {
                found = true;
                break;
            }
            foreach (var d in dirs)
            {
                Vector3Int neighbor = current + d;
                if (!parent.ContainsKey(neighbor) && IsCellPassable(neighbor))
                {
                    parent[neighbor] = current;
                    frontier.Enqueue(neighbor);
                }
            }
        }

        if (!found) return null;

        // Reconstruct path from endCell back to startCell
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int tmp = endCell;
        while (tmp != startCell)
        {
            path.Add(tmp);
            tmp = parent[tmp];
        }
        path.Add(startCell);
        path.Reverse();
        return path;
    }

    bool IsCellPassable(Vector3Int cellPos)
    {
        // A tile is passable if floorTilemap has a tile and solidsTilemap does NOT
        TileBase floor = floorTilemap.GetTile(cellPos);
        TileBase solid = solidsTilemap.GetTile(cellPos);
        if (floor == null) return false;
        if (solid != null) return false;
        return true;
    }

    void HighlightTiles(List<Vector3Int> cells)
    {
        highlightTilemap.ClearAllTiles();
        foreach (var c in cells)
        {
            highlightTilemap.SetTile(c, highlightTile);
        }
    }

    void ClearHighlights()
    {
        highlightTilemap.ClearAllTiles();
    }

    void ClearHoverData()
    {
        if (lastHoverCell.x != int.MaxValue)
        {
            highlightTilemap.SetTile(lastHoverCell, lastHoverOriginalTile);
        }
        lastHoverCell = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
        lastHoverOriginalTile = null;
    }
}