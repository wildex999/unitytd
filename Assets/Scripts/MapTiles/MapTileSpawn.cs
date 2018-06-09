using UnityEngine;
using System.Collections;

//This tile will create a MobSpawn on top of itself

public class MapTileSpawn : MapTile
{
    public override void init(MapManager map, int x, int y)
    {
        base.init(map, x, y);

        //Create MobSpawner
        GameObject obj = new GameObject();
        MobSpawn spawn = obj.AddComponent<MobSpawn>();
        if(map != null)
            map.addObject(spawn, new FVector2(x*(int)MapBase.unitSizeFixed, y*(int)MapBase.unitSizeFixed));
    }
}
