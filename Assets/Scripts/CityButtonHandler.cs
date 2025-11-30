using UnityEngine;
using UnityEngine.EventSystems;

public class CityButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI Interaction")]
    [Tooltip("Drag the Panel GameObject (e.g., Match Setup Panel) here.")]
    public GameObject panelToOpen;

    // References
    private MapController mapController;
    
    // This flag will track if we are dragging
    private bool isDragging = false;

    void Start()
    {
        // 1. Find the MapController (on the MapImage sibling) so we can pass drag events
        if (transform.parent != null)
        {
            mapController = transform.parent.GetComponentInChildren<MapController>();
        }

        if (mapController == null)
            Debug.LogError("CityButtonHandler: Could not find MapController! Dragging will not work.");
            
        // Ensure the panel is closed by default (Optional, removes need to hide manually)
        if (panelToOpen != null)
        {
            // panelToOpen.SetActive(false); // Uncomment if you want it to auto-hide on start
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // User has pressed down. Reset the drag flag.
        isDragging = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // User has started dragging!
        isDragging = true;
        
        // Pass the event to the MapController so it can start dragging the map
        if (mapController != null) mapController.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // User is dragging. Pass the event to the MapController.
        if (mapController != null) mapController.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // User stopped dragging. Pass the event to the MapController.
        if (mapController != null) mapController.OnEndDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // User released the click/tap.
        
        // Check if we were dragging.
        if (!isDragging)
        {
            // --- LOGIC CHANGED HERE ---
            // Instead of loading a scene, we open the panel.
            if (panelToOpen != null)
            {
                panelToOpen.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"No Panel assigned to CityButtonHandler on {gameObject.name}");
            }
        }
    }
}