using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    // Drag your "ButtonContainer" object here in the Inspector
    public Animator buttonContainerAnimator; 

    // This will track if the menu is open or not
    private bool isMenuOpen = false;

    // This function will be called by your round button
    public void ToggleMenu()
    {
        if (buttonContainerAnimator == null)
        {
            Debug.LogError("MenuToggle is missing its 'buttonContainerAnimator' reference!");
            return; 
        }
        // Flip the state
        isMenuOpen = !isMenuOpen; 

        // Tell the Animator to update
        buttonContainerAnimator.SetBool("isOpen", isMenuOpen);
    }
}