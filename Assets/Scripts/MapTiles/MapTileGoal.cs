using UnityEngine;
using System.Collections;

public class MapTileGoal : MapTile
{
    public override void init(MapManager map, int x, int y)
    {
        base.init(map, x, y);
        if(map != null)
            map.setGoalTile(this);
    }
}
