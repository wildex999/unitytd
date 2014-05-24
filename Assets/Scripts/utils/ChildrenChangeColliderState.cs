using System.Collections.Generic;
using UnityEngine;

//Helper function to disable input(Collider disabled) on an object and all it's children(Recursive), while allowing to reverse this later
public class ChildrenChangeColliderState
{
    public static Dictionary<GameObject, bool> setNewState(GameObject start, bool newState)
    {
        Dictionary<GameObject, bool> stateMap = new Dictionary<GameObject, bool>();
        recursiveSetState(stateMap, start, newState);
        return stateMap;
    }

    private static void recursiveSetState(Dictionary<GameObject, bool> stateMap, GameObject parent, bool newState)
    {
        if (parent.collider != null)
        {
            stateMap[parent] = parent.collider.enabled;
            parent.collider.enabled = newState;
        }
        foreach(Transform child in parent.transform)
            recursiveSetState(stateMap, child.gameObject, newState);
    }

    public static void revertState(Dictionary<GameObject, bool> stateMap)
    {
        foreach(KeyValuePair<GameObject, bool> obj in stateMap)
        {
            if (obj.Key == null || obj.Key.collider == null)
                continue;
            obj.Key.collider.enabled = obj.Value;
        }
    }
}