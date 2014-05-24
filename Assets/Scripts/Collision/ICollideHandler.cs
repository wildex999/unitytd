using System.Collections.Generic;
using UnityEngine;

//Fixed point collision handler
public interface ICollideHandler
{
    Vector2Gen<int> getFixedPosition();
    GameObject getGameObject();
    bool isValid(); //whether this handler is still valid. Used by remembered collisions

    void handleCollisionEnter(FixedCollider other); //First collision with other
    void handleCollisionContinue(FixedCollider other); //Continued collision with other
    void handleCollisionExit(FixedCollider other); //No longer colliding with other
}