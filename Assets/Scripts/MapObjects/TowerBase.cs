

using UnityEngine;
public abstract class TowerBase : Building, ITileObject
{
    protected TileGroup tileGroup;
    protected Monster currentTarget;
    protected GameObject currentTargetObj; //Store GameObject to avoid GetCompontent calls

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

    public virtual bool canMonsterPass(Monster mob)
    {
        if (mob.getMoveType() == MonsterMoveType.Walking)
            return false;
        return true;
    }

    //Tile
    public GameObject getGameObject()
    {
        return gameObject;
    }

    public TileGroup getTileGroup()
    {
        return tileGroup;
    }

    //Projectile
    //Create a new(copy) projectile from base
    public Projectile createProjectile(GameObject baseObj)
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

        return proj;
    }


    //Detection

    //The class inheriting from this class can overide these functions by redefining them
    void OnTriggerStay2D(Collider2D other)
    {
        //TODO: Implement different targeting:
        //Closest: Check every object in range if closest(DOn't to it every frame, once a seocnd?)
        //Front: Target object that is closest to the end(I.e at the front), check the cost of the current node they are walking towards.
        //Weakest
        //Strongest
        //Triggers run before update, so it's possible to gather a list of monsters in range, and decide.
        //For now we just target the first one to enter our range since previous target went out
        if (currentTarget == null)
        {
            currentTarget = other.gameObject.GetComponent<Monster>();
            //If the other object is not of type Monster, currentTarget will be set to null
            //TODO: Use layers to reduce tower->tower false positives?
            if (currentTarget != null)
                currentTargetObj = other.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == currentTargetObj)
        {
            currentTarget = null;
            currentTargetObj = null;
        }
    }
}