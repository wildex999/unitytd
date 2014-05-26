
using UnityEngine;
public class TestTower : TowerBase
{
    private static int size = 2; //2x2
    private static GameObject bulletBase = null;

    public int firePause; //Timet(Steps) to wait between firing
    private int bulletTimer; 

    public override void OnCreate()
    {
        base.OnCreate();

        if (bulletBase == null)
            bulletBase = Resources.Load<GameObject>("Projectiles/Laser");

        bulletTimer = 0;
        GA.API.Design.NewEvent("ALPHA1:GAME:BuiltTower", transform.position);
    }

    public override void StepUpdate()
    {
        //Fire at current target
        if (currentTarget != null)
        {
            bulletTimer--;
            if (bulletTimer > 0)
                return;

            Projectile bullet = createProjectile(bulletBase, getFixedPosition());
            bullet.setTarget(currentTarget);
            bulletTimer += firePause;
        }
        else
            bulletTimer = 0;
    }

    public override string getName()
    {
        return displayName;
    }

    public override string getDescription()
    {
        return "Your everyday basic Test Tower!\nFree air included with every 10th purchase.";
    }

    public override int getPrice()
    {
        return 10;
    }

    public override int getSize()
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

    public override FInt sellValue()
    {
        return FInt.FromParts(0, 500);
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