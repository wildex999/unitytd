using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonSave : MonoBehaviour
{
    public GameObject selectorParent;

    private MapSelector selector = null;
    private Dictionary<GameObject, bool> oldState = null;

    void OnClick()
    {
        if (EditorMap.instance == null || selector != null)
            return;

        //TODO: Handle online saving

        if(EditorMap.instance.getCurrentMapFile().Length > 0)
        {
            //Save to current filename
            EditorMap.instance.doSaveMap(EditorMap.instance.getCurrentMapFile());
        }
        else
        {
            //Get filename from MapSelector
            oldState = ChildrenChangeColliderState.setNewState(NGUITools.GetRoot(gameObject), false);
            selector = MapSelector.createMapSelector(selectorParent, MapSelectorType.Save);
            selector.setFilename(EditorMap.instance.getMapInfo().name);
            selector.SelectMapEvent += OnMapSelected;
        }

    }

    void OnMapSelected(MapInfo map)
    {
        ChildrenChangeColliderState.revertState(oldState);

        if (selector != null)
            selector.SelectMapEvent -= OnMapSelected;

        if (EditorMap.instance == null || map == null)
            return;

        //TODO: Handle online

        EditorMap.instance.doSaveMap(map.filename);
    }
}