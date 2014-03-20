using UnityEngine;
using System.Collections;
using System;

public class Monster : MapObject, IPathable {

    protected MonsterMoveType moveType = MonsterMoveType.Walking;
    protected PathFinder currentPath = null;
    protected MapTile currentNode = null; //Node we were on previously
    protected MapTile nextNode = null; //Node we are moving towards

    protected float speed = 1f;

    public virtual int getMoveCost(IPathNode from, IPathNode to)
    {
        MapTile tileTo = (MapTile)to;
        if (!tileTo.canMonsterPass(this))
            return Int32.MaxValue;
        return 1;
    }

    public virtual MonsterMoveType getMoveType()
    {
        return moveType;
    }

    public void setPath(PathFinder path)
    {
        currentPath = path;
        currentNode = null;
    }
    //A helper function to follow the path towards the goal
    //Takes care of movement and rotation
    public void followPath()
    {
        if (currentPath == null)
        {
            Debug.Log("No path!");
            return;
        }
        if(currentNode == null)
        {
            //Get node for current position and get next node
            currentNode = map.getTileWorld(transform.position.x, transform.position.y);
            if(currentNode == null)
            {
                Debug.Log("No node available for position: " + transform.position.x + " | " + transform.position.y);
                return;
            }
            nextNode = (MapTile)currentPath.getPathNext(currentNode);

            if(nextNode == null)
            {
                Debug.Log("Got null nextNode!");
                return;
            }
        }

        //How much will we move this step
        float nextVelocity = Time.deltaTime * speed;

        //Check how close we are to nextNode and see if we are to move on to the next node
        Vector3 direction = nextNode.transform.position - transform.position;
        if (direction.sqrMagnitude <= (nextVelocity * nextVelocity))
        {
            currentNode = nextNode;
            nextNode = (MapTile)currentPath.getPathNext(currentNode);
            //Set our position to currentNode and don't move this frame. Thus avoiding bypassing the node
            transform.position = currentNode.transform.position;
            if(nextNode == null) //We have reached the end
                Destroy(gameObject);
        }
        else
        {
            //Move towards next node
            transform.position += (direction.normalized * nextVelocity);
            //Rotate towards nextNode
            //TODO
        }

    }
}
