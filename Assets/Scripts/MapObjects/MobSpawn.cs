using UnityEngine;
using System.Collections;

public class MobSpawn : MapObject {

    public FInt timeBetweenSpawns = FInt.FromParts(1, 100) * MapBase.simFramerate;
    public FInt timeToFirstFlyer = FInt.FromParts(10, 0) * MapBase.simFramerate;
    public FInt timeBetweenFlyers = FInt.FromParts(2, 500) * MapBase.simFramerate;

    private FInt lastUnitTime = 0;
    private FInt lastFlyerTime = 0;
    private GameObject Mob1 = null;
    private GameObject Mob2 = null;
    private CollisionManager collisionManager;

    private MapTile spawnTile;

    public override void OnCreate()
    {
        if (Mob1 == null)
        {
            Mob1 = Resources.Load<GameObject>("Mobs/Mob1");
            Mob2 = Resources.Load<GameObject>("Mobs/Mob2_Flying");
        }

        //Get the tile we are on and mark it as a spawn tile
        spawnTile = map.getTileWorld(getFixedPosition());
        if(spawnTile == null)
        {
            Debug.LogError("Got null tile for spawn: " + getFixedPosition());
            Destroy(gameObject);
            return;
        }
        map.addSpawn(spawnTile);

        //Start timer for waves
        //TODO

        Debug.Log("MobSpawner at: " + getFixedPosition());
    }

    public override void OnRemove()
    {
        //Mark the tile we are on as no longer a spawn tile
    }

    public override void StepUpdate()
    {
        if (!MapManager.gameRunning)
            return;

        lastUnitTime --;
        if (lastUnitTime <= 0)
        {
            lastUnitTime += timeBetweenSpawns;
            MapObject mob = map.addObject(MapManager.createObject(Mob1), getFixedPosition());
        }
        if (timeToFirstFlyer > 0)
            timeToFirstFlyer--;
        else
        {
            lastFlyerTime--;
            if (lastFlyerTime <= 0)
            {
                lastFlyerTime += timeBetweenFlyers;
                MapObject mob = map.addObject(MapManager.createObject(Mob2), getFixedPosition());
            }
        }
        //Spawn waves
        //TODO
    }
}
