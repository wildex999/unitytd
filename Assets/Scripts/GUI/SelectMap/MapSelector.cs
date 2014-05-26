using System;
using System.IO;
using UnityEngine;

public enum MapSelectorType
{
    Open, //Open owned maps or other maps
    OpenEditor, //Open only owned maps, or maps you can edit(Hides things like friends maps etc.)
    Save //Choose where to save map
}

public class MapSelector : MonoBehaviour
{
    public delegate void SelectedMapEventHandler(MapInfo item);
    public event SelectedMapEventHandler SelectMapEvent; //Triggers when the window closes(item = null if no map choosen)

    public UILabel buttonLabel; //Label to change depending on type
    public GameObject filenameInput; //Parent of the filename input, allowing us to disable it when using online
    public UILabel currentMapName; //Label showing the name of the currently selected map

    private MapSelectorType selectorType;

    //Create a new MapSelector window
    public static MapSelector createMapSelector(GameObject parent, MapSelectorType type)
    {
        GameObject selector = NGUITools.AddChild(parent, GUIPrefabs.getGUIPrefabs().MapSelector);

        MapSelector inst = selector.GetComponent<MapSelector>();
        inst.selectorType = type;

        if (type == MapSelectorType.Save)
            inst.buttonLabel.text = "Save";

        return inst;
    }

    public void setFilename(string filename)
    {
        UIInput inputLabel = filenameInput.GetComponentInChildren<UIInput>();
        if (inputLabel == null)
            return;

        inputLabel.text = filename;
    }

    public string getFilename()
    {
        UIInput inputLabel = filenameInput.GetComponentInChildren<UIInput>();
        if (inputLabel == null)
            return "";

        return inputLabel.text;
    }

    //Called when the cancel button is pressed
    private void OnCancel()
    {
        if (SelectMapEvent != null)
            SelectMapEvent(null);

        Destroy(gameObject);
    }

    //Called whenever a map is selected and the "Open"/"Save" button is pressed
    private void OnOk()
    {
        //If loading, verify that file with given name actually exists.
        //If saving, give warning if we are overwriting

        //TODO: Handle online maps

        MapInfo item = null;
        string filename = getFilename();
        FileInfo file = null;

        switch(selectorType)
        {
            case MapSelectorType.Open:
            case MapSelectorType.OpenEditor:
                try
                {
                    if (filename.EndsWith(".utdmap"))
                        filename = filename.Remove(filename.LastIndexOf(".utdmap")); //Remove if it already exists
                    file = new FileInfo(MapInfo.getMapsPath() + "/" + filename + ".utdmap");
                    if (!file.Exists)
                    {
                        MessageBox.createMessageBox("Error", "Unable to load map, the file doesn't exist: " + MapInfo.getMapsPath() + "/" + filename + ".utdmap", NGUITools.GetRoot(gameObject));
                        return;
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.createMessageBox("Error", "Unable to load map:" + ex.Message, NGUITools.GetRoot(gameObject));
                    return;
                }

                item = new MapInfo();

                //In case the user wrote in a filename directly, we have to try to read it
                try
                {
#if !UNITY_WEBPLAYER
                    FileStream fileInput = file.Open(FileMode.Open);
                    item = MapBase.readMapHeader(fileInput);
#endif
                    
                }
                catch(Exception ex)
                {
                    MessageBox.createMessageBox("Error", "Unable to load map:" + ex.Message, NGUITools.GetRoot(gameObject));
                    return;
                }

                item.filename = filename + ".utdmap";

                if (SelectMapEvent != null)
                    SelectMapEvent(item);

                break;
            case MapSelectorType.Save:
                try
                {
                    if (filename.EndsWith(".utdmap"))
                        filename = filename.Remove(filename.LastIndexOf(".utdmap")); //Remove if it already exists
                    file = new FileInfo(MapInfo.getMapsPath() + "/" + filename + ".utdmap");
                    if (file.Exists)
                    {
                        //Show overwrite warning here
                        Debug.Log("OVERWRITE");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.createMessageBox("Error", "Save file error:" + ex.Message, NGUITools.GetRoot(gameObject));
                    return;
                }

                item = new MapInfo();
                item.filename = getFilename();
                if (SelectMapEvent != null)
                    SelectMapEvent(item);

                break;
        }

        Destroy(gameObject);
    }

    //Called whenever the current map selection changes
    private void OnMapSelect(GameObject button)
    {
        MapItem item = button.GetComponent<MapItem>();
        if(item == null)
            return;
        currentMapName.text = item.mapName.text;

        UIInput filename = filenameInput.GetComponentInChildren<UIInput>();
        if (filename == null)
            return;
        filename.text = item.mapInfo.filename;

    }


}