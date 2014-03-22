﻿

using UnityEngine;
public class TowerAirWall : TowerBase
{
    private static int size = 1;


    public override bool canMonsterPass(Monster mob)
    {
        if (mob.getMoveType() == MonsterMoveType.Flying)
            return false;
        else if (mob.getMoveType() == MonsterMoveType.Walking)
            return true;
        else
            return base.canMonsterPass(mob);
    }

    public override string getName()
    {
        throw new System.NotImplementedException();
    }

    public override string getDescription()
    {
        throw new System.NotImplementedException();
    }

    public override uint getPrice()
    {
        throw new System.NotImplementedException();
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
        return size;
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