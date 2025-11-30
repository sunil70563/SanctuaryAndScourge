using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Image

[RequireComponent(typeof(Image))] // This guarantees an Image component exists
public class ImageSlider : MonoBehaviour
{
    // 1. Create a public array for your 6 sprites
    public Sprite[] sliderStates;

    // 2. Create a public reference to the Image component
    private Image displayImage;

    // 'Awake' runs before any 'OnEnable' or 'Start' functions
    void Awake()
    {
        // Get the Image component on this same GameObject
        displayImage = GetComponent<Image>();
    }

    // 3. This is the public function we will call from the slider
    public void UpdateSliderImage(float value)
    {
        // Add a null check just to be safe
        if (displayImage == null)
        {
            displayImage = GetComponent<Image>(); 
        }

        if (displayImage == null)
        {
            Debug.LogError("ImageSlider is missing its Image component!");
            return; 
        }


        // Convert the float value (0, 1, 2, 3, 4, or 5) to an integer
        int index = (int)value;

        // Change the sprite
        if (sliderStates.Length > index && index >= 0) // Added check for index >= 0
        {
            displayImage.sprite = sliderStates[index];
        }
    }
}