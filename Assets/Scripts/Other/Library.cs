using System.Collections.Generic;
using UnityEngine;

//Stores a list of prefabs, making it possible to get the prefab from the Unique prefab Id
public class Library : MonoBehaviour
{
    public static Library instance;

    public List<GameObject> prefabs = new List<GameObject>();

    public static Library getInstance()
    {
        if (instance != null)
            return instance;

        GameObject prefab = Resources.Load<GameObject>("Library");
        if (prefab == null)
            Debug.LogError("Could not get Library prefab");

        GameObject newObj = (GameObject)Instantiate(prefab);
        instance = newObj.GetComponent<Library>();

        return instance;
    }

    public void addPrefab(int id, GameObject obj)
    {
        if (prefabs.Count == id) //Adding next element
            prefabs.Add(obj);
        else if(prefabs.Count < id) //Adding outside array
        {
            while (prefabs.Count < id)
                prefabs.Add(null);
            prefabs.Add(obj);
        }
        else //Overwriting existing
        {
            Debug.LogWarning("Overwriting existing unique Prefab in Library. Id: " + id);
            prefabs[(int)id] = obj;
        }
    }
}