using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Monster : MapObject, IPathable {

    protected MonsterMoveType moveType = MonsterMoveType.Walking;
    protected PathFinder currentPath = null;
    protected MapTile currentNode = null; //Node we were on previously
    protected MapTile nextNode = null; //Node we are moving towards

    public float speed = 1f;
    public int health = 1;

    public static LinkedList<Monster> monsters = new LinkedList<Monster>();
    private LinkedListNode<Monster> listNode;

    public Monster()
    {
        //Add to list of active monsters
        listNode = monsters.AddLast(this);
    }

    public void OnDisable()
    {
        if(listNode != null)
            monsters.Remove(listNode);
    }

    //Caled when hit by a projectile
    public virtual void onHit(Projectile projectile)
    {
        health -= projectile.getDamage();
        if (health <= 0)
        {
            GA.API.Design.NewEvent("ALPHA1:GAME:MonsterKilled", transform.position);
            Destroy(gameObject);
        }
    }

    //Add a effect on this monster(Slow, Freeze, Rampage etc.)
    public virtual void addEffect()
    {
        //TODO: Implements effects
        //TODO: Implement imunities
    }

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

    public PathFinder getPath()
    {
        return currentPath;
    }

    //Used during placing of towers to ensure no tower blocks the path of a monster
    public MapTile getNextNode()
    {
        return nextNode;
    }

    //A helper function to follow the path towards the goal
    //Takes care of movement and rotation
    public void followPath()
    {
        if (currentPath == null)
        {
            Debug.LogError("No path!");
            return;
        }
        if(currentNode == null)
        {
            //Get node for current position and get next node
            currentNode = map.getTileWorld(transform.position.x, transform.position.y);
            if(currentNode == null)
            {
                Debug.LogError("No node available for position: " + transform.position.x + " | " + transform.position.y);
                return;
            }
            nextNode = (MapTile)currentPath.getPathNext(currentNode);

            if(nextNode == null)
            {
                Debug.LogError("Got null nextNode!");
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
            if (nextNode == currentNode) //We have reached the end
            {
                GA.API.Design.NewEvent("ALPHA1:GAME:MonsterReachedCastle");
                Destroy(gameObject);
            }
            //Debug.Log("Moving towards: " + nextNode.tileX + " | " + nextNode.tileY);
        }
        else
        {
            //Move towards next node
            Vector3 directionNormalized = direction.normalized;
            transform.position += (directionNormalized * nextVelocity);
            //Rotate towards nextNode
            float angle = Mathf.Atan2(directionNormalized.y, directionNormalized.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
        }

    }
}
