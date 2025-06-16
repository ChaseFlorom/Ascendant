using UnityEngine;
using UnityEngine.Tilemaps;

public class MapEngine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerCharacterPrefab;
    [SerializeField] private Grid mapGrid;

    [Header("Player Spawn Grid Position")]
    [SerializeField] private int spawnGridX = 0;
    [SerializeField] private int spawnGridY = 0;
    [SerializeField] public float xOffset = 0f;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap solidsTilemap;
    [SerializeField] private Tilemap highlightTilemap;

    private GameObject playerInstance;

    private void Start()
    {
        if (playerCharacterPrefab == null || mapGrid == null)
        {
            Debug.LogError("MapEngine: Missing references to prefab or grid.");
            return;
        }

        Vector3Int cellPos = new Vector3Int(spawnGridX, spawnGridY, 0);
        Vector3 worldPos = mapGrid.CellToWorld(cellPos);
        worldPos.x += xOffset;

        playerInstance = Instantiate(playerCharacterPrefab, worldPos, Quaternion.identity);

        // Assign tilemaps to PlayerClickToMove
        var clickToMove = playerInstance.GetComponent<PlayerClickToMove>();
        if (clickToMove != null)
        {
            clickToMove.floorTilemap = floorTilemap;
            clickToMove.solidsTilemap = solidsTilemap;
            clickToMove.highlightTilemap = highlightTilemap;
        }
    }
} 