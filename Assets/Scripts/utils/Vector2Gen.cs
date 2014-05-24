

public class Vector2Gen<T>
{
    private T _x, _y;

    public Vector2Gen(T x, T y)
    {
        _x = x;
        _y = y;
    }



    public T x
    {
        get { return _x; }
    }

    public T y
    {
        get { return _y; }
    }
}