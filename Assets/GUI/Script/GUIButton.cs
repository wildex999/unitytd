using UnityEngine;
using System.Collections;

public class GUIButton : GUIWidget, IClickHandler {

    public Sprite buttonUp;
    public Sprite buttonDown;
    public Sprite buttonHover;
    public Sprite buttonDisabled;

    void Start()
    {
        doFitCollisionToSprite();
    }

    public bool onClick()
    {
        Debug.Log("Button: " + name);
        return true;
    }

    public bool onDown()
    {
        throw new System.NotImplementedException();
    }

    public bool onUp()
    {
        throw new System.NotImplementedException();
    }

    public bool onHover()
    {
        throw new System.NotImplementedException();
    }
}
