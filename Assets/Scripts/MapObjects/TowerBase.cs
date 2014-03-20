

using UnityEngine;
public abstract class TowerBase : Building, ITileObject
{
    protected MapTile currentTile;

    public abstract string getName(); //Name of tower, used in tower menu and when showing tower info
    public abstract string getDescription(); //Description, same as name
    public abstract uint getPrice(); //Cost of the tower

    public abstract Sprite getMenuSprite(); //Sprite to show while in the menu
    public abstract Sprite getPlacementSprite(); //Sprite to show while placing tower

    public abstract Vector2 getSize(); //The size of the tower in number of tiles(X and Y)

    public abstract void showRange(bool show); //Render the range indicator for the tower

    public abstract bool getIsTowerBuyVisible(); //Is the tower visible in the buy menu(Allows for hidden towers that are only visible depending on conditions)
    public abstract bool getIsTowerLocked(); //Is the tower visible, but locked
    public abstract string getLockReason(); //Return an explanation on why the tower is locked, and how to unlock it

    public abstract bool canSell(); //Whether or not the tower can be sold
    public abstract float sellValue(); //Percentage(0 to 1) of max price is returned on sell

    //TODO: Upgrade options 

    //Tile
    public GameObject getGameObject()
    {
        return gameObject;
    }

    public MapTile getTile()
    {
        return currentTile;
    }

    public void setTile(MapTile tile)
    {
        currentTile = tile;
    }
}