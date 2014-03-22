using UnityEngine;
using System.Collections;

public class MapTileSpawn : MapTile {

    public float timeBetweenSpawns;
    public float timeToFirstFlyer;
    public float timeBetweenFlyers;

    private float lastUnitTime;
    private float lastFlyerTime;
    private GameObject Mob1 = Resources.Load<GameObject>("Mobs/Mob1");
    private GameObject Mob2 = Resources.Load<GameObject>("Mobs/Mob2_Flying");


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
        if(timeToFirstFlyer > 0f)
            timeToFirstFlyer-= Time.deltaTime;
        else
        {
            lastFlyerTime -= Time.deltaTime;
            if(lastFlyerTime <= 0f)
            {
                lastFlyerTime += timeBetweenFlyers;
                MapObject mob = map.addObject(MapManager.createObject(Mob2));
                mob.transform.position = transform.position;
            }
        }
        //Spawn waves
        //TODO
    }
}
