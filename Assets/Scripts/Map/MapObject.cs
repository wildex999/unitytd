using UnityEngine;
using System.Collections;

/*
 * Base class for all objects that can appear and interact on the map(Mobs, towers etc.)
 * This does not include control objects, Tiles, GUI, special effects and so on.
 * */

public abstract class MapObject : MonoBehaviour {
    public string displayName; //Name displayed in-game
    MapTile currentTile; //The current tile the Object is at(Null for moving objects)

    public virtual void setTile(MapTile tile)
    {
        currentTile = tile;
    }

    public virtual MapTile getTile()
    {
        return currentTile;
    }

}
