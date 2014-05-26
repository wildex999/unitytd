

using UnityEngine;
public class FixedBoxCollider : FixedCollider
{
    //AABB box size
    public int width; //Width = 1 means one on the left AND one on the right(So x size = 3)
    public int height;

    public FixedBoxCollider(ICollideHandler handler, int width, int height)
        : base(handler)
    {
        this.width = width;
        this.height = height;
    }

    public override bool collideWith(FixedCircleCollider other)
    {
        //Get closest point to circle center, clamped by box size, 
        //and check if the distance between that point and circle center is less than circle radius
        FVector2 otherPos = other.handler.getFixedPosition();
        FVector2 pos = handler.getFixedPosition();

        int xDistance = (int)(otherPos.x - pos.x);
        int yDistance = (int)(otherPos.y - pos.y);

        if(xDistance * xDistance > width * width) //Test absolute value
        {
            //Clamp it
            if (xDistance < 0)
                xDistance = -1 * width;
            else
                xDistance = width;
        }
        if(yDistance * yDistance > height * height)
        {
            if (yDistance < 0)
                yDistance = -1 * height;
            else
                yDistance = height;
        }

        int pointX = (int)(pos.x + xDistance);
        int pointY = (int)(pos.y + yDistance);

        //Test if point is inside circle
        xDistance = (int)(pointX - otherPos.x);
        yDistance = (int)(pointY - otherPos.y);

        if ((xDistance * xDistance) + (yDistance * yDistance) <= other.radius * other.radius)
            return true;
        return false;
    }

    public override bool collideWith(FixedBoxCollider other)
    {
        throw new System.NotImplementedException();
    }

    public override string ToString()
    {
        return "Box col - Handler: " + handler + " Pos: " + (handler == null ? "null" : handler.getFixedPosition().ToString()) + " Size: " + width + " | " + height;
    }
}