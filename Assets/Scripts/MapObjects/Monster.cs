using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading;

public class Monster : MapObject, IPathable, ICollideHandler {

    protected MonsterMoveType moveType = MonsterMoveType.Walking;
    protected PathFinder currentPath = null;
    protected MapTile currentNode = null; //Node we were on previously
    protected MapTile nextNode = null; //Node we are moving towards
    protected bool isDead = false;

    //Fixed simulation(Deterministic)
    protected FixedCircleCollider fixedCollider = null; //Collider used to check if within range of towers

    public uint speed = 10; //10 = 1 tile a second(200 = 20 tiles a second)(Sim fps = 20)
    public int health = 1;
    public int colliderRadius = 1;

    public static LinkedList<Monster> monsters = new LinkedList<Monster>();
    private LinkedListNode<Monster> listNode;

    public override void OnCreate()
    {
        fixedCollider = new FixedCircleCollider(this, colliderRadius);
        map.collisionManager.addUnit(fixedCollider);

        //Add to list of active monsters
        listNode = monsters.AddLast(this);

        //Debug.Log("Monster init on step: " + map.getGameManager().getCurrentFixedStep() + "\n" + Environment.StackTrace);
    }

    public override void OnRemove()
    {
        isDead = true;
        if(listNode != null)
            monsters.Remove(listNode);
        if(fixedCollider != null)
            map.collisionManager.removeUnit(fixedCollider);
    }

    //Caled when hit by a projectile
    public virtual void onHit(Projectile projectile)
    {
        health -= projectile.getDamage();
        if (health <= 0)
        {
            GA.API.Design.NewEvent("ALPHA1:GAME:MonsterKilled", transform.position);
            onKilled();
            Destroy(gameObject);
        }
    }

    public virtual void onKilled()
    {
        map.getGameManager().Money += 1;
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

    //Get next node if not set, and put it at currentPath
    public void getNode()
    {
        if (currentPath == null)
        {
            Debug.LogError("No path!");
            return;
        }
        if (currentNode == null)
        {
            //Get node for current position and get next node
            currentNode = map.getTileWorld(fixedPosition);
            if (currentNode == null)
            {
                Debug.LogError("No node available for position: " + fixedPosition.x + " | " + fixedPosition.y);
                return;
            }
            nextNode = (MapTile)currentPath.getPathNext(currentNode);

            if (nextNode == null)
            {
                Debug.LogError("Got null nextNode!");
                return;
            }
        }
    }

    //A helper function to follow the path towards the goal
    //For fixed simulation
    public void followPathFixed()
    {
        getNode();
        if(nextNode == null)
            return;

        //TODO: Allow for position/move offset(I a bit to the top and left of middle path) Allowing smaller monsters to move beside each other
        //Move in direction of node

        int movementRemain = (int)speed;

        //TODO: Allow more than 4 directions of movement at a time
        //Check move direction
        while (movementRemain > 0)
        {
            FVector2 nodePos = nextNode.getFixedPosition();
            int xDiff = (int)(nodePos.x - fixedPosition.x);
            int yDiff = (int)(nodePos.y - fixedPosition.y);

            if (xDiff != 0)
            {
                if (xDiff * xDiff > movementRemain * movementRemain)
                {
                    //Limit to speed
                    if (xDiff > 0)
                        xDiff = (int)movementRemain;
                    else
                        xDiff = (int)-movementRemain;
                }
                fixedPosition = new FVector2(fixedPosition.x + xDiff, fixedPosition.y);
                if (xDiff > 0)
                    movementRemain -= xDiff;
                else
                    movementRemain += xDiff;
            }
            else if (yDiff != 0)
            {
                if (yDiff * yDiff > movementRemain * movementRemain)
                {
                    if (yDiff > 0)
                        yDiff = (int)movementRemain;
                    else
                        yDiff = (int)-movementRemain;
                }
                fixedPosition = new FVector2(fixedPosition.x, fixedPosition.y + yDiff);
                if (yDiff > 0)
                    movementRemain -= yDiff;
                else
                    movementRemain += yDiff;
            }

            if (nodePos.x == fixedPosition.x && nodePos.y == fixedPosition.y)
            {
                //Get next node
                currentNode = nextNode;
                nextNode = (MapTile)currentPath.getPathNext(currentNode);
                if (nextNode == currentNode)
                {
                    //Reached goal
                    movementRemain = 0;
                    map.getGameManager().Lives -= 1; //TODO: Allow to define how much a mob is worth(Can be dynamic depending on health)
                    GA.API.Design.NewEvent("ALPHA1:GAME:MonsterReachedCastle");
                    Destroy(gameObject);
                }
            }
        }

        //Set local postion to fixed position(Sync)
        transform.localPosition = getPosition(true);
    }

    //Make local sprite follow path
    public void followPathLocal()
    {
        if (nextNode == null)
            return;
        //How much will we move this step
        float nextVelocity = Time.deltaTime * (speed/(float)MapBase.unitSizeFixed) * MapBase.simFramerate;

        //Check how close we are to nextNode and see if we are to move on to the next node
        Vector3 direction = nextNode.transform.position - transform.position;
        if (direction.sqrMagnitude <= (nextVelocity * nextVelocity))
        {
            transform.position = nextNode.transform.position;
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

    //Collision handlers
    public virtual void handleCollisionEnter(FixedCollider other)
    {
        throw new NotImplementedException();
    }

    public virtual void handleCollisionContinue(FixedCollider other)
    {
        throw new NotImplementedException();
    }

    public virtual void handleCollisionExit(FixedCollider other)
    {
        throw new NotImplementedException();
    }

    public bool isValid()
    {
        return !isDead;
    }
}
