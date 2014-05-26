using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

/*
 * Base class for all objects that can appear and interact on the map(Mobs, towers, Projectiles etc.)(Synced objects)
 * This does not include control objects, Tiles, GUI, special effects and so on.(Non-synced objects)
 * */

public abstract class MapObject : MonoBehaviour, IFixedPosition, ISerializedObject {
    public string displayName; //Name displayed in-game
    public MapManager map;
    public int prefabId;

    protected FVector2 fixedPosition; //Integer position for use in fixed point simulation
    protected IFixedPosition parent; //Parent who our position is offset from

    private LinkedList<MapObject> objectList;
    private LinkedListNode<MapObject> listNode;

    #region Event handlers

    public abstract void OnCreate();

    public virtual void StepUpdate() {} //Called every FixedStep by MapManager/GameManager

    private void OnDestroy()
    {
        //Remove from list
        if(objectList != null)
            objectList.Remove(listNode);

        OnRemove();
    }
    public abstract void OnRemove();
    #endregion

    #region init
    //Init is called when they are added to the map
    public void init(MapManager map, FVector2 fixedPosition)
    {
        transform.localPosition = Vector3.zero;
        this.map = map;
        setPosition(fixedPosition);

        objectList = map.objectList;
        listNode = map.objectList.AddLast(this);

        OnCreate();
    }

    public void init(MapManager map, FVector2 fixedPositionLocal, IFixedPosition parent)
    {
        setParent(parent);
        init(map, fixedPositionLocal);
    }
    #endregion

    public void setPosition(FVector2 position)
    {
        if (position == null)
            position = new FVector2(0, 0);
        fixedPosition = position;
        transform.localPosition = getPosition(true);
    }

    public FVector2 getFixedPosition(bool local = false)
    {
        if(parent == null || local)
            return fixedPosition;
        else
        {
            FVector2 parentPos = parent.getFixedPosition();
            return new FVector2(parentPos.x + fixedPosition.x, parentPos.y + fixedPosition.y);
        }
    }

    public Vector2 getPosition(bool local = false)
    {
        if(parent == null || local == true)
            return new Vector2((int)fixedPosition.x/(float)MapBase.unitSizeFixed, (int)fixedPosition.y/(float)MapBase.unitSizeFixed);
        else
        {
            FVector2 parentPos = parent.getFixedPosition();
            return new Vector2((int)(parentPos.x + fixedPosition.x) / (float)MapBase.unitSizeFixed, (int)(parentPos.y + fixedPosition.y) / (float)MapBase.unitSizeFixed);
        }
    }

    public void setParent(IFixedPosition parent)
    {
        transform.parent = parent.getGameObject().transform;
        this.parent = parent;
        //Set position to match parent offset
        setPosition(fixedPosition);
    }

    public GameObject getGameObject()
    {
        return gameObject;
    }

    public IFixedPosition getParent()
    {
        return parent;
    }


    public void setUniqueId(int id)
    {
        prefabId = id;
    }

    public int getUniqueId()
    {
        return prefabId;
    }

    public virtual void writeToStream(BinaryWriter stream)
    {
        throw new System.NotImplementedException();
    }

    public virtual void readFromStream(BinaryReader stream)
    {
        throw new System.NotImplementedException();
    }

    //Return a hash(int) for the objects current network state(Synced variables)
    //Must give same hash on all systems if they have the same state
    public virtual int getSyncHash()
    {
        int hash = 0;
        hash += (int)getFixedPosition().x;
        hash += (int)getFixedPosition().y;

        return hash;
    }
}
