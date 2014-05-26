
//Interface used for every object who has an fixed position(Fixed point/deterministic simulation)
using UnityEngine;
public interface IFixedPosition
{
    void setPosition(FVector2 position);
    void setParent(IFixedPosition parent);

    FVector2 getFixedPosition(bool local = false);
    GameObject getGameObject();
    IFixedPosition getParent();
}
