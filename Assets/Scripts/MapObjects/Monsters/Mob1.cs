﻿using UnityEngine;
using System.Collections;

public class Mob1 : Monster {

	// Use this for initialization
	void Start () {
        setPath(map.walkingPath);
        speed = 1f;
	}
	
	// Update is called once per frame
	void Update () {
        followPath();
	}
}
