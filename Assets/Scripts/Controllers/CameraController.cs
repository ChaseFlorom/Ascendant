using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
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
        
        // If no target is set, try to find the player
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogWarning("No target set for camera and no player found in scene!");
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Calculate target position
        Vector3 targetPosition = target.position;
        targetPosition.z = transform.position.z; // Keep the camera's z position

        // Clamp position within limits
        targetPosition.x = Mathf.Clamp(targetPosition.x, -moveLimits.x, moveLimits.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, -moveLimits.y, moveLimits.y);

        // Smoothly move to target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        HandleZoom();
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