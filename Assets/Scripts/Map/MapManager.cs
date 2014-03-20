using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/*
 * The MapManager has the job of managing everything happening on the map, thus:
 * -Loading/Initializing the map
 * -Managing objects on the map
 * -Act on input from player
 * -Sync with networking
 * 
 * The MapManager is in effect the game logic manager.
 * */

public class MapManager : MonoBehaviour {

    public List<List<MapTile>> grid = new List<List<MapTile>>();
    public string mapData = null;
    public static int tileSize = 64; //A tile is 64x64 pixels

    private bool moveCamera = false; //Set to true when moving camera(Middle mouse button down)
    private Camera mapCam = null;
    private MapTile currentTile = null; //Current tile mouse is hovering over
    private TileHover hoverObj = null;  //GameObject placed over current tile hover

    public PathFinder walkingPath;
    public PathFinder flyingPath;

    void Start()
    {
        mapCam = GameObject.Find("MapCam").camera;

        //If mapData is not null, load it
        if (mapData != null)
        {
            loadMap(mapData);
        }

        //Load TileHover
        GameObject hoverPrefab = (GameObject)Resources.Load("TileHover");
        hoverObj = ((GameObject)Instantiate(hoverPrefab)).GetComponent<TileHover>();
        hoverObj.gameObject.SetActive(false); //Disable it until we hover over a tile

        //TestMap
        Debug.Log("Loading test map");
        int sizeX = 20;
        int sizeY = 20;
        setSize(sizeX, sizeY);
        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                addTile(x, y, "MapTiles/MapTileGround");
            }
        }

        addTile(2, 2, "MapTiles/MapTileWall");
        addTile(3, 2, "MapTiles/MapTileWall");
        addTile(4, 2, "MapTiles/MapTileWall");
        addTile(5, 2, "MapTiles/MapTileWall");
        addTile(6, 2, "MapTiles/MapTileWall");
        addTile(7, 2, "MapTiles/MapTileWall");

        //Test Mob1
        Monster testMob = (Monster)createObject("Mobs/Mob1");

        //Setup pathfinders
        walkingPath = new PFDijkstra4Dir();
        walkingPath.init(addTile(15, 15, "MapTiles/MapTileWall"), testMob, this);
        walkingPath.calculatePath();
    }
	
	// Update is called once per frame
	void Update () {
        //Camera movement
        if (moveCamera == true)
        {
            mapCam.transform.position -= new Vector3(Input.GetAxisRaw("Mouse X") / 2f, Input.GetAxisRaw("Mouse Y") / 2f);
            //TODO: Check for borders
        }

        if (Input.GetMouseButtonDown(2)) //Middle mouse button
        {
            Screen.lockCursor = true;
            moveCamera = true;
        }
        else if (Input.GetMouseButtonUp(2))
        {
            Screen.lockCursor = false;
            moveCamera = false;

        }
        //Camera Movement end

        //Tile hover check
        Vector3 tilePos = mapCam.ScreenToWorldPoint(Input.mousePosition) - this.transform.position;
        //1 Unit = 64 pixels, so 1 Unit = 1 tile
        int tileX = (int)Math.Floor(tilePos.x+0.5f);
        int tileY = (int)Math.Floor(tilePos.y+0.5f);

        currentTile = getTile(tileX, tileY);
        if (currentTile != null)
        {
            //Place hover object on tile
            hoverObj.gameObject.SetActive(true);
            hoverObj.transform.parent = currentTile.transform;
            hoverObj.transform.localPosition = Vector3.zero;
        }
        else
            hoverObj.gameObject.SetActive(false);

        //TODO: Handle mouse click
        //Left: Place tower if holding
        //Left: Select placed tower and show info 
        //Left: Select tower in menu for placing

        //Right: If placing tower, stop placing
        //Right: If tower is selected, remove selection
        if(Input.GetMouseButtonDown(0))
        {
            if(currentTile == null)
            {
                Debug.Log("Left click on null");
            }
            else
            {
                Debug.Log("Left click on tile: " + currentTile + "(X:" + currentTile.tileX + " Y:"+ currentTile.tileY + ")");
            }
        }


	}

    //Load a map from a string.
    public void loadMap(string mapInput)
    {
        mapData = mapInput;

        //Parse and create tiles
        //TODO
        //sizex;sizey;ObjName,arg1,arg2...;Objname2;objName3;...
    }

    //Set the map size.
    //Note: Clears anything existing
    public void setSize(int width, int height)
    {
        clearMap();

        for (int y = 0; y < height; y++)
        {
            List<MapTile> xList = new List<MapTile>();
            grid.Add(xList);
            for (int x = 0; x < width; x++)
                xList.Add(null);
        }
    }

    //Load GameObject from string
    public GameObject loadGameObject(string resource)
    {
        //TODO Reosurces.Load is slow, so cache the returned object and reuse for later
        GameObject obj = (GameObject)Resources.Load(resource);
        return obj;
    }

    //Create a new GameObject and add it as a tile
    public MapTile addTile(int x, int y, string resource)
    {
        MapTile tile;
        try
        {
            GameObject obj = loadGameObject(resource);
            if (obj == null)
                return null;
            GameObject newObj = (GameObject)Instantiate(obj);
            newObj.transform.parent = this.transform;
            newObj.transform.localPosition = new Vector2(x, y);

            tile = newObj.GetComponent<MapTile>();
            if (tile == null)
            {
                Destroy(newObj.gameObject); //Don't need it if this failed
                return null;
            }
        }
        catch (InvalidCastException e)
        {
            Debug.Log("Casting to MapTile failed for " + resource + "(" + e + ")");
            return null;
        }

        return setTile(x, y, tile);
    }

    //Set the tile at the x-y position on the grid
    public MapTile setTile(int x, int y, MapTile tile)
    {
        if (y >= grid.Count || y < 0 || x >= grid[y].Count || x < 0)
            return null;

        MapTile oldTile = grid[y][x];
        if (oldTile != null)
            Destroy(oldTile.gameObject);

        grid[y][x] = tile;
        tile.init(this, x, y);

        return tile;
    }

    //Create a new instance of objecty given by resource string and add it to the map
    public MapObject createObject(string resource)
    {
        GameObject obj = loadGameObject(resource);
        if(obj == null)
            return null;
        GameObject newObj = (GameObject)Instantiate(obj);

        return addObject(newObj.GetComponent<MapObject>());
    }

    //Add object to map
    public MapObject addObject(MapObject obj)
    {
        obj.transform.parent = this.transform;
        obj.transform.localPosition = Vector3.zero;
        obj.init(this);
        return obj;
    }

    public void removeObject(MapObject obj)
    {
        obj.transform.parent = null;
        obj.map = null;
    }

    public MapTile getTile(int x, int y)
    {
        if (y >= grid.Count || y < 0)
            return null;
        if (x >= grid[y].Count || x < 0)
            return null;

        return grid[y][x];
    }

    //Get the tile at the given world position
    public MapTile getTileWorld(float x, float y)
    {
        Vector3 tilePos = new Vector3(x, y, 0) - transform.position;
        int tileX = (int)Math.Floor(tilePos.x + 0.5f);
        int tileY = (int)Math.Floor(tilePos.y + 0.5f);

        return getTile(tileX, tileY);
    }

    //Clear every MapTile and MapObject(Delete them)
    void clearMap()
    {
        int height = grid.Count;
        for (int y = 0; y < height; y++)
        {
            int width = grid[y].Count;
            for (int x = 0; x < width; x++)
            {
                MapTile tile = grid[y][x];
                if (tile == null)
                    continue;

                //Destroy map object on tile
                MapObject obj = tile.getMapObject();
                if (obj != null)
                    Destroy(obj.gameObject);

                //Destroy tile object
                Destroy(tile.gameObject);
            }
        }
    }
}
