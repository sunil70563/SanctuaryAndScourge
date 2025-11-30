using UnityEngine;

public class ExitButton : MonoBehaviour
{
    public void OpenExitPanel()
    {
        gameObject.SetActive(true); 
    }

    public void CloseExitPanel()
    {
        gameObject.SetActive(false);
    }

    public void ExitApplication(){
        Application.Quit();
    }
}
