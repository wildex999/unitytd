using UnityEngine;
using System.Collections;

public class ToggleShowTiles : MonoBehaviour, IClickHandler {

    public bool onClick()
    {
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
