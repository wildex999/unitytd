using System;
using System.IO;
using UnityEngine;


public abstract class MapBase : MonoBehaviour
{
    public string mapData = null;
    public static uint unitSizeFixed = 200; //This is the size of a tile in fixed point simulation(Distance when moving from one node to the next)(MUST be divisible by 2)
    public static uint maxTiles = (uint)Math.Floor((Double)(UInt32.MaxValue/unitSizeFixed)); //Max number of tiles per axis with given speed
    public static int simFramerate = 20;

    protected MapInfo mapInfo = null;

    private ResourceCache<GameObject> prefabCache = new ResourceCache<GameObject>();

    public MapBase()
    {
        if (unitSizeFixed / 2f != Mathf.Floor(unitSizeFixed / 2f))
            Debug.LogWarning("MapBase.tileSizeFixed is NOT divisible by 2! Floating point is involved!!!");
    }

    //Load a map from a binary stream
    //Return false if failed
    public virtual bool loadMap(BinaryReader stream)
    {
        /*Map Format:
         * map name(string)
         * map version(int)
         * author user id(int)(online)(= -1 in offline)
         * author name(string)(offline/local)
         * recommended players(ushort)
         * minimum players(ushort)
         * map description(string)
         * size x(int)
         * size y(int)
         * editor version(int)
         * 
         * foreach y
         *   foreach x
         *     tiletype(string)
         * 
         * foreach object
         *     object position, type, arg1, arg2 etc.
         * 
         * 
         * TODO: Tile setup:
         * Tile graphic(string) (Name of sprite to use)
         * Tile Function(string) (Name of script to attach to tile)
         *     Arg1, arg2 etc... depending on function(Get function by name, then pass stream to function parser)
         * Basic Movement modifiers(count):
         *    Walking(id = 0):  movement cost(short)
         *    Flying(id = 1): movement cost(short)
        */
        
        //Read header
        mapInfo = new MapInfo();
        mapInfo.name = DataStream.readStreamStringUTF8(stream);
        mapInfo.version = stream.ReadInt32();
        mapInfo.userId = stream.ReadInt32();
        mapInfo.ownerName = DataStream.readStreamStringUTF8(stream);
        mapInfo.recommendedPlayers = stream.ReadUInt16();
        mapInfo.minimumPlayers = stream.ReadUInt16();
        mapInfo.description = DataStream.readStreamStringUTF8(stream);
        mapInfo.sizeX = stream.ReadInt32();
        mapInfo.sizeY = stream.ReadInt32();
        mapInfo.editorVersion = stream.ReadInt32();

        /*Debug.Log("Map name: " + mapInfo.name);
        Debug.Log("Map Version: " + mapInfo.version);
        Debug.Log("Map userId: " + mapInfo.userId);
        Debug.Log("Map ownerName: " + mapInfo.ownerName);
        Debug.Log("Map Recommended: " + mapInfo.recommendedPlayers);
        Debug.Log("Map Minim: " + mapInfo.minimumPlayers);
        Debug.Log("Map Description:" + mapInfo.description);
        Debug.Log("Map Size: " + mapInfo.sizeX + " | " + mapInfo.sizeY);
        Debug.Log("Map Editor version: " + mapInfo.editorVersion);*/

        setSize(mapInfo.sizeX, mapInfo.sizeY);

        //Read tiles
        Library library = Library.getInstance();
        for(int y = 0; y < mapInfo.sizeY; y++)
        {
            for(int x = 0; x < mapInfo.sizeX; x++)
            {
                int prefabId = stream.ReadInt32();

                if (prefabId == 0)
                {
                    continue; //0 = no tile
                }

                if(prefabId >= library.prefabs.Count)
                {
                    MessageBox.createMessageBox("Error while loading map!", "Got tile ID outside scope: " + prefabId + ". The map might be corrupt or from a different editor version.");
                    return false;
                }

                GameObject prefab = library.prefabs[prefabId];
                if(prefab == null)
                {
                    MessageBox.createMessageBox("Error while loading map!", "Got tile ID which does not exist: " + prefabId + ". The map migth be corrupt, or from a different editor version.");
                    return false;
                }

                //Create and place the tile
                GameObject newObj = (GameObject)Instantiate(prefab);
                MapTile newTile = newObj.GetComponent<MapTile>();

                if(newTile == null)
                {
                    MessageBox.createMessageBox("Error while loading map!", "Tried to load tile, but got something else: " + prefab + "\nID: " + prefabId);
                    return false;
                }

                setTile(x, y, newTile);

                //Read any extra data
                newTile.readFromStream(stream);
            }
        }

        return true;
    }

    //Read the map header from open file
    public static MapInfo readMapHeader(Stream stream)
    {
        //TODO: Handle exceptions
        BinaryReader reader = new BinaryReader(stream);

        string mapName = DataStream.readStreamStringUTF8(reader);
        int mapVersion = reader.ReadInt32();
        int userId = reader.ReadInt32();
        string ownerName = DataStream.readStreamStringUTF8(reader);
        ushort recommendedPlayers = reader.ReadUInt16();
        ushort minimumPlayers = reader.ReadUInt16();
        string mapDescription = DataStream.readStreamStringUTF8(reader);
        int sizeX = reader.ReadInt32();
        int sizeY = reader.ReadInt32();
        int editorVersion = reader.ReadInt32();

        MapInfo headerInfo = new MapInfo(mapName, mapVersion, recommendedPlayers, minimumPlayers, mapDescription, sizeX, sizeY, editorVersion);

        return headerInfo;
    }

    //Read the map header from data array
    public static MapInfo readMapHeader(byte[] mapData)
    {
        return readMapHeader(new MemoryStream(mapData));
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

    public abstract MapObject addObject(MapObject obj, FVector2 fixedPositon);
    public abstract void removeObject(MapObject obj);

    //Create a new instance of objecty given by resource string and add it to the map
    public MapObject createObject(string resource, FVector2 fixedPosition)
    {
        GameObject obj = loadGameObject(resource);
        if (obj == null)
            return null;
        return addObject(createObject(obj), fixedPosition);
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

    public MapTile getTileWorld(FVector2 pos)
    {
        return getTileWorld((int)pos.x, (int)pos.y);
    }

    public MapTile getTileWorld(int x, int y)
    {
        //TODO: Check if this is deterministic. Are there corner cases where this can diverge? (199.9999 on one computer, 200 on another)
        //Shouldn't be any divergense when both x and tileSizeFixed is an int I think.
        //TODO: Write a test case and run it on all platforms
        int tileX = (int)Mathf.Floor(x / MapBase.unitSizeFixed);
        int tileY = (int)Mathf.Floor(y / MapBase.unitSizeFixed);
        return getTile(tileX, tileY);
    }

    public MapInfo getMapInfo()
    {
        return mapInfo;
    }

    //Return the map size
    public abstract uint getSizeFixed();
    public abstract float getSize();
}