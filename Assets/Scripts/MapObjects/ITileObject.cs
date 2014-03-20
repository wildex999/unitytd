using UnityEngine;


/*
 * ITileObject is implemented by any object that can be placed on a tile(Exlusively)
 * */


public interface ITileObject
{
    void setTile(MapTile tile); //Set the tile this object is on
    MapTile getTile();
    GameObject getGameObject();
}

