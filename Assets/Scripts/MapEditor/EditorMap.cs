using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

/*
 * The Map manager for the Map Editor
 * 
 * */ 

public enum EditorState
{
    None, //Nothing/In menu
    PlaceTile, //Placing tiles
    PlaceGraphic //Placing graphics
}

public class EditorMap : MapBase {
    //Use a camera for each main element, allowing us to enable/disable the rendering easily, and manage draw order.
    public GameObject cameraPosition; //The parent of the tiles and graphics camera
    public Camera tilesCamera; //Draws the tiles
    public Camera graphicsCamera; //Draws the graphic layers
    public Camera groundCamera; //Draw the ground
    public UICamera guiCamera;

    public UIInput mapNameInput;

    public static EditorMap instance;
    public GameObject placeHover; //Sprite hovering at the mouse position when placing tiles or graphics.

    private Dictionary<string, MapTile> tiles = new Dictionary<string, MapTile>();
    private MapTile currentPlaceTile;

    private EditorState editorState;
    private bool inputOnGui; //Mouse over gui, ignore input
    private bool moveCamera = false;


    public EditorMap()
    {
        instance = this;
        editorState = EditorState.None;
        mapInfo = new MapInfo();
    }

	// Use this for initialization
	void Start () {
        placeHover.SetActive(false);
        mapNameInput.text = mapInfo.name;
        GA.API.Design.NewEvent("ALPHA2:GAME:StartMapEditor");

        MessageBox.createMessageBox("Warning", "Please note that the Map file format is currently under heavy iteration, and any maps saved will therefore most likely not be loadable in the next version!\n" +
                                                "Only use the Map editor for testing!");
	}
	
	// Update is called once per frame
	void Update () {
        //List for exit(TODO: Replace with a proper menu)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.LoadLevel(0);
        }

