using System.Collections.Generic;
using UnityEngine;

//Holds a spatial partitioned map of objects to do quick collision detection between two collision objects
public class CollisionManager : MonoBehaviour
{
    //Quadtree storing towers
    private QuadTree<FixedCircleCollider> towers;
    private List<FixedCircleCollider> towerList = new List<FixedCircleCollider>();
    private LinkedList<FixedCollider> units = new LinkedList<FixedCollider>();
    private List<FixedCollider> toAddUnit = new List<FixedCollider>(); //List of items to add after done iterating
    private List<FixedCollider> toRemoveUnit = new List<FixedCollider>(); //List of items to remove after done iterating
    private bool isIterating; //If trying to remove while iterating, add to a list for removal afterwards.

    //Initialize the CollisionManager for a given map
    public bool initialize(MapBase map)
    {
        towers = new QuadTree<FixedCircleCollider>((int)map.getSizeFixed());
        return true;
    }

    //Add tower collider to quadtree
    public void addTower(FixedCircleCollider collider)
    {
        if (towers.insert(collider))
            towerList.Add(collider);
    }

    public void removeTower(FixedCircleCollider collider)
    {
        towers.remove(collider);
        //TODO: Make this faster somehow, as it is we have to iterate the whole list to remove a tower
        towerList.Remove(collider);
    }

    //Add unit that should be checked against tower colliders
    public void addUnit(FixedCollider collider)
    {
        collider.removeCollider = false;
        if(isIterating)
            toAddUnit.Add(collider);
        else
            collider.listNode = units.AddLast(collider);
    }

    public void removeUnit(FixedCollider collider)
    {
        collider.removeCollider = true;
        if (isIterating)
            toRemoveUnit.Add(collider);
        else
            units.Remove(collider.listNode);
    }

    //Do collision check for all objects
    public void doCollisionCheck()
    {
        isIterating = true;
        int collisionChecks = 0;

        //Handle remembered units in towers
        foreach(FixedCollider towerCollider in towerList)
            towerCollider.checkRememberedCollisions(true);

        //Iterate and test
        foreach(FixedCollider unitCollider in units)
        {
            if (unitCollider.removeCollider)
                continue;
            List<FixedCircleCollider> towerColliderList = towers.getItems(unitCollider);
            //Debug.Log("Doing checks: " + towerColliderList.Count + " (Current Towers: " + towerList.Count + ")");
            //List<FixedCircleCollider> towerColliderList = towerList;
            foreach(FixedCircleCollider tower in towerColliderList)
            {
                collisionChecks++;
                if(tower.collideWith(unitCollider))
                {
                    if (tower.isRemembered(unitCollider))
                        continue; //Skip remembered as they are already handled

                    //Unit is within towers range, call onEnter and add it to watch list
                    tower.rememberCollision(unitCollider, true);
                }
            }
        }

        //Debug.Log("Performed " + collisionChecks + " Collision checks!");

        //Add from toAdd list
        foreach (FixedCollider addCollider in toAddUnit)
            addCollider.listNode = units.AddLast(addCollider);
        toAddUnit.Clear();

        //Remove using toRemove list
        //TODO: Store node in item for O(1) removal
        foreach (FixedCollider removeCollider in toRemoveUnit)
            units.Remove(removeCollider.listNode);
        toRemoveUnit.Clear();

        isIterating = false;
    }
}