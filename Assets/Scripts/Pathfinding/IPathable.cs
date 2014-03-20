//An object implementing this can be used in pathfinding

public interface IPathable
{
    int getMoveCost(IPathNode from, IPathNode to);
}