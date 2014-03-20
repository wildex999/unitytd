using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(TowerMenuItem))]
public class TowerMenuItemEditor : Editor
{
    private void CallbackFunction()
    {
        Debug.Log("updating...");
        // Call the update of the MyScript
    }

    void OnEnable()
    {
        EditorApplication.update += CallbackFunction;
        Debug.Log("ONENABLE");
    }

    void OnDisable()
    {
        EditorApplication.update -= CallbackFunction;
        Debug.Log("ONDISABLE");
    }
}