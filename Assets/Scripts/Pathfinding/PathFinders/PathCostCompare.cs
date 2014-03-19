using System;
using System.Collections.Generic;

//Compares the move cost of two nodes

public class PathCostCompare : IComparer<PathNodeInfo>
{
    public int Compare(PathNodeInfo tile1, PathNodeInfo tile2)
    {
        if (tile1.cost < tile2.cost)
            return -1;
        else if (tile1.cost == tile2.cost)
        {
            if (tile1 == tile2)
                return 0;
            //Can not return 0 for SortedList, as that would cause an exception
            //So we must sort them somehow
            if (tile1.node.getNodeID() > tile2.node.getNodeID())
                return 1;
            else return -1;
        }
        else
            return 1;

    }
}
