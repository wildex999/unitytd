using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * Uses Dijkstra's algorithm to calculate paths from all sources to one target.
 * 4 Directions(No diagonal)
 * */

public class PFDijkstra4Dir : PathFinder
{
    private Dictionary<string, PathNodeInfo> nodes; //Store node path info after calculations are complete
    private SortedList<PathNodeInfo, PathNodeInfo> openList; //Use this as a priority queue, sorted by cost

    private PathNodeInfo target;
    private IPathable pathObj;
    private bool calculated;
    private MapManager map;

    public override bool init(IPathNode target, IPathable pathObj, MapManager map)
    {
        nodes = new Dictionary<string, PathNodeInfo>(); //stores with the string( x + "-" + y, I.e 10-5)
        openList = new SortedList<PathNodeInfo, PathNodeInfo>(new PathCostCompare());
        calculated = false;

        this.target = new PathNodeInfo(target);
        this.pathObj = pathObj;
        this.map = map;
        
        return true;
    }

    //Tries to get the node from the nodes dictionary, or gets it from the map if not available(Creates new PathNodeInfo)
    public PathNodeInfo getNode(int x, int y, bool createIfNotFound = false)
    {
        PathNodeInfo node = null;
        if(!nodes.TryGetValue(x + "-" + y, out node) && createIfNotFound)
        {
            IPathNode pathNode = map.getTile(x, y);
            if (pathNode == null)
                return null;
            node = new PathNodeInfo(pathNode);
        }
        return node;
    }

    //Calculate cost values from all sources to target.
    public override bool calculatePath()
    {
        //Prepare
        openList.Add(target, null);
        int objMovePoints = pathObj.maxMovePoints();

        //Go through and mark every tile as unvisited(For now just check a square round the obj, optimized would be a circle)
        objMovePoints++; //Add a border of 1 around
        for (int y = 0; y < objMovePoints * 2; y++)
        {
            for (int x = 0; x < objMovePoints * 2; x++)
            {
                //TODO: Check if it already exists in nodes dictionary(Avoids creating a new PathNodeInfo)
                PathNodeInfo newNodeInfo = getNode((target.node.getNodeX() - objMovePoints) + x, (target.node.getNodeY() - objMovePoints) + y, true);
                if (newNodeInfo == null)
                    continue;
                newNodeInfo.cost = -1;
                newNodeInfo.visited = false;
            }
        }
        objMovePoints--; //Remove border

        target.cost = 0; //Target tile always has a cost of 0


        //Run algorithm
        //Get first from sorted Open List, put into Closed list
        //Check every neighbour(calculate cost) and add them to Open list if within range
        //Repeat
        PathNodeInfo currentNode = null;
        int nodesFound = 0;
        while (openList.Count != 0)
        {
            currentNode = openList.Keys[0]; //We use Keys instead of Values
            openList.Remove(currentNode);

            if (currentNode.cost > objMovePoints)
                continue;

            nodesFound++;
            currentNode.visited = true;

            //Neighbours
            List<PathNodeInfo> neighbours = getNeighbours(currentNode);

            foreach (PathNodeInfo tile in neighbours)
            {
                if (tile == null || tile.visited)
                    continue;

                int newCost = currentNode.cost + pathObj.getMoveCost(currentNode.node, tile.node);

                if (tile.cost == -1)
                {
                    //If new
                    tile.cost = newCost;
                    openList.Add(tile, null);
                }
                else if (newCost < tile.cost)
                {
                    //If this cost is lower, remove and re-add to get new sorting with correct cost
                    tile.cost = newCost;
                    openList.Remove(tile);
                    openList.Add(tile, null);
                }
            }
        }

        Debug.Log("Done calculating possible movement: " + (nodesFound));
        return true;
    }

    public override IPathNode getPathNext(IPathNode source)
    {
        PathNodeInfo node = getNode(source.getNodeX(), source.getNodeY());
        if (node == null || node.cost == -1)
            return null;
        if (node.cost == 0)
            return source; //At target
        //Return the neightbour node with least cost
        List<PathNodeInfo> neighbours = getNeighbours(node);
        PathNodeInfo retNode = null;
        int retCost = Int32.MaxValue;
        foreach(PathNodeInfo currentNode in neighbours)
        {
            if(currentNode.cost < retCost)
            {
                retNode = currentNode;
                retCost = currentNode.cost;
            }
        }

        return retNode.node;
    }

    private List<PathNodeInfo> getNeighbours(PathNodeInfo node)
    {
        List<PathNodeInfo> neighbours = new List<PathNodeInfo>(4);
        int tileX = node.node.getNodeX();
        int tileY = node.node.getNodeY();
        neighbours.Add(getNode(tileX + 1, tileY)); //Right
        neighbours.Add(getNode(tileX - 1, tileY)); //Left
        neighbours.Add(getNode(tileX, tileY + 1)); //Up(Positive Y up or down?)
        neighbours.Add(getNode(tileX, tileY - 1)); //Down(?)
        //To include diagonals, add them to the neighbours list here

        return neighbours;
    }
}
