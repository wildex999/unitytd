using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(TowerMenuItem))]
public class TowerMenuItemEditor : Editor
{

    void OnEnable()
    {
        //Set the sprite
        TowerMenuItem item = (TowerMenuItem)target;
        if (item.tower == null)
            return;
        item.GetComponent<SpriteRenderer>().sprite = item.tower.getMenuSprite();
    }

    void OnDisable()
    {
    }
}