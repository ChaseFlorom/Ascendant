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

    private void Start()
    {
        targetPosition = transform.position;
        currentCell = floorTilemap.WorldToCell(transform.position);
    }

    private void Update()
    {
        if (GameStateManager.Instance.IsInBattle())
            return;

        HandleMovementInput();
        UpdateMovement();
    }

    private void HandleMovementInput()
    {
        if (isMoving)
            return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (horizontal != 0 || vertical != 0)
        {
            Vector3Int newCell = currentCell;
            
            if (horizontal != 0)
            {
                newCell.x += (int)Mathf.Sign(horizontal);
            }
            else if (vertical != 0)
            {
                newCell.y += (int)Mathf.Sign(vertical);
            }

            // Check if the new cell is valid
            if (IsValidMove(newCell))
            {
                currentCell = newCell;
                targetPosition = CellToWorldSnapped(newCell);
                isMoving = true;

                // Set animation direction
                lastDirection = targetPosition - transform.position;
                ActivateRig(lastDirection);
            }
        }
    }

    private void UpdateMovement()
    {
        if (!isMoving)
        {
            // Ensure animation is in idle state when not moving
            if (characterAnimController)
            {
                characterAnimController.SetBool("isWalking", false);
            }
            return;
        }

        if (characterAnimController)
        {
            characterAnimController.SetBool("isWalking", true);
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            isMoving = false;

            // Ensure we're in the correct idle state based on last direction
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
            characterAnimController.ActivateSide(facingLeft);
        }
        else
        {
            if (dir.y > 0) characterAnimController.ActivateUp();
            else characterAnimController.ActivateDown();
        }
    }

    private Vector3 CellToWorldSnapped(Vector3Int cell)
    {
        Vector3 wPos = floorTilemap.CellToWorld(cell);
        wPos.x += xOffset;
        return wPos;
    }
} 