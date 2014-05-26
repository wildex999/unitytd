//Vector3 using Fixed Point FInt
//TODO: Finish writing this(For now its written as its needed)

public class FVector3
{
    public FInt x, y, z;

    public FVector3(FInt x, FInt y, FInt z)
    {
        Set(x, y, z);
    }

    public void Set(FInt x, FInt y, FInt z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public FInt sqrMagnitude
    {
        get { return (x * x) + (y * y) + (z * z); }
    }

    public FVector3 normalized
    {
        get 
        {
            FInt sqrM = FInt.Sqrt(this.sqrMagnitude);
            return new FVector3(x/sqrM, y/sqrM, z/sqrM); 

            //FInt m = this.sqrMagnitude;
            //return new FVector3((x*x)/m, (y*y)*m, (z*z)*m);
        }
    }

    public static FVector3 operator *(FVector3 one, FInt other)
    {
        return new FVector3(one.x * other, one.y * other, one.z * other);
    }

    public static FVector3 operator -(FVector3 one, FVector3 other)
    {
        return new FVector3(one.x - other.x, one.y - other.y, one.z - other.z);
    }

    public static FVector3 operator +(FVector3 one, FVector3 other)
    {
        return new FVector3(one.x + other.x, one.y + other.y, one.z + other.z);
    }

    public static explicit operator UnityEngine.Vector3(FVector3 src)
    {
        return new UnityEngine.Vector3((float)src.x, (float)src.y, (float)src.z);
    }

    public override string ToString()
    {
        return "FVector3: " + x + " | " + y + " | " + z;
    }

}