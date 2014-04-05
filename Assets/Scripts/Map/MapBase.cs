using System;
using UnityEngine;


public abstract class MapBase : MonoBehaviour
{
    public string mapData = null;
    private ResourceCache<GameObject> prefabCache = new ResourceCache<GameObject>();

    //Load a map from a string.
    public void loadMap(string mapInput)
    {
        //mapData = mapInput;

        //Parse and create tiles
        //TODO
        //sizex;sizey;ObjName,arg1,arg2...;Objname2;objName3;...
    }

    //Set the map size.
    //Note: Clears anything existing
    public abstract void setSize(int width, int height);

    public abstract void clearMap();

    //Load GameObject from string
    public GameObject loadGameObject(string resource)
    {
        GameObject obj = prefabCache.getResource(resource);
        if (obj == null)
        {
            obj = (GameObject)Resources.Load(resource);
            prefabCache.setResource(resource, obj);
        }
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
        MapTile ret = setTile(x, y, tile);
        if (ret == null)
        {
            Debug.LogError("Failed to set tile at " + x + " | " + y);
            Destroy(tile.gameObject);
        }
        return ret;
    }

    //Set the tile at the x-y position on the grid
    public abstract MapTile setTile(int x, int y, MapTile tile);
    public abstract MapTile getTile(int x, int y);

    public abstract MapObject addObject(MapObject obj);
    public abstract void removeObject(MapObject obj);

    //Create a new instance of objecty given by resource string and add it to the map
    public MapObject createObject(string resource)
    {
        GameObject obj = loadGameObject(resource);
        if (obj == null)
            return null;
        return addObject(createObject(obj));
    }

    //Creates a copy(Instantiates) of the base object
    public static MapObject createObject(GameObject baseObj)
    {
        GameObject newObj = (GameObject)Instantiate(baseObj);
        return newObj.GetComponent<MapObject>();
    }

    //Get the tile at the given world position
    public MapTile getTileWorld(float x, float y)
    {
        Vector3 tilePos = new Vector3(x, y, 0) - transform.position;
        int tileX = (int)Math.Floor(tilePos.x + 0.5f);
        int tileY = (int)Math.Floor(tilePos.y + 0.5f);

        return getTile(tileX, tileY);
    }
}