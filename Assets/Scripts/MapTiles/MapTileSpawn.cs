using UnityEngine;
using System.Collections;

public class MapTileSpawn : MapTile {

    public float timeBetweenSpawns;

    private float lastUnitTime;
    private GameObject Mob1 = Resources.Load<GameObject>("Mobs/Mob1");


    void Start()
    {
        //Start timer for waves
        //TODO
    }

    void Update()
    {
        lastUnitTime -= Time.deltaTime;
        if (lastUnitTime <= 0f)
        {
            lastUnitTime += timeBetweenSpawns;
            MapObject mob = map.addObject(MapManager.createObject(Mob1));
            mob.transform.position = transform.position;
        }
        //Spawn waves
        //TODO
    }
}
