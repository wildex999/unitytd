using UnityEngine;


/*
 * ITileObject is implemented by any object that can be placed on a tile(Exlusively)
 * */


public interface ITileObject
{
    TileGroup getTileGroup();
    GameObject getGameObject();
    bool canMonsterPass(Monster mob);
    void init(MapManager map, Vector2Gen<int> fixedPosition);
    void init(MapManager map, Vector2Gen<int> fixedPositionLocal, IFixedPosition parent);
}

