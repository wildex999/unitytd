

//A quadtree implementation
//Will add into parent node if item covers multiple leaf nodes
using System.Collections.Generic;
using UnityEngine;
public class QuadTree<T> where T : FixedCollider
{
    private int maxItems = 5; //Max items in a node before its split
    private QuadTreeNode<T> rootNode;
    private FixedBoxCollider collider = new FixedBoxCollider(null, 0, 0); //Shared collider used when testing item against nodes

    private int size;
    private uint maxLevels; //Max levels of nodes 

    //Will round up size to nearest multiple of 2
    public QuadTree(int size)
    {
        Debug.Log("Got size: " + size);
        this.size = nearestPower2Up(size);
        Debug.Log("Nearest size: " + this.size);

        //Calculate how many levels we can have
        maxLevels = (uint)Mathf.Floor(Mathf.Log(this.size) / Mathf.Log(2)) - 1;
        Debug.Log("Max levels: " + maxLevels);

        rootNode = new QuadTreeNode<T>(null, (int)size / 2, (int)size / 2, this.size);
    }

    //Round up to nearset power of 2(From: http://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2 )
    private int nearestPower2Up(int v)
    {
        if (v == 0)
            return 2;
        v--;
        v |= v >> 1;
        v |= v >> 2;
        v |= v >> 4;
        v |= v >> 8;
        v |= v >> 16;
        v++;
        return v;
    }

    //Get all items within nodes hit by the given collider
    public List<T> getItems(FixedCollider selector)
    {
        List<QuadTreeNode<T>> nodes = new List<QuadTreeNode<T>>();

        nodes.Add(rootNode);
        getNodes(rootNode, selector, nodes);

        List<T> items = new List<T>();

        foreach (QuadTreeNode<T> node in nodes)
        {
            List<T> nodeItems = node.items;
            foreach (T item in nodeItems)
            {
                items.Add(item);
            }
        }
        return items;
    }

    //Return the bottom SINGLE node covered by selector(If covering multiple children, parent is returned)
    //Assumes the given root is within selector
    private QuadTreeNode<T> getNode(QuadTreeNode<T> root, FixedCollider selector)
    {
        QuadTreeNode<T>[] leaves = root.children;
        if (leaves == null)
            return root;

        //Test children against selector
        short hitCount = 0;
        QuadTreeNode<T> lastHit = null;
        foreach(QuadTreeNode<T> node in leaves)
        {
            //Set-up shared collider to avoid allocating one per node
            collider.handler = node;
            collider.width = collider.height = node.size/2;
            if (collider.collideWith(selector))
            {
                hitCount++;
                if (hitCount > 1)
                {
                    return root; //Covers multiple children, return root
                }
                lastHit = node;
            }
        }
        if (lastHit == null) //If no hit
        {
            return null;
        }

        //One child was hit. Check it's children
        lastHit = getNode(lastHit, selector);

        return lastHit;
    }

    //Add all nodes covered by selector, to the given list
    //Assumes the given root is already added to list
    private void getNodes(QuadTreeNode<T> root, FixedCollider selector, List<QuadTreeNode<T>> nodeList, bool ignoreNoItems = false)
    {
        QuadTreeNode<T>[] leaves = root.children;
        if (leaves == null)
            return;

        //Test children
        foreach (QuadTreeNode<T> node in leaves)
        {
            collider.handler = node;
            collider.width = collider.height = node.size/2;
            if (collider.collideWith(selector))
            {
                if(ignoreNoItems)
                {
                    if (node.items.Count != 0)
                        nodeList.Add(node);
                }
                else
                    nodeList.Add(node);

                //Add children of child
                getNodes(node, selector, nodeList, ignoreNoItems);
            }
        }
    }

    //Insert object into QuadTree
    public bool insert(T item, bool allowDuplicate = true)
    {
        //TODO: Check for duplicates when adding

        QuadTreeNode<T> hitNode = getNode(rootNode, item);
        if (hitNode == null)
            Debug.LogError("Got null node during insert: " + item);
        addToNode(hitNode, item);

        //Debug.Log("Added item to " + hitNode);

        return true;
    }

    //Remove item from QuadTree. Note: Assumes same collider(position, size) is given as when added!
    public bool remove(T item)
    {
        //TODO: unSplit when removing?
        //Same procedure as insert
        QuadTreeNode<T> hitNode = getNode(rootNode, item);
        if (hitNode == null)
            Debug.LogError("Got null node during remove: " + item);
        hitNode.items.Remove(item);

        //Debug.Log("Remove item from " + hitNode);

        return true;
    }

    //Add item to node, and split node if item count exceeds max
    private void addToNode(QuadTreeNode<T> node, T item)
    {
        node.items.Add(item);
        if(node.items.Count >= maxItems && node.children == null)
        {
            //Split the node and move the items into their respective nodes
            if(!splitNode(node))
                return;

            List<T> nodeItems = node.items;
            for(int index = nodeItems.Count-1; index >= 0; index--)
            {
                T moveItem = nodeItems[index];
                //Get new node for item, and if different from current, remove it from current and add it to the new one
                QuadTreeNode<T> newNode = getNode(node, moveItem);
                if (newNode == null)
                    Debug.LogError("newNode is null! Item: " + moveItem);
                if(newNode != node)
                {
                    node.items.RemoveAt(index);
                    newNode.items.Add(moveItem);
                }
            }

        }

    }

    //Split a node to have 4 child nodes. Don't split if reached max levels
    //Returns true if split
    private bool splitNode(QuadTreeNode<T> node)
    {
        if (node.level + 1 > maxLevels || node.children != null)
            return false;
        node.split();

        return true;
    }
}