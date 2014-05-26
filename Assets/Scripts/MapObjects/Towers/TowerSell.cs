

//Temporary used to sell placed towers
//TODO: Move over to a dedicated system

using UnityEngine;
public class TowerSell : TowerBase
{

    public override string getName()
    {
        return displayName;
    }

    public override string getDescription()
    {
        return "Temporary way to sell towers.\nTODO: Hit your nearest developer untill he develops\na proper sell function!";
    }

    public override uint getPrice()
    {
        return 0;
    }

    public override UnityEngine.Sprite getMenuSprite()
    {
        return GetComponent<SpriteRenderer>().sprite;
    }

    public override UnityEngine.Sprite getPlacementSprite()
    {
        return GetComponent<SpriteRenderer>().sprite;
    }

    public override int getSize()
    {
        return 1;
    }

    public override void showRange(bool show)
    {
        throw new System.NotImplementedException();
    }

    public override bool getIsTowerBuyVisible()
    {
        return true;
    }

    public override bool getIsTowerLocked()
    {
        return false;
    }

    public override string getLockReason()
    {
        throw new System.NotImplementedException();
    }

    public override bool canSell()
    {
        throw new System.NotImplementedException();
    }

    public override float sellValue()
    {
        throw new System.NotImplementedException();
    }
}