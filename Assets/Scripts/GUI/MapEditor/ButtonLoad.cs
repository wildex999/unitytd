using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonLoad : MonoBehaviour
{
    public GameObject selectorParent;

    private MapSelector selector = null;
    private Dictionary<GameObject, bool> oldState = null;

    void OnClick()
    {
        if (selector != null)
            return;

        //Open the Map selector
        oldState = ChildrenChangeColliderState.setNewState(NGUITools.GetRoot(gameObject), false);
        selector = MapSelector.createMapSelector(selectorParent, MapSelectorType.OpenEditor);
        selector.SelectMapEvent += OnMapSelected;
    }

    void OnMapSelected(MapInfo map)
    {
        ChildrenChangeColliderState.revertState(oldState);

        if (selector != null)
            selector.SelectMapEvent -= OnMapSelected;

        if (EditorMap.instance == null || map == null)
            return;

        //TODO: Handle online

        EditorMap.instance.doLoadMap(map.filename);
    }
}