//A dummy tower placed in network games whenever a client places a tower to lessen apparent lag.
//This will have the sprite and position of the real tower, but will not block the monsters, fire upon them or have any other effect.
//When the server replies either with failure, or with a tower placement, this Dummy is replaced.
//TODO: Show tower with 0.5 alpha transparency

public class DummyTower : TowerBase
{
    private TowerBase original;

    public void setOriginal(TowerBase tower)
    {
        original = tower;
    }

    public override string getName()
    {
        return original.getName();
    }

    public override string getDescription()
    {
        return original.getDescription();
    }

    public override uint getPrice()
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

    public override float sellValue()
    {
        return 0;
    }
}
