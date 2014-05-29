/*
 * A custom priority queue for use with pathfinding.
 * 
 * Each priority can have multiple entries, which are in a linked list.
 * A Dictionary has an entry pointing to the linked list for each priority.
 * This will allow for insertion and removal without having to sort the elements.
 * 
 * We do not care about the order of elements of the same priority, so new elements are just
 * added on the back.
 * 
 * (Lower is higher priority, i.e cost)
 * 
 *
*/

using System.Collections.Generic;
public class PriorityQueue
{
    private SortedDictionary<int, LinkedList<PathNodeInfo>> priorityDictionary = new SortedDictionary<int,LinkedList<PathNodeInfo>>();

    public PriorityQueue()
    {
    }

    //Pop the element with highest priority
    public PathNodeInfo pop()
    {
        LinkedList<PathNodeInfo> list = null;
        int currentKey = 0;

        //Net 2.0(Unity) does not have the 'First' entry.
        foreach(KeyValuePair<int, LinkedList<PathNodeInfo>> value in priorityDictionary)
        {
            list = value.Value;
            currentKey = value.Key;
            break;
        }

        if (list == null)
            return null; //No nodes left

        PathNodeInfo node = list.First.Value;
        list.RemoveFirst();

        if (list.Count == 0)
            priorityDictionary.Remove(currentKey);
        node.listNode = null;

        return node;
    }

    public void push(PathNodeInfo node)
    {
        LinkedList<PathNodeInfo> list;
        if(priorityDictionary.TryGetValue(node.cost, out list))
        {
            //Priority exists
            node.listNode = list.AddLast(node);
        }
        else
        {
            //Create list for priority
            list = new LinkedList<PathNodeInfo>();
            node.listNode = list.AddLast(node);
            priorityDictionary[node.cost] = list;
        }
    }

    //Remove node from queue
    public void remove(PathNodeInfo node)
    {
        LinkedList<PathNodeInfo> list = node.listNode.List;
        list.Remove(node.listNode);
        node.listNode = null;
    }

    public void clear()
    {
        priorityDictionary.Clear();
    }

    //TODO: Keep a proper count
    public int Count
    {
        get { return priorityDictionary.Count; }
    }
}