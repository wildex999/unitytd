
//Fixed point collision
using System.Collections.Generic;
using UnityEngine;
public abstract class FixedCollider
{
    public ICollideHandler handler; //Object to handle collisions
    public LinkedListNode<FixedCollider> listNode; //The node for this collider in linkedlist(For units in CollisionManager)
    public bool removeCollider; //Used to mark for later removal(Dont test against)

    public FixedCollider(ICollideHandler handler)
    {
        this.handler = handler;
    }

    //Return true on collision
    public virtual bool collideWith(FixedCollider other)
    {
        if (other == this)
            return false;

        FixedCircleCollider circle = other as FixedCircleCollider;
        if (circle != null)
            return collideWith(circle);

        FixedBoxCollider box = other as FixedBoxCollider;
        if (box != null)
            return collideWith(box);

        return false;
    }

    public abstract bool collideWith(FixedCircleCollider other);
    public abstract bool collideWith(FixedBoxCollider other);
    //public abstract bool collideWith(PointCollider other);

    //Enter, Continue, Exit detection
    private List<FixedCollider> rememberedCollisions = new List<FixedCollider>();

    //Remember an collision, and watch it.
    //Call events on continued collisions, and on exit(No longer colliding)
    //callOnEnter: If true, will call handleCollisionEnter on the handler
    public void rememberCollision(FixedCollider other, bool callOnEnter, bool checkDuplicate = false)
    {
        if(checkDuplicate)
        {
            if (rememberedCollisions.Contains(other))
                return;
        }

        rememberedCollisions.Add(other);
        if (callOnEnter)
            handler.handleCollisionEnter(other);
    }

    //Check remembered collision for continue and exit events
    //callEvents: If true, will call events on the handler. If not true, will still remove from remembered list on exit!
    public void checkRememberedCollisions(bool callEvents)
    {
        for(int index = rememberedCollisions.Count - 1; index >= 0; index--)
        {
            FixedCollider currentCollider = rememberedCollisions[index];
            if(currentCollider.handler == null || !currentCollider.handler.isValid())
            {
                //Remove Deleted gameObjects/object with no handlers
                if (callEvents)
                    handler.handleCollisionExit(currentCollider);
                rememberedCollisions.RemoveAt(index);
                continue;
            }

            if (collideWith(currentCollider) && callEvents)
                handler.handleCollisionContinue(currentCollider);
            else
            {
                if(callEvents)
                    handler.handleCollisionExit(currentCollider);
                rememberedCollisions.RemoveAt(index);
            }
        }
    }

    //Check remembered collision if they are still colliding
    //callOnExit: if true, call exit event on handler. If not true, will still remove from remembered list!
    public void checkRememberedExit(bool callOnExit)
    {
        for (int index = rememberedCollisions.Count - 1; index >= 0; index--)
        {
            FixedCollider currentCollider = rememberedCollisions[index];

            if (!collideWith(currentCollider))
            {
                if(callOnExit)
                    handler.handleCollisionExit(currentCollider);
                rememberedCollisions.RemoveAt(index);
            }
        }
    }

    public bool isRemembered(FixedCollider collider)
    {
        if (rememberedCollisions.Contains(collider))
            return true;
        return false;
    }

}