using System.Collections.Generic;

public class PathNodeInfo
{
    public IPathNode node;
    public int cost;
    public bool visited;
    public uint age;
    public LinkedListNode<PathNodeInfo> listNode;

    public PathNodeInfo(IPathNode node)
    {
        this.node = node;
    }
}