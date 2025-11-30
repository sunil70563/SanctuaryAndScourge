using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    public float smoothTime = 0.3f;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;
    public float playerViewSize = 7f;
    public float fullMapViewSize = 15.2f;

    [Header("Map Settings")]
    public Vector3 mapCenterPosition = new Vector3(0, -0.1f, 0);

    [Header("Clamping Settings")]
    [Tooltip("Minimum X coordinate the camera can move to.")]
    public float minX = -20f;
    [Tooltip("Maximum X coordinate the camera can move to.")]
    public float maxX = 20f;
    [Tooltip("Minimum Y coordinate the camera can move to.")]
    public float minY = -10f;
    [Tooltip("Maximum Y coordinate the camera can move to.")]
    public float maxY = 10f;

    private Transform target;
    private bool showFullMap = false;
    private Camera cam;
    private Vector3 currentVelocity;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraController needs to be attached to a Camera object!");
            this.enabled = false;
        }
    }

    void LateUpdate()
    {
        Vector3 desiredPosition;
        float desiredSize;

        if (showFullMap)
        {
            // State 1: Full Map View
            desiredPosition = mapCenterPosition + offset;
            desiredSize = fullMapViewSize;
        }
        else
        {
            // State 2: Follow Player
            if (target != null)
            {
                desiredPosition = target.position + offset;
            }
            else
            {
                desiredPosition = transform.position;
            }
            desiredSize = playerViewSize;
        }

        // 1. Calculate the SmoothDamp position (but don't apply it yet)
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);

        // 2. Apply Clamping Logic
        // We clamp the X and Y values to keep the camera within bounds
        float clampedX = Mathf.Clamp(smoothedPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(smoothedPosition.y, minY, maxY);

        // 3. Apply final position
        transform.position = new Vector3(clampedX, clampedY, smoothedPosition.z);

        // 4. Handle Zoom
        if (cam != null)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, desiredSize, Time.deltaTime * zoomSpeed);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public bool ToggleViewMode()
    {
        showFullMap = !showFullMap;
        return showFullMap;
    }
}