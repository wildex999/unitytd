
public class PathNodeInfo
{
    public IPathNode node;
    public int cost;
    public bool visited;
    public uint age;

    public PathNodeInfo(IPathNode node)
    {
        this.node = node;
    }
}