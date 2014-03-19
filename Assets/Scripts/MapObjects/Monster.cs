using UnityEngine;
using System.Collections;

public class Monster : MapObject, IPathable {

    private MonsterMoveType moveType;

    public int getMoveCost(IPathNode from, IPathNode to)
    {
        return 0;
    }

    public int maxMovePoints()
    {
        return 0;
    }

    public virtual MonsterMoveType getMoveType()
    {
        return moveType;
    }
}
