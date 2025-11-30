using UnityEngine;

public class EnterRoomPanel : MonoBehaviour
{
    public void OpenEnterRoomPanel()
    {
        gameObject.SetActive(true); 
    }

    public void CloseEnterRoomPanel()
    {
        gameObject.SetActive(false);
    }
}
