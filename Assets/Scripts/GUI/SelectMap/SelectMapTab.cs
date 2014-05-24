using System.IO;
using System.Security;
using UnityEngine;

public class SelectMapTab : MonoBehaviour
{
    protected static SelectMapTab currentTab = null;
    protected static GameObject baseMapItemPrefab = null;

    public GameObject itemsList; //Parent object for the list of Maps
    public UIDraggablePanel itemsPanel; //The draggable panel containing the items 
    public MapSelector selector;

    protected string cOwner = " [FF0000]Owner:[-] "; //Colored owner
    protected string cVersion = " [01DF01]Version:[-] ";
    protected string cRecommendedPlayers = " [00BFFF]Recommended Players:[-] ";
    protected string cSize = " [ACFA58]Size:[-] ";

    void Awake()
    {
        if (itemsList == null)
            Debug.LogError("No items list set for maps!");

        if(baseMapItemPrefab == null)
        {
            baseMapItemPrefab = Resources.Load<GameObject>("GUI/SelectMap/MapItem");
            if (baseMapItemPrefab == null)
                Debug.LogError("Unable to load MapItem prefab!");
        }
    }

    //Get the save directory, create it if it doesn't exist
    protected DirectoryInfo getDirectory(string path, bool create = true)
    {
        DirectoryInfo dir = null;
        try
        {
            dir = new DirectoryInfo(path);
        }
        catch(SecurityException ex)
        {
            Debug.LogError("User does not have permission to access path: " + path + ". Got error: " + ex.Message);
            return null;
        }

        try
        {
            if (!dir.Exists && create)
                dir.Create();
        }
        catch(IOException ex)
        {
            Debug.LogError("Could not create path: " + path + ". Got error: " + ex.Message);
            return null;
        }

        return dir;
    }
}