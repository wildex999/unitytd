﻿
using UnityEngine;
public class TestTower : TowerBase
{
    private static Vector2 size = new Vector2(1,1);

    public override string getName()
    {
        return displayName;
    }

    public override string getDescription()
    {
        return "Your everyday basic Test Tower! Free air included with every 10th purchase.";
    }

    public override uint getPrice()
    {
        return 10;
    }

    public override UnityEngine.Vector2 getSize()
    {
        return size;
    }

    public override void showRange(bool show)
    {
        //TODO
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
        return "";
    }

    public override bool canSell()
    {
        return true;
    }

    public override float sellValue()
    {
        return 0.5f;
    }

    public override Sprite getMenuSprite()
    {
        return GetComponent<SpriteRenderer>().sprite;
    }

    public override Sprite getPlacementSprite()
    {
        return GetComponent<SpriteRenderer>().sprite;
    }
}