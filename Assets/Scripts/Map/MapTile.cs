using UnityEngine;
using System.Collections;

/*
 * A MapTile is a object defining non-interactive tiles, usually ground, cliff etc.
 * Tiles should not stack or overlap(Use objects for that), so for each grid tile there is a single MapTile.
 * The Tiles can have an effect on any MapObject traveling on them, but can themself not be affected by the player
 * (Not Entirely true, tiles can be effect by certain events or actions on the map, I.e, dynamic maps)
 * 
 * Each MapTile can have one MapObject placed on it, only static MapObject are placed on tiles,
 * for example towers. 
 * Moving MapObjects like mobs are not marked as placed on the tile.
 * */

public class MapTile : MonoBehaviour, IPathNode {

    public int tileX, tileY; //The tiles X and Y position on the map. Set by the MapManager when the tile is created

    //Allow these default values to be set in the editor for prefabs
    public bool canMonsterPassDefault; //Can monster pass on this tile(Fly/Walk)
    public int monsterPassCostDefault; //Cost of monster walking/Flying over this tile
    public bool canBuildDefault;

    private MapManager map;
    private MapObject mapObj; //The map object placed on this tile(I.e a tower)

    public void init(MapManager map, int x, int y)
    {
        this.map = map;
        tileX = x;
        tileY = y;
    }

    public MapObject getMapObject()
    {
        return mapObj;
    }

    public MapManager getMapManager()
    {
        return map;
    }

    public void setMapObject(MapObject obj)
    {
        mapObj = obj;

        if (obj == null)
            return;

        //Remove from previous tile
        MapTile prevTile = obj.getTile();
        if (prevTile != null)
        {
            if (prevTile == this)
                return;
            if (prevTile.getMapObject() == obj)
                prevTile.setMapObject(null);
        }

        //Move the object to the tile
        obj.transform.parent = this.transform;
        obj.transform.localPosition = new Vector2(0f, 0f);
        obj.setTile(this);
    }

    //Returns whether a monster can walk/fly over this tile
    public virtual bool canMonsterPass(Monster mob)
    {
        return canMonsterPassDefault;
    }

    //Returns whether you can build on this tile
    public virtual bool canBuild(Building build)
    {
        return canBuildDefault;
    }

    //PathFinder
    int IPathNode.getNodeX()
    {
        return tileX;
    }

    //PathFinder
    int IPathNode.getNodeY()
    {
        return tileY;
    }

    //PathFinder: get globally Unique ID for this object
    int IPathNode.getNodeID()
    {
        return this.GetInstanceID();
    }
}
