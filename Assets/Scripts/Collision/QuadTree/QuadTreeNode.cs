

using System.Collections.Generic;
using UnityEngine;
public class QuadTreeNode<T> : ICollideHandler
{
    public QuadTreeNode<T> parent;
    public QuadTreeNode<T>[] children; //Leafs of this node
    public List<T> items = new List<T>(); //Items in this node
    public uint level = 0;

    public int posX;
    public int posY;
    public int size;

    public QuadTreeNode(QuadTreeNode<T> parent, int posX, int posY, int size)
    {
        if (parent != null)
        {
            this.parent = parent;
            this.level = parent.level + 1;
        }
        this.posX = posX;
        this.posY = posY;
        this.size = size;
        //Debug.Log("New Quad: " + posX + " | " + posY + " | " + size);
    }

    public void split()
    {
        if (children == null)
        {
            children = new QuadTreeNode<T>[4];
            int newSize = size / 2;
            children[0] = new QuadTreeNode<T>(this, posX - (int)(newSize / 2), posY - (int)(newSize / 2), newSize);
            children[1] = new QuadTreeNode<T>(this, posX + (int)(newSize / 2), posY - (int)(newSize / 2), newSize);
            children[2] = new QuadTreeNode<T>(this, posX + (int)(newSize / 2), posY + (int)(newSize / 2), newSize);
            children[3] = new QuadTreeNode<T>(this, posX - (int)(newSize / 2), posY + (int)(newSize / 2), newSize);
        }
    }

    public Vector2Gen<int> getFixedPosition()
    {
        return new Vector2Gen<int>(posX, posY);
    }

    public override string ToString()
    {
        return "Node - Level: " + level + " PosX: " + posX + " PosY: " + posY + " Items: " + items.Count + " Split: " + (children == null ? "No" : "Yes");    
    }

    //Ignore events and gameObject. We onyl use the collider for the collideWith().
    public GameObject getGameObject()
    {
        throw new System.NotImplementedException();
    }

    public void handleCollisionEnter(FixedCollider other)
    {
        throw new System.NotImplementedException();
    }

    public void handleCollisionContinue(FixedCollider other)
    {
        throw new System.NotImplementedException();
    }

    public void handleCollisionExit(FixedCollider other)
    {
        throw new System.NotImplementedException();
    }

    public bool isValid()
    {
        return true;
    }
}