        //Check if we are to check input
        Camera nguiCam = guiCamera.camera;
        Ray inputRay = nguiCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(inputRay.origin, inputRay.direction, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("GUIObj")))
            inputOnGui = true;
        else
            inputOnGui = false;

        switch(editorState)
        {
            case EditorState.PlaceTile:
                tileUpdate();
                break;
            case EditorState.PlaceGraphic:
                graphicUpdate();
                break;
        }

        //Camera Movement
        if(!inputOnGui && Input.GetMouseButtonDown(2))
        {
            Screen.lockCursor = true;
            moveCamera = true;
        }
        else if(Input.GetMouseButtonUp(2)) //Allow to go out of movement while over gui
        {
            Screen.lockCursor = false;
            moveCamera = false;
        }
        //Panning
        if(moveCamera)
            cameraPosition.transform.position -= new Vector3(Input.GetAxisRaw("Mouse X") / 2f, Input.GetAxisRaw("Mouse Y") / 2f);

        //Zoom
        float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        Camera[] cameras = cameraPosition.GetComponentsInChildren<Camera>();
        foreach (Camera cam in cameras)
        {
            if (mouseScroll > 0)
                cam.orthographicSize = Mathf.Max(cam.orthographicSize - mouseScroll, 3.5f);
            else if (mouseScroll < 0)
                cam.orthographicSize = Mathf.Min(cam.orthographicSize - mouseScroll, 15f);
        }

        //TODO: Press C to center camera

	}

    //Set state to placing tile, and set tile as current
    public void startPlaceTile(MapTile tile)
    {
        currentPlaceTile = tile;
        SpriteRenderer placeSpr = placeHover.GetComponent<SpriteRenderer>();
        SpriteRenderer tileSpr = tile.GetComponent<SpriteRenderer>();

        placeSpr.sprite = tileSpr.sprite;

        setEditorState(EditorState.PlaceTile);
    }

    //Set a new editor state(Unsetting the previous state)
    private void setEditorState(EditorState newState)
    {
        //Unset previous state, cleaning up whatever is needed
        switch(editorState)
        {
            case EditorState.PlaceTile:
                placeHover.SetActive(false);
                break;
        }

        //Set the new state
        switch(newState)
        {
            case EditorState.PlaceTile:
                placeHover.SetActive(true);
                break;
        }
        editorState = newState;
    }

    //Handle input and movement while placing tiles
    void tileUpdate()
    {
        //Tile hover check
        Vector3 worldMousePos = tilesCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 tilePos = worldMousePos - this.transform.position;
        int tileX = (int)Mathf.Floor(tilePos.x + 0.5f);
        int tileY = (int)Mathf.Floor(tilePos.y + 0.5f);
        //1 Unit = 1 tile
        placeHover.transform.parent = this.transform;
        placeHover.transform.localPosition = new Vector3(tileX, tileY, placeHover.transform.localPosition.z);

        //Check mouse clicks
        if (!inputOnGui)
        {
            if (Input.GetMouseButton(0)) //Left click/hold: Place tile
            {
                GameObject newObj = (GameObject)Instantiate(currentPlaceTile.gameObject);
                if (newObj == null)
                {
                    Debug.LogError("Failed to Instantiate new tile from: " + currentPlaceTile);
                    return;
                }

                MapTile newTile = newObj.GetComponent<MapTile>();
                if (newTile == null)
                {
                    Debug.LogError("Failed to get MapTile from gameobject during place: " + currentPlaceTile);
                    return;
                }
                newTile.prefab = currentPlaceTile.gameObject;

                setTile(tileX, tileY, newTile);
            }
            else if (Input.GetMouseButtonDown(1)) //Right click: Deselect tile
            {
                setEditorState(EditorState.None);
                return;
            }
        }
    }

    //Handle input, movement and placing of graphics.
    void graphicUpdate()
    {

    }

    public override void setSize(int width, int height)
    {
        //Do nothing for now. Later remove anything outside the defined size
    }

    public override void clearMap()
    {
        foreach(KeyValuePair<string, MapTile> tile in tiles)
        {
            Destroy(tile.Value.gameObject);
        }
        tiles.Clear();
    }

    public override MapTile setTile(int x, int y, MapTile tile)
    {
        MapTile prev = getTile(x, y);
        if (prev != null)
            Destroy(prev.gameObject);

        tiles[x + "-" + y] = tile;
        tile.transform.parent = this.transform;
        tile.transform.localPosition = new Vector2(x, y);
        tile.init(null, x, y);

        return tile;
    }

    public override MapTile getTile(int x, int y)
    {
        MapTile tile = null;
        tiles.TryGetValue(x + "-" + y, out tile);

        return tile;
    }

    public override MapObject addObject(MapObject obj, FVector2 fixedPosition)
    {
        throw new System.NotImplementedException();
    }

    public override void removeObject(MapObject obj)
    {
        throw new System.NotImplementedException();
    }

    public override uint getSizeFixed()
    {
        throw new NotImplementedException();
    }

    public override float getSize()
    {
        throw new NotImplementedException();
    }

    private void OnMapNameSubmit(string newName)
    {
        mapInfo.name = newName;
    }

    //Return the current map file
    public string getCurrentMapFile()
    {
        return mapInfo.filename;
    }

    //Start a new map
    public void newMap()
    {
        clearMap();
    }

    public void doLoadMap(string filename)
    {
        if (filename.EndsWith(".utdmap"))
            filename = filename.Remove(filename.LastIndexOf(".utdmap")); //Remove if it already exists

        FileStream mapFile = null;
        try 
        {
            mapFile = File.Open(MapInfo.getMapsPath() + "/" + filename + ".utdmap", FileMode.Open);
        }
        catch (Exception ex)
        {
            MessageBox.createMessageBox("Error loading map", "Unable to open file: \n" + ex.Message, NGUITools.GetRoot(gameObject));
            return;
        }

        //Clear existing map
        newMap();

        BinaryReader reader = new BinaryReader(mapFile);
        loadMap(reader);

        mapFile.Close();

        Debug.Log("Loaded map!");
    }

    //Initiate saving the current map
    public void doSaveMap(string filename)
    {
        DirectoryInfo mapsPath = new DirectoryInfo(MapInfo.getMapsPath());
        if (!mapsPath.Exists)
            mapsPath.Create();

        if (filename.EndsWith(".utdmap"))
            filename = filename.Remove(filename.LastIndexOf(".utdmap")); //Remove if it already exists

        FileStream mapFile = null;
        try
        {
            mapFile = File.Open(MapInfo.getMapsPath() + "/" + filename + ".utdmap", FileMode.Create);
        }
        catch(Exception ex)
        {
            MessageBox.createMessageBox("Error", "Got error while opening file for saving!\n" + ex.Message, NGUITools.GetRoot(gameObject));
            return;
        }

        byte[] mapData = saveMap();

        mapFile.Write(mapData, 0, mapData.Length);

        mapFile.Close();

        Debug.Log("Write complete!");
    }

    //Write map to byte array
    private byte[] saveMap()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        DataStream.writeStringUTF8ToStream(writer, mapInfo.name);
        writer.Write(++mapInfo.version);
        //TODO: If saving to online, write current user id(Used for publishing)
        writer.Write(mapInfo.userId);
        DataStream.writeStringUTF8ToStream(writer, mapInfo.ownerName);
        writer.Write(mapInfo.recommendedPlayers);
        writer.Write(mapInfo.minimumPlayers);
        DataStream.writeStringUTF8ToStream(writer, mapInfo.description);

        //Get Min and Max X/Y
        int minX = 0;
        int minY = 0;
        int maxX = 0; //Used to update map size
        int maxY = 0;
        foreach(KeyValuePair<string, MapTile> pair in tiles)
        {
            MapTile tile = pair.Value;
            if (tile.tileX < minX)
                minX = tile.tileX;
            else if (tile.tileX > maxX)
                maxX = tile.tileX;

            if (tile.tileY < minY)
                minY = tile.tileY;
            else if (tile.tileY > maxY)
                maxY = tile.tileY;
        }
        Debug.Log("Min: " + (-minX) + " | " + (-minY));

        mapInfo.sizeX = (-minX) + maxX + 1;
        mapInfo.sizeY = (-minY) + maxY + 1;

        if(tiles.Count == 0)
        {
            mapInfo.sizeX = 0;
            mapInfo.sizeY = 0;
        }

        Debug.Log("Map size: " + mapInfo.sizeX + " | " + mapInfo.sizeY);

        writer.Write(mapInfo.sizeX);
        writer.Write(mapInfo.sizeY);
        writer.Write(mapInfo.editorVersion); //TODO: Set new editorversion

        //Write the tiles
        //TODO: Use tile system described in MapBase loadmap.
        for (int y = minY; y <= maxY; y++)
        {
            for(int x = minX; x <= maxX; x++)
            {
                string tileStr = x + "-" + y;
                MapTile tile;
                tiles.TryGetValue(tileStr, out tile);
                
                if (tile == null)
                {
                    writer.Write((int)0); //Write 0(No tile)
                    continue;
                }

                writer.Write(tile.getUniqueId()); //Write the id
                tile.writeToStream(writer); //Let the tile write any extra data it requires
            }
        }
        
        return stream.ToArray();
    }
}
