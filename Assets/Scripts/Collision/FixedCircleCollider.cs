
using UnityEngine;
public class FixedCircleCollider : FixedCollider
{
    public int radius;

    public FixedCircleCollider(ICollideHandler handler, int radius)
        : base(handler)
    {
        this.radius = radius;
    }

    public override bool collideWith(FixedCircleCollider other)
    {
        //Get distance between centers, and test if larger than their added radiuses.
        //(x2 - x1)^2 + (y2 - y1)^2 <= (r1^2 + r2^2) 
        FVector2 otherPos = other.handler.getFixedPosition();
        FVector2 pos = handler.getFixedPosition();
        int xDiff = (int)((otherPos.x - pos.x) * (otherPos.x - pos.x));
        int yDiff = (int)((otherPos.y - pos.y) * (otherPos.y - pos.y));

        if (xDiff + yDiff < (other.radius * other.radius) + (radius * radius))
            return true;
        return false;
    }

    public override bool collideWith(FixedBoxCollider other)
    {
        //Implemented in box collider
        return other.collideWith(this);
    }

    public void setRadius(int radius)
    {
        this.radius = radius;
    }

    public override string ToString()
    {
        return "Circle col - Handler: " + handler + " Pos: " + (handler == null ? "null" : handler.getFixedPosition().ToString()) + " Radius: " + radius;
    }
}