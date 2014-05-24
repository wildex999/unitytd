using UnityEngine;
using System.Collections;
using System.IO;

/*
 * A MapTile is a object defining non-interactive tiles, usually ground, cliff etc.
 * Tiles should not stack or overlap(Use objects for that), so for each grid tile there is a single MapTile.
 * The Tiles can have an effect on any MapObject traveling on them, but can themself not be affected by the player
 * (Not Entirely true, tiles can be effect by certain events or actions on the map, I.e, dynamic maps)
 * 
 * Each MapTile can have one MapObject placed on it, only static MapObject are placed on tiles,
 * for example towers. 
 * Moving MapObjects like mobs are not marked as placed on the tile.
 * 
 * */

public class MapTile : MonoBehaviour, IPathNode, IFixedPosition, ISerializedObject {

    public int tileX, tileY; //The tiles X and Y position on the map. Set by the MapManager when the tile is created
    public MapManager map;
    public GameObject prefab; //Used by editor when saving. Set by editor when placing

    //Allow these default values to be set in the editor for prefabs
    public bool canMonsterPassDefault; //Can monster pass on this tile(Fly/Walk)
    public int monsterPassCostDefault; //Cost of monster walking/Flying over this tile
    public bool canBuildDefault;

    public int prefabId;

    private ITileObject mapObj; //The map object placed on this tile(I.e a tower)
    private Vector2Gen<int> fixedPosition;

    public virtual void init(MapManager map, int x, int y)
    {
        this.map = map;
        tileX = x;
        tileY = y;
        //TODO: update this as TileX or TileY is changed.
        fixedPosition = new Vector2Gen<int>(tileX * (int)MapBase.unitSizeFixed, tileY * (int)MapBase.unitSizeFixed);
    }

    public ITileObject getMapObject()
    {
        return mapObj;
    }

    public MapManager getMapManager()
    {
        return map;
    }

    public virtual void setMapObject(ITileObject obj)
    {
        mapObj = obj;
    }

    //Returns whether a monster can walk/fly over this tile
    public virtual bool canMonsterPass(Monster mob)
    {
        if (mapObj == null)
            return canMonsterPassDefault;
        else
            return mapObj.canMonsterPass(mob);
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

    public void setPosition(Vector2Gen<int> position)
    {
        throw new System.NotImplementedException();
    }

    public void setParent(IFixedPosition parent)
    {
        throw new System.NotImplementedException();
    }

    public Vector2Gen<int> getFixedPosition()
    {
        //TODO: Get offset to parent if set
        return fixedPosition;
    }

    public GameObject getGameObject()
    {
        return gameObject;
    }

    public IFixedPosition getParent()
    {
        throw new System.NotImplementedException();
    }

    public void setUniqueId(int id)
    {
        prefabId = id;
    }

    public int getUniqueId()
    {
        return prefabId;
    }

    public virtual void writeToStream(BinaryWriter stream)
    {
        //A normal tile will just write it's id(Done by editor)
    }

    public virtual void readFromStream(BinaryReader stream)
    {
        //Nothing extra to read for normal tile
    }
}
