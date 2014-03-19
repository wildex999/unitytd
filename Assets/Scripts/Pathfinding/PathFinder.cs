

/*
 * Base class for pathfinding algorithms
 * */

public abstract class PathFinder
{
    //Initialize PathFinder
    //target: The goal node
    //pathObj: The object traveling the path(Or an representative for a group of objects)(Used to calculate path cost)
    public abstract bool init(IPathNode target, IPathable pathObj, MapManager map);

    //Do path calculation(Or recalculation)
    public abstract bool calculatePath();

    //Get next node from source to target
    public abstract IPathNode getPathNext(IPathNode source);
}