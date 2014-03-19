
public class PathNodeInfo
{
    public IPathNode node;
    public int cost;
    public bool visited;

    public PathNodeInfo(IPathNode node)
    {
        this.node = node;
    }
}