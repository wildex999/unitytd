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
    //private Dictionary<string, PathNodeInfo> nodes; //Store node path info after calculations are complete
    private List<List<PathNodeInfo>> nodes;
    //private SortedList<PathNodeInfo, PathNodeInfo> openList; //Use this as a priority queue, sorted by cost
    private PriorityQueue openList;

    private PathNodeInfo target;
    private IPathable pathObj;
    private MapManager map;
    private uint age; //Used during recalculation

    public override bool init(IPathNode target, IPathable pathObj, MapManager map)
    {
        //nodes = new Dictionary<string, PathNodeInfo>(); //stores with the string( x + "-" + y, I.e 10-5)
        nodes = new List<List<PathNodeInfo>>();

        //Initialise nodes for map size
        int sizeX, sizeY;
        map.getSize(out sizeX, out sizeY);
        for (int y = 0; y < sizeY; y++)
        {
            List<PathNodeInfo> listY = new List<PathNodeInfo>();
            nodes.Add(listY);
            for(int x = 0; x < sizeX; x++)
                listY.Add(null);
        }

            //openList = new SortedList<PathNodeInfo, PathNodeInfo>(new PathCostCompare());
            openList = new PriorityQueue();

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
        PathNodeInfo node = nodes[y][x];
        if (node == null && createIfNotFound)
        {
            IPathNode pathNode = map.getTile(x, y);
            if (pathNode == null)
                return null;
            node = new PathNodeInfo(pathNode);
            node.cost = -1;
            node.visited = false;
            node.age = age;
            nodes[y][x] = node;
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

    public override PathNodeInfo getNodeInfo(IPathNode node)
    {
        return getNode(node.getNodeX(), node.getNodeY());
    }

    //Calculate cost values from all sources to target.
    public override bool calculatePath()
    {
        age++;

        //Prepare
        openList.push(target);
        
        if(getNode(target.node.getNodeX(), target.node.getNodeY()) == null)
        {
            //Make sure the target is in the nodes list
            //nodes.Add(target.node.getNodeX() + "-" + target.node.getNodeY(), target);
            nodes[target.node.getNodeY()][target.node.getNodeX()] = target;
            target.age = age;
        }
        target.cost = 0; //Target tile always has a cost of 0
        target.visited = false;

        Stopwatch watch = new Stopwatch();
        watch.Start();


        long totalTicks = watch.ElapsedTicks;
        /*long ticks = 0;
        long pushTicks = 0;
        long popTicks = 0;
        long repositionTicks = 0;
        long neighbourTicks = 0;

        UnityEngine.Debug.Log("HighPerformanceTimer: " + Stopwatch.IsHighResolution);*/

        //Run algorithm
        //Get first from sorted Open List, put into Closed list
        //Check every neighbour(calculate cost) and add them to Open list if within range
        //Repeat
        PathNodeInfo currentNode = null;
        int nodesFound = 0;
        while (openList.Count != 0)
        {
            //ticks = watch.ElapsedTicks;
            currentNode = openList.pop();
            //popTicks += watch.ElapsedTicks - ticks;

            nodesFound++;
            currentNode.visited = true;

            //Neighbours
            //ticks = watch.ElapsedTicks;
            List<PathNodeInfo> neighbours = getNeighbours(currentNode, true);
            //neighbourTicks += watch.ElapsedTicks - ticks;

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
                    //ticks = watch.ElapsedTicks;
                    openList.push(tile);
                    //pushTicks += watch.ElapsedTicks - ticks;
                }
                else if (newCost < tile.cost)
                {
                    //If this cost is lower, remove and re-add to get new sorting with correct cost
                    tile.cost = newCost;
                    //ticks = watch.ElapsedTicks;
                    openList.remove(tile);
                    openList.push(tile);
                    //repositionTicks += watch.ElapsedTicks - ticks;
                }
            }
        }

        totalTicks = watch.ElapsedTicks - totalTicks;
        watch.Stop();
        GA.API.Design.NewEvent("ALPHA1:PERF:PathFindTime", watch.ElapsedMilliseconds);

        UnityEngine.Debug.Log("Done calculating possible movement: " + (nodesFound) + " and used " + watch.ElapsedMilliseconds + " ms");
        UnityEngine.Debug.Log("Ticks: " + totalTicks + "(" + (float)totalTicks / (float)Stopwatch.Frequency + " seconds)");
        //UnityEngine.Debug.Log("Pop: " + popTicks + " Push: " + pushTicks + " Reposition: " + repositionTicks + " Neightbour: " + neighbourTicks);
        //UnityEngine.Debug.Log("TotalTicks: " + totalTicks + "(" + (popTicks+pushTicks+repositionTicks+neighbourTicks) + ")");

        return true;
    }

    public override IPathNode getPathNext(IPathNode source)
    {
        PathNodeInfo node = getNode(source.getNodeX(), source.getNodeY());
        if (node == null || node.cost == -1)
        {
            UnityEngine.Debug.Log("Next node null or cost -1: " + node);
            return null;
        }
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
