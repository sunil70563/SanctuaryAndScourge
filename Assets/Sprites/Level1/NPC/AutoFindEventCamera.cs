using UnityEngine;

// This script will automatically find the Main Camera
// and assign it to the World Space Canvas it's on.
[RequireComponent(typeof(Canvas))]
public class AutoFindEventCamera : MonoBehaviour
{
    void Start()
    {
        // Get the Canvas component on this GameObject
        Canvas canvas = GetComponent<Canvas>();

        // Find the Main Camera in the scene
        // and assign it to the 'Event Camera' slot
        if (canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
        }
    }
}