using UnityEngine;
using System.Collections;

/*
 * Base class for all objects that can appear and interact on the map(Mobs, towers etc.)
 * This does not include control objects, Tiles, GUI, special effects and so on.
 * */

public abstract class MapObject : MonoBehaviour {
    public string displayName; //Name displayed in-game
    public MapManager map;

    public virtual void init(MapManager map)
    {
        this.map = map;
    }

}
