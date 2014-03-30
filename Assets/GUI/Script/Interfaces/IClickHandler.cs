using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//Any child of a GUIPanel that implements this interface, can receive a click if the gameobject has a collision box.

public interface IClickHandler
{
    //Called when object is clicked. Return true if handled(Will not propagate to other objects)
    bool onClick();
    bool onDown();
    bool onUp();
    bool onHover();
}
