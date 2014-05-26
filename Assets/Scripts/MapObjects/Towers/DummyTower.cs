//A dummy tower placed in network games whenever a client places a tower to lessen apparent lag.
//This will have the sprite and position of the real tower, but will not block the monsters, fire upon them or have any other effect.
//When the server replies either with failure, or with a tower placement, this Dummy is replaced.
//TODO: Show tower with 0.5 alpha transparency

using UnityEngine;
public class DummyTower : TowerBase
{
    private TowerBase original;

    public void setOriginal(TowerBase tower)
    {
        original = tower;
        SpriteRenderer rend = this.GetComponent<SpriteRenderer>();
        rend.sprite = tower.GetComponent<SpriteRenderer>().sprite;
        rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, 0.3f);
        transform.localScale = tower.transform.localScale;
    }

    public void OnDestroy()
    {
        Debug.Log("DUMMY DESTROYED");
    }

    //Make sure the dummy doesn't affect the pathfinding
    public override bool canMonsterPass(Monster mob)
    {
        return true;
    }

    public override string getName()
    {
        return original.getName();
    }

    public override string getDescription()
    {
        return original.getDescription();
    }

    public override int getPrice()
    {
        return 0;
    }

    public override UnityEngine.Sprite getMenuSprite()
    {
        throw new System.NotImplementedException();
    }

    public override UnityEngine.Sprite getPlacementSprite()
    {
        return original.getPlacementSprite();
    }

    public override int getSize()
    {
        return original.getSize();
    }

    public override void showRange(bool show)
    {
        throw new System.NotImplementedException();
    }

    public override bool getIsTowerBuyVisible()
    {
        throw new System.NotImplementedException();
    }

    public override bool getIsTowerLocked()
    {
        throw new System.NotImplementedException();
    }

    public override string getLockReason()
    {
        throw new System.NotImplementedException();
    }

    public override bool canSell()
    {
        return false;
    }

    public override FInt sellValue()
    {
        return 0;
    }

    public override int getSyncHash()
    {
        return 0;
    }
}
