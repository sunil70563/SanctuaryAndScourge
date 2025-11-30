using UnityEngine;
using UnityEngine.EventSystems; // Required for the interfaces

public class ButtonScaleEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    private float pressedScale = 0.9f;

    void Awake()
    {
        // Save the button's original scale when it first loads
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Button pressed: scale down
        transform.localScale = originalScale * pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Button released: scale back to normal
        transform.localScale = originalScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Mouse dragged off the button: scale back to normal
        // This prevents the button from getting "stuck" in the down state
        transform.localScale = originalScale;
    }
}