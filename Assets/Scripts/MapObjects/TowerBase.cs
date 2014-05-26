

using System.Collections.Generic;
using System.IO;
using UnityEngine;
public abstract class TowerBase : Building, ITileObject, ICollideHandler
{
    protected TileGroup tileGroup;
    protected Monster currentTarget;
    protected GameObject currentTargetObj; //Store GameObject to avoid GetCompontent calls

    public FixedCircleCollider fixedCollider;
    public int colliderRadius = (int)MapBase.unitSizeFixed;

    public abstract string getName(); //Name of tower, used in tower menu and when showing tower info
    public abstract string getDescription(); //Description, same as name
    public abstract uint getPrice(); //Cost of the tower

    public abstract Sprite getMenuSprite(); //Sprite to show while in the menu
    public abstract Sprite getPlacementSprite(); //Sprite to show while placing tower

    public abstract int getSize(); //The size of the tower in number of tiles

    public abstract void showRange(bool show); //Render the range indicator for the tower

    public abstract bool getIsTowerBuyVisible(); //Is the tower visible in the buy menu(Allows for hidden towers that are only visible depending on conditions)
    public abstract bool getIsTowerLocked(); //Is the tower visible, but locked
    public abstract string getLockReason(); //Return an explanation on why the tower is locked, and how to unlock it

    public abstract bool canSell(); //Whether or not the tower can be sold
    public abstract float sellValue(); //Percentage(0 to 1) of max price is returned on sell

    //TODO: Upgrade options 

    public TowerBase()
    {
        tileGroup = new TileGroup(this);
    }

    public override void OnCreate()
    {
        updateCollider();
    }

    public override void OnRemove()
    {

    }

    protected virtual void updateCollider()
    {
        fixedCollider = new FixedCircleCollider(this, colliderRadius);
    }

    public virtual bool canMonsterPass(Monster mob)
    {
        if (mob.getMoveType() == MonsterMoveType.Walking)
            return false;
        return true;
    }

    //Tile
    public TileGroup getTileGroup()
    {
        return tileGroup;
    }

    //Projectile
    //Create a new(copy) projectile from base
    public Projectile createProjectile(GameObject baseObj, FVector2 fixedPosition)
    {
        //TODO: Object pool
        if (baseObj == null)
        {
            Debug.LogError("Got null baseObj from projectile");
            return null;
        }

        GameObject obj = (GameObject)Instantiate(baseObj);
        if(obj == null)
        {
            Debug.LogError("Failed to Instantiate projectile object: " + baseObj);
            return null;
        }

        Projectile proj = obj.GetComponent<Projectile>();
        if(proj == null)
        {
            Debug.LogError("Failed to get Projectile script from object: " + obj);
            return null;
        }

        map.addObject(proj, fixedPosition);

        return proj;
    }


    //Detection
    public virtual void handleCollisionEnter(FixedCollider other)
    {
    }

    public virtual void handleCollisionContinue(FixedCollider other)
    {
        //TODO: Implement different targeting:
        //Closest: Check every object in range if closest(Don't to it every frame, once a seocnd?)
        //Front: Target object that is closest to the end(I.e at the front), check the cost of the current node they are walking towards.
        //Weakest
        //Strongest
        //Triggers run before update, so it's possible to gather a list of monsters in range, and decide.
        //For now we just target the first one to enter our range since previous target went out
        if (currentTarget == null)
        {
            currentTarget = other.handler.getGameObject().GetComponent<Monster>();
            //If the other object is not of type Monster, currentTarget will be set to null
            if (currentTarget != null)
                currentTargetObj = other.handler.getGameObject();
        }
    }

    public virtual void handleCollisionExit(FixedCollider other)
    {
        if (!other.handler.isValid() || other.handler.getGameObject() == currentTargetObj)
        {
            currentTarget = null;
            currentTargetObj = null;
        }
    }

    public bool isValid()
    {
        if (gameObject == null)
            return false;
        return true;
    }

    //Place a tower of the given prefab, on the given tile(s) and return a copy
    public virtual T createTower<T>(MapTile tile, T prefab) where T : TowerBase
    {
        MapManager map = tile.getMapManager();
        T newTower = (T)MapBase.createObject(prefab.gameObject);
        newTower.getTileGroup().setGroup(tile, prefab.getSize());

        //Recalculate all paths
        map.calculatePaths(MonsterMoveType.Unknown);

        return newTower;
    }

    //Remove the current tower
    public virtual void removeTower()
    {
        Destroy(gameObject);
        getTileGroup().removeFromGroup();

        //Recalculate all paths
        map.calculatePaths(MonsterMoveType.Unknown);
    }

    //Check if space is free for tower to be built
    public virtual bool canBuild(MapTile tile)
    {
        int towerSize = getSize();
        MapManager map = tile.getMapManager();

        for (int x = 0; x < towerSize; x++)
        {
            for (int y = 0; y < towerSize; y++)
            {
                MapTile checkTile = map.getTile(tile.tileX + x, tile.tileY + y);
                if (checkTile == null)
                    return false;

                if (!checkTile.canBuild(this) || checkTile.getMapObject() != null)
                    return false;
            }
        }
        return true;
    }

}