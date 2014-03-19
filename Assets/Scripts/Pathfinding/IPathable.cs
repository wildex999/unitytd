//An object implementing this can be used in pathfinding

public interface IPathable
{
    int maxMovePoints(); //Return the maximum number of 1 cost nodes it can move(I.e, max movement points)
    int getMoveCost(IPathNode from, IPathNode to);
}