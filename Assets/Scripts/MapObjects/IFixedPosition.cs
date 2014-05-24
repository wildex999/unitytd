
//Interface used for every object who has an fixed position(Fixed point/deterministic simulation)
using UnityEngine;
public interface IFixedPosition
{
    void setPosition(Vector2Gen<int> position);
    void setParent(IFixedPosition parent);

    Vector2Gen<int> getFixedPosition();
    GameObject getGameObject();
    IFixedPosition getParent();
}
