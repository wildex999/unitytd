using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private uint age; //Used during recalculation

    public override bool init(IPathNode target, IPathable pathObj, MapManager map)
    {
        nodes = new Dictionary<string, PathNodeInfo>(); //stores with the string( x + "-" + y, I.e 10-5)
        openList = new SortedList<PathNodeInfo, PathNodeInfo>(new PathCostCompare());
        calculated = false;

        this.target = new PathNodeInfo(target);
        this.pathObj = pathObj;
        this.map = map;
        age = 1;
        
        return true;
    }

    public override MapManager getMap()
    {
        return map;
    }

    //Tries to get the node from the nodes dictionary, or gets it from the map if not available(Creates new PathNodeInfo)
    public PathNodeInfo getNode(int x, int y, bool createIfNotFound = false)
    {
        PathNodeInfo node = null;
        string nodeStr = x + "-" + y;
        if (!nodes.TryGetValue(nodeStr, out node) && createIfNotFound)
        {
            IPathNode pathNode = map.getTile(x, y);
            if (pathNode == null)
                return null;
            node = new PathNodeInfo(pathNode);
            node.cost = -1;
            node.visited = false;
            node.age = age;
            nodes[nodeStr] = node;
        }
        else if(node != null)
        {
            if (node.age != age)
            {
                node.cost = -1;
                node.visited = false;
                node.age = age;
            }
        }
        return node;
    }

    //Calculate cost values from all sources to target.
    public override bool calculatePath()
    {
        age++;

        //Prepare
        openList.Add(target, null);
        target.cost = 0; //Target tile always has a cost of 0
        target.visited = false;

        Stopwatch watch = new Stopwatch();
        watch.Start();

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

            nodesFound++;
            currentNode.visited = true;

            //Neighbours
            List<PathNodeInfo> neighbours = getNeighbours(currentNode, true);

            foreach (PathNodeInfo tile in neighbours)
            {
                if (tile == null || tile.visited)
                    continue;

                int newCost = pathObj.getMoveCost(currentNode.node, tile.node);
                if (newCost == Int32.MaxValue)
                    continue;
                newCost += currentNode.cost;

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

        watch.Stop();

        UnityEngine.Debug.Log("Done calculating possible movement: " + (nodesFound) + " and used " + watch.ElapsedMilliseconds + " ms");
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
            if (currentNode == null || currentNode.cost == -1)
                continue;
            if(currentNode.cost < retCost)
            {
                retNode = currentNode;
                retCost = currentNode.cost;
            }
        }

        return retNode.node;
    }

    private List<PathNodeInfo> getNeighbours(PathNodeInfo node, bool createIfNotFound = false)
    {
        List<PathNodeInfo> neighbours = new List<PathNodeInfo>(4);
        int tileX = node.node.getNodeX();
        int tileY = node.node.getNodeY();
        neighbours.Add(getNode(tileX + 1, tileY, createIfNotFound)); //Right
        neighbours.Add(getNode(tileX - 1, tileY, createIfNotFound)); //Left
        neighbours.Add(getNode(tileX, tileY + 1, createIfNotFound)); //Up(Positive Y up or down?)
        neighbours.Add(getNode(tileX, tileY - 1, createIfNotFound)); //Down(?)
        //To include diagonals, add them to the neighbours list here

        return neighbours;
    }
}
