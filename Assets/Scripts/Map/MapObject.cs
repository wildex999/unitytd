using UnityEngine;
using System.Collections;
using System.IO;

/*
 * Base class for all objects that can appear and interact on the map(Mobs, towers etc.)
 * This does not include control objects, Tiles, GUI, special effects and so on.
 * */

public abstract class MapObject : MonoBehaviour, IFixedPosition, ISerializedObject {
    public string displayName; //Name displayed in-game
    public MapManager map;
    public int prefabId;

    protected Vector2Gen<int> fixedPosition; //Integer position for use in fixed point simulation
    protected IFixedPosition parent; //Parent who our position is offset from

    public virtual void init(MapManager map, Vector2Gen<int> fixedPosition)
    {
        transform.localPosition = Vector3.zero;
        this.map = map;
        setPosition(fixedPosition);
    }

    public virtual void init(MapManager map, Vector2Gen<int> fixedPositionLocal, IFixedPosition parent)
    {
        init(map, fixedPositionLocal);
        setParent(parent);
    }

    public void setPosition(Vector2Gen<int> position)
    {
        if (position == null)
            position = new Vector2Gen<int>(0, 0);
        fixedPosition = position;
        transform.localPosition = getPosition(true);
    }

    public Vector2Gen<int> getFixedPosition()
    {
        if(parent == null)
            return fixedPosition;
        else
        {
            Vector2Gen<int> parentPos = parent.getFixedPosition();
            return new Vector2Gen<int>(parentPos.x + fixedPosition.x, parentPos.y + fixedPosition.y);
        }
    }

    public Vector2 getPosition(bool local = false)
    {
        if(parent == null || local == true)
            return new Vector2(fixedPosition.x/(float)MapBase.unitSizeFixed, fixedPosition.y/(float)MapBase.unitSizeFixed);
        else
        {
            Vector2Gen<int> parentPos = parent.getFixedPosition();
            return new Vector2((parentPos.x + fixedPosition.x) / (float)MapBase.unitSizeFixed, (parentPos.y + fixedPosition.y) / (float)MapBase.unitSizeFixed);
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
}
