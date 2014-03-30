using UnityEngine;
using System.Collections;

public class GUITextInput : MonoBehaviour {

    public string stringToEdit = "Hello World";
    void OnGUI()
    {

        Vector3 screenPoint = Camera.allCameras[0].WorldToScreenPoint(transform.position);
        Debug.Log("Y: " + screenPoint.y);
        stringToEdit = GUI.TextField(new Rect(screenPoint.x, Camera.allCameras[0].pixelHeight - screenPoint.y, 200, 20), stringToEdit, 25);
    }
}
