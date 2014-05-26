using UnityEngine;


/*
 * ITileObject is implemented by any object that can be placed on a tile(Exlusively)
 * */


public interface ITileObject
{
    TileGroup getTileGroup();
    GameObject getGameObject();
    bool canMonsterPass(Monster mob);
    void init(MapManager map, FVector2 fixedPosition);
    void init(MapManager map, FVector2 fixedPositionLocal, IFixedPosition parent);
}

