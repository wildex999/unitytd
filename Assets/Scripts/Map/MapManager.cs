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

    private bool moveCamera = false; //Set to true when moving camera(Middle mouse button down)
    private Camera mapCam = null;

    void Start()
    {
        mapCam = GameObject.Find("MapCam").camera;

        //If mapData is not null, load it
        if (mapData != null)
        {
            loadMap(mapData);
        }

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
    void setSize(int width, int height)
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

    //Create a new GameObject and add it as a tile
    bool addTile(int x, int y, string resource)
    {
        MapTile tile;
        try
        {
            //TODO Reosurces.Load is slow, so cache the returned object and reuse for later
            GameObject obj = (GameObject)Resources.Load(resource);
            if (obj == null)
                return false;
            GameObject newObj = (GameObject)Instantiate(obj);
            newObj.transform.parent = this.transform;
            newObj.transform.localPosition = new Vector2(x, y);

            tile = (MapTile)newObj.GetComponent(typeof(MapTile));
            if (tile == null)
            {
                Destroy(newObj.gameObject); //Don't need it if this failed
                return false;
            }
        }
        catch (InvalidCastException e)
        {
            Debug.Log("Casting to MapTile failed for " + resource + "(" + e + ")");
            return false;
        }

        return setTile(x, y, tile);
    }

    //Set the tile at the x-y position on the grid
    bool setTile(int x, int y, MapTile tile)
    {
        if (y >= grid.Count || y < 0 || x >= grid[y].Count || x < 0)
            return false;

        MapTile oldTile = grid[y][x];
        if (oldTile != null)
            Destroy(oldTile.gameObject);

        grid[y][x] = tile;
        tile.init(this, x, y);

        return true;
    }

    public MapTile getTile(int x, int y)
    {
        if (y >= grid.Count || y < 0)
            return null;
        if (x >= grid[y].Count || x < 0)
            return null;

        return grid[y][x];
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
