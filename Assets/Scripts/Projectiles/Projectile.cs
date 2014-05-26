using UnityEngine;
using System.Collections;

public class Projectile : MapObject {

    protected Monster target;

    //public projectileType type;
    public int damage;
    public FInt speed = FInt.FromParts(100,5);

    public override void OnCreate()
    {

    }

    public override void OnRemove()
    {

    }

    public override void StepUpdate()
    {
        moveBullet();
    }

    public virtual void setTarget(Monster target)
    {
        this.target = target;

        //Rotate towards the target
        FVector2 targetPos = target.getFixedPosition();
        FVector2 direction = new FVector2(targetPos.x, targetPos.y) - fixedPosition;
        FVector2 directionNormalized = direction.normalized;

        //Rotate towards the target(Visual only, so we can use normal floating point here)
        float angle = Mathf.Atan2((float)directionNormalized.y, (float)directionNormalized.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90f));
    }

    //Get the base damage this projectile does
    public virtual int getDamage()
    {
        return damage;
    }

    public void moveBullet()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        //Check how close to the target we are
        //FVector3 direction = target.transform.position - transform.position;
        FVector2 targetPos = target.getFixedPosition();
        FVector2 direction = new FVector2(targetPos.x, targetPos.y) - fixedPosition;
        if (direction.sqrMagnitude <= (speed * speed))
        {
            //Hit, do damage to enemy
            target.onHit(this);
            Destroy(gameObject);
        }
        else
        {
            FVector2 directionNormalized = direction.normalized;
            fixedPosition += (directionNormalized * speed);


            //Rotate towards the target(Visual only, so we can use normal floating point here)
            float angle = Mathf.Atan2((float)directionNormalized.y, (float)directionNormalized.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle+90f));
        }

        //Update visual
        transform.position = getPosition();
    }

    public void setPosition(int x, int y)
    {
        fixedPosition.Set(x, y);
        //Update visual position
        transform.localPosition = getPosition();
    }

    public FVector2 getFixedPosition()
    {
        return new FVector2((int)fixedPosition.x, (int)fixedPosition.y);
    }

    public Vector2 getPosition()
    {
        return new Vector2((float)fixedPosition.x / (float)MapBase.unitSizeFixed, (float)fixedPosition.y / (float)MapBase.unitSizeFixed);
    }
}
