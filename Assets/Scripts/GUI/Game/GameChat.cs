using UnityEngine;


public class GameChat : MonoBehaviour
{
    public UITextList output;
    public UIInputSaved input;

    public static GameChat createGameChat(GameObject parent)
    {
        GameObject prefab = GUIPrefabs.getGUIPrefabs().gameChat;
        GameObject newObj = NGUITools.AddChild(parent, prefab);

        GameChat chat = newObj.GetComponent<GameChat>();
        if(chat == null)
        {
            Debug.LogError("Failed to get GameChat!");
            Destroy(newObj);
        }
        Debug.Log("Created chat! Parent:" + parent);

        return chat;
    }
}