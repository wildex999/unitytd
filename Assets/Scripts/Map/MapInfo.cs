
//Class containing the info of a map, contained at the head of the map file format.

using UnityEngine;
public class MapInfo
{
    public int id; //Id when published online, 0 if not
    public string filename; //Filename when stored locally, empty if not
    public byte[] mapData = null; //Raw map data. Used by authorative when mapid = 0, and all players when downloading the map

    public string name; //Map name
    public int version; //Map version
    public ushort recommendedPlayers;
    public ushort minimumPlayers;
    public string description;
    public int sizeX; //Size in tiles
    public int sizeY;
    public bool offlineChanged; //True if the map has been changed offline. Used to avoid loading from online overwriting offline changes.
    public int editorVersion; //Version of the map editor which created the map

    //Name or Id depends on if the map was stored online or offline.
    public string ownerName = ""; //Offline/Local
    public int userId = -1; //Online

    public MapInfo(string name, int version, ushort recommendedPlayers, ushort minimumPlayers, string description, int sizeX, int sizeY, int editorVersion)
    {
        this.name = name;
        this.version = version;
        this.recommendedPlayers = recommendedPlayers;
        this.minimumPlayers = minimumPlayers;
        this.description = description;
        this.sizeX = sizeX;
        this.sizeY = sizeY;
        this.editorVersion = editorVersion;
    }

    //Default set-up for map editor
    public MapInfo() 
    {
        this.id = 0;
        this.filename = "";

        this.name = "No name";
        this.version = 0;
        this.recommendedPlayers = 4;
        this.minimumPlayers = 2;
        this.description = "Enter your description of the map here.";
        this.sizeX = 100;
        this.sizeY = 100;
        this.editorVersion = 0;
    }

    public static string getMapsPath()
    {
        return Application.persistentDataPath + "/maps/";
    }
}