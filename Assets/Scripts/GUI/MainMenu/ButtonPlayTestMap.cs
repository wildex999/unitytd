using UnityEngine;
using System.Collections;

public class ButtonPlayTestMap : MonoBehaviour {

    void OnMouseDown()
    {
        Debug.Log("CLICKED");
        Application.LoadLevel(1);
    }
}
