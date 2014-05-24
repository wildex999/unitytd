using UnityEngine;
using System.Collections;

public class Mob1 : Monster {

	// Use this for initialization
	void Start () {
        setPath(map.walkingPath);
	}
	
	// Update is called once per frame
	void Update () {
        followPathLocal();
	}

    void FixedUpdate()
    {
        followPathFixed();
    }
}
