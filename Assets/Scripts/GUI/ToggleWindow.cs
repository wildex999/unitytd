using UnityEngine;
using System.Collections;

//Toggle the given window the enabled or disabled. Also disable any globaly open window previously opened with this script.

public class ToggleWindow : MonoBehaviour {

    public GameObject targetWindow;
    public bool closePrevious = true; //Whether to disable the previously opened window
    public bool saveAsPrevious = true; //Whether to save this as a previous window(false = this window won't be closed unless done so manually)

    public static GameObject previousWindow;

    public void OnClick()
    {
        if(closePrevious && previousWindow != null && previousWindow != targetWindow)
        {
            if (previousWindow.activeSelf)
                previousWindow.SetActive(false);
            previousWindow = null;
        }

        if(targetWindow == null)
            return;

        targetWindow.SetActive(!targetWindow.activeSelf);

        if (saveAsPrevious)
            previousWindow = targetWindow;

    }
}
