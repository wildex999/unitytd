using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

/**
 * Go through the library, and give an unique id to all prefabs who have a Component that implements the ISerializedObject interface 
 * Some code borrowed from http://forum.unity3d.com/threads/136318-Passing-Asset-Reference-over-Network-with-RPCs
 **/

public class PrefabIdentifier
{

    [MenuItem("Assets/Identify Prefabs")]
    static void IdentifyPrefabs()
    {
        GameObject libraryObj = Resources.Load<GameObject>("Library");
        Library library = libraryObj.GetComponent<Library>();

        List<Object> prefabs = getAssets("*.prefab", Application.dataPath + "/Resources");

        //We have to do a first pass to find out what is the current highest id(And thus where we can begin allocating new ones from)
        int nextId = 0;
        List<KeyValuePair<ISerializedObject, GameObject>> toAdd = new List<KeyValuePair<ISerializedObject, GameObject>>();
        foreach(Object obj in prefabs)
        {
            GameObject gameObj = (GameObject)obj;
            ISerializedObject serialized = gameObj.GetComponent(typeof(ISerializedObject)) as ISerializedObject;
            if (serialized == null)
                continue;

            if (serialized.getUniqueId() > nextId)
                nextId = serialized.getUniqueId();
            else if (serialized.getUniqueId() == 0)
                toAdd.Add(new KeyValuePair<ISerializedObject, GameObject>(serialized, gameObj));
        }

        //Give unique id and add to library
        int count = 0;
        foreach (KeyValuePair<ISerializedObject, GameObject> pair in toAdd)
        {
            GameObject gameObj = pair.Value;
            pair.Key.setUniqueId(++nextId);
            EditorUtility.SetDirty(gameObj); //Make sure Unity saves it

            library.addPrefab(nextId, gameObj);
            count++;
            Debug.Log("GameObj: " + gameObj.name);
        }

        EditorUtility.SetDirty(libraryObj);

        EditorUtility.DisplayDialog("Prefab Identification", "Done identifying prefabs. " + count + " New prefabs identified!", "OK");

    }


    static List<Object> getAssets(string searchPattern, string dataPath)
    {

        List<Object> tempObjects = new List<Object>();
        DirectoryInfo directory = new DirectoryInfo(dataPath);
        FileInfo[] goFileInfo = directory.GetFiles(searchPattern, SearchOption.AllDirectories);
        int goFileInfoLength = goFileInfo.Length;
        FileInfo tempGoFileInfo; string tempFilePath;
        int assetIndex; Object tempGO;

        for (int i = 0; i < goFileInfoLength; i++)
        {
            // Check File Info
            tempGoFileInfo = goFileInfo[i] as FileInfo;
            if (tempGoFileInfo == null) continue;
            tempFilePath = tempGoFileInfo.FullName;

            // Build Relative Path
            assetIndex = tempFilePath.IndexOf("Assets");
            if (assetIndex < 0) { assetIndex = 0; }
            tempFilePath = tempFilePath.Substring(assetIndex, tempFilePath.Length - assetIndex);

            // Try to Load and Add to List
            tempGO = (Object)AssetDatabase.LoadAssetAtPath(tempFilePath, typeof(Object));
            if (tempGO == null) { continue; }
            tempObjects.Add(tempGO);
        }
        return tempObjects;
    }

}