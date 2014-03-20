using UnityEngine;
using System.Collections;

/* 
 * Clicking this MenuItem will put you in to "Tower placement" mode, with the MenuItems tower as selected.
 * 
 * */

public class TowerMenuItem : MonoBehaviour {

    public TowerBase tower;

	// Use this for initialization
	void Start () {
        if(tower == null)
            return;
	    this.GetComponent<SpriteRenderer>().sprite = tower.GetComponent<SpriteRenderer>().sprite;
	}

    void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(0))
        {
            //Set as selected tower
            TowerMenu.instance.map.startPlacingTower(tower);
        }
    }

    void OnMouseEnter()
    {
        //We are still on the menu
        TowerMenu.instance.mouseHover = true;
    }

    void OnMouseExit()
    {
        TowerMenu.instance.mouseHover = false;
    }
}

