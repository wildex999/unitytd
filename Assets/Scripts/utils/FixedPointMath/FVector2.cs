//Vector2 using Fixed Point FInt
//TODO: Finish writing this(For now its written as its needed)

public class FVector2
{
    public FInt x, y;

    public FVector2(FInt x, FInt y)
    {
        Set(x, y);
    }

    public void Set(FInt x, FInt y)
    {
        this.x = x;
        this.y = y;
    }

    public FInt sqrMagnitude
    {
        get { return (x * x) + (y * y); }
    }

    public FVector2 normalized
    {
        get
        {
            FInt sqrM = FInt.Sqrt(this.sqrMagnitude);
            return new FVector2(x / sqrM, y / sqrM);
        }
    }

    public static FVector2 operator *(FVector2 one, FInt other)
    {
        return new FVector2(one.x * other, one.y * other);
    }

    public static FVector2 operator -(FVector2 one, FVector2 other)
    {
        return new FVector2(one.x - other.x, one.y - other.y);
    }

    public static FVector2 operator +(FVector2 one, FVector2 other)
    {
        return new FVector2(one.x + other.x, one.y + other.y);
    }

    public static explicit operator UnityEngine.Vector2(FVector2 src)
    {
        return new UnityEngine.Vector2((float)src.x, (float)src.y);
    }

    public override string ToString()
    {
        return "FVector2: " + x + " | " + y;
    }

}