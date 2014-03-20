using UnityEngine;
using System.Collections;

public class TowerMenu : MonoBehaviour {

    public bool menuVisible; //True if the menu isn't hidden
    public MapManager map;
    public static TowerMenu instance;

    public bool mouseHover; //Set to true when mouse is over the menu


	// Use this for initialization
	void Start () {
        instance = this;
	}
	
    void OnMouseEnter()
    {
        mouseHover = true;
    }

    void OnMouseExit()
    {
        mouseHover = false;
    }
}
