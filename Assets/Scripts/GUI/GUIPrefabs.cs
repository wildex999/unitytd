using UnityEngine;
using System.Collections;

public class GUIPrefabs : MonoBehaviour {
    private static GUIPrefabs instance;

    public GameObject MapSelector;
    public GameObject MessageBox;
    public GameObject PreGamePanel;
    public GameObject gameChat;

    public GUIPrefabs()
    {
        instance = this;
    }

    public static GUIPrefabs getGUIPrefabs()
    {
        if (instance != null)
            return instance;

        GameObject prefab = Resources.Load<GameObject>("GUI/GUIPrefabs");
        if (prefab == null)
            Debug.LogError("Unable to load prefab GUIPrefabs!");
        Instantiate(prefab);

        return instance;
    }
}
