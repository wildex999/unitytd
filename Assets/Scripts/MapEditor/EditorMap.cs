using UnityEngine;
using System.Collections;

/*
 * The Map manager for the Map Editor
 * 
 * */ 

public class EditorMap : MonoBehaviour {
    //Use a camera for each main element, allowing us to enable/disable the rendering easily, and manage draw order.
    public Camera tilesCamera; //Draws the tiles
    public Camera graphicsCamera; //Draws the graphic layers
    public Camera groundCamera; //Draw the ground


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
