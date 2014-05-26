using UnityEngine;
using System.Collections;

public class Mob1 : Monster {

	// Use this for initialization
    public override void OnCreate()
    {
 	    base.OnCreate();
        setPath(map.walkingPath);
    }
	
	// Update is called once per frame
	void Update () {
        followPathLocal();
	}

    public override void StepUpdate()
    {
        followPathFixed();
    }
}
