using UnityEngine;
using UnityEngine.Tilemaps;

public class ExplorationMovement : MonoBehaviour
{
    [Header("References")]
    public Tilemap floorTilemap;
    public Tilemap solidsTilemap;
    public CharacterAnimatorController characterAnimController;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float xOffset = 0.5f;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private Vector3Int currentCell;
    private Vector3 lastDirection;
    private Vector2Int queuedDirection = Vector2Int.zero;

    private void Start()
    {
        targetPosition = transform.position;
        currentCell = floorTilemap.WorldToCell(transform.position);
    }

    private void Update()
    {
        if (GameStateManager.Instance.IsInBattle())
            return;

        // Always record the latest input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        queuedDirection = Vector2Int.zero;
        if (horizontal != 0) queuedDirection.x = (int)Mathf.Sign(horizontal);
        else if (vertical != 0) queuedDirection.y = (int)Mathf.Sign(vertical);

        HandleMovementInput();
        UpdateMovement();
    }

    private void HandleMovementInput()
    {
        if (isMoving)
            return;

        if (queuedDirection != Vector2Int.zero)
        {
            Vector3Int newCell = currentCell + new Vector3Int(queuedDirection.x, queuedDirection.y, 0);
            if (IsValidMove(newCell))
            {
                currentCell = newCell;
                targetPosition = CellToWorldSnapped(newCell);
                isMoving = true;

                // Set animation direction
                lastDirection = targetPosition - transform.position;
                ActivateRig(lastDirection);

                // Set isWalking to true only when movement starts
                if (characterAnimController)
                    characterAnimController.SetBool("isWalking", true);
            }
        }
    }

    private void UpdateMovement()
    {
        if (!isMoving)
            return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            isMoving = false;

            // After finishing a move, immediately start moving in the queued direction if valid
            if (queuedDirection != Vector2Int.zero)
            {
                Vector3Int nextCell = currentCell + new Vector3Int(queuedDirection.x, queuedDirection.y, 0);
                if (IsValidMove(nextCell))
                {
                    currentCell = nextCell;
                    targetPosition = CellToWorldSnapped(nextCell);
                    isMoving = true;
                    lastDirection = targetPosition - transform.position;
                    ActivateRig(lastDirection);
                    // Do NOT set isWalking to false here
                    return;
                }
            }

            // Only set isWalking to false if not moving again
            if (characterAnimController)
            {
                characterAnimController.SetBool("isWalking", false);
                ActivateRig(lastDirection); // This will set the correct idle animation
            }
        }
    }

    private bool IsValidMove(Vector3Int cell)
    {
        // Check if there's a floor tile
        if (floorTilemap.GetTile(cell) == null)
            return false;

        // Check if there's a solid tile
        if (solidsTilemap.GetTile(cell) != null)
            return false;

        return true;
    }

    private void ActivateRig(Vector3 dir)
    {
        if (!characterAnimController) return;

        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);

        if (absX > absY)
        {
            bool facingLeft = (dir.x > 0);
            characterAnimController.ActivateSide(facingLeft, isMoving);
        }
        else
        {
            if (dir.y > 0) characterAnimController.ActivateUp(isMoving);
            else characterAnimController.ActivateDown(isMoving);
        }
    }

    private Vector3 CellToWorldSnapped(Vector3Int cell)
    {
        Vector3 wPos = floorTilemap.CellToWorld(cell);
        wPos.x += xOffset;
        return wPos;
    }
} 