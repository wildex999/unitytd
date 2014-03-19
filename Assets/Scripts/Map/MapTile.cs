using UnityEngine;
using System.Collections;

/*
 * A MapTile is a object defining non-interactive tiles, usually ground, cliff etc.
 * Tiles should not stack or overlap(Use objects for that), so for each grid tile there is a MapTile.
 * The Tiles can have an effect on any MapObject traveling on them, but can themself not be affected by the player
 * (Not Entirely true, tiles can be effect by certain events or actions on the map, I.e, dynamic maps)
 * 
 * Each MapTile can have one MapObject placed on it, only static MapObject are placed on tiles,
 * for example towers. 
 * Moving MapObjects like mobs are not marked as placed on the tile.
 * */

public class MapTile : MonoBehaviour {

    public static int tileSize = 64; //A tile is 64x64 pixels
    private MapObject mapObj; //The map object placed on this tile(I.e a tower)

	// Use this for initialization
	void Start () {
	
	}
}
