using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private Vector2 moveLimits = new Vector2(50f, 50f); // X and Y limits

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;
    [SerializeField] private float smoothZoomTime = 0.1f;

    private Vector3 velocity = Vector3.zero;
    private float targetZoom;
    private float zoomVelocity;

    private void Start()
    {
        targetZoom = Camera.main.orthographicSize;
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    private void HandleMovement()
    {
        // Get input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calculate target position
        Vector3 targetPosition = transform.position + new Vector3(horizontal, vertical, 0) * moveSpeed * Time.deltaTime;

        // Clamp position within limits
        targetPosition.x = Mathf.Clamp(targetPosition.x, -moveLimits.x, moveLimits.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, -moveLimits.y, moveLimits.y);

        // Smoothly move to target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    private void HandleZoom()
    {
        // Get scroll input
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        // Calculate target zoom
        targetZoom -= scroll * zoomSpeed;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        // Smoothly adjust zoom
        Camera.main.orthographicSize = Mathf.SmoothDamp(
            Camera.main.orthographicSize,
            targetZoom,
            ref zoomVelocity,
            smoothZoomTime
        );
    }
} 