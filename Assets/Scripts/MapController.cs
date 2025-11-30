using UnityEngine;
using UnityEngine.EventSystems;

public class MapController : MonoBehaviour, IDragHandler, IScrollHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("Object References")]
    public RectTransform canvasRect; // Drag your Canvas here

    [Header("Zoom Settings")]
    public float minZoom = 1f;
    public float maxZoom = 3f;
    public float zoomSpeed = 0.1f;

    [Header("Navigation Settings")]
    [Tooltip("How fast the map slides after release (0.95 is a good start)")]
    public float dampingRate = 0.95f;
    [Tooltip("How fast the map snaps back from the edge")]
    public float bounceStiffness = 8.0f;

    private RectTransform mapContainerRect;
    private RectTransform mapImageRect;
    private Vector2 velocity;
    private bool isDragging = false;

    void Start()
    {
        mapContainerRect = transform.parent.GetComponent<RectTransform>();
        mapImageRect = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        velocity = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.delta / mapContainerRect.lossyScale.x;
        mapContainerRect.anchoredPosition += delta;
        velocity = delta; // Record speed for inertia
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnScroll(PointerEventData eventData)
    {
        float scroll = eventData.scrollDelta.y;
        float newScale = mapContainerRect.localScale.x + scroll * zoomSpeed;
        newScale = Mathf.Clamp(newScale, minZoom, maxZoom);
        mapContainerRect.localScale = new Vector3(newScale, newScale, 1f);

        // Snap back after zooming
        Vector2 targetPos = GetClampedPosition();
        mapContainerRect.anchoredPosition = Vector2.Lerp(mapContainerRect.anchoredPosition, targetPos, Time.deltaTime * bounceStiffness * 2f);
    }

    void Update()
    {
        if (isDragging) return;

        // Inertia (Damping)
        if (velocity.magnitude > 0.1f)
        {
            mapContainerRect.anchoredPosition += velocity;
            velocity *= dampingRate;
        }
        else
        {
            velocity = Vector2.zero;
        }

        // Bounce (Elasticity)
        if (velocity.magnitude <= 0.1f)
        {
            mapContainerRect.anchoredPosition = Vector2.Lerp(
                mapContainerRect.anchoredPosition,
                GetClampedPosition(),
                Time.deltaTime * bounceStiffness
            );
        }
    }

    Vector2 GetClampedPosition()
    {
        float mapWidth = mapImageRect.rect.width * mapContainerRect.localScale.x;
        float mapHeight = mapImageRect.rect.height * mapContainerRect.localScale.y;

        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        float overhangX = Mathf.Max(0, mapWidth - canvasWidth);
        float overhangY = Mathf.Max(0, mapHeight - canvasHeight);

        float minX = -overhangX / 2f;
        float maxX = overhangX / 2f;
        float minY = -overhangY / 2f;
        float maxY = overhangY / 2f;

        Vector2 clampedPos = mapContainerRect.anchoredPosition;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minX, maxX);
        clampedPos.y = Mathf.Clamp(clampedPos.y, minY, maxY);

        return clampedPos;
    }
}