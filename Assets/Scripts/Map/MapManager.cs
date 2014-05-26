using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

/*
 * The MapManager has the job of managing everything happening on the map, thus:
 * -Loading/Initializing the map
 * -Managing objects on the map
 * -Act on input from player
 * -Sync with networking
 * 
 * The MapManager is in effect the game logic manager.
 * */

public enum PlayerState
{
    Idle,
    PlacingTower,
    InMenu //In menu(Stop taking input/Pause game in singleplayer)
}

public class MapManager : MapBase {
    public static bool gameRunning = false; //Used for pausing or when in map editor
    private static MapManager instance;
    public static bool isClient; //When true, check with server for events
    public static bool isServer; //When true, send network updates to clients

    public List<List<MapTile>> grid = new List<List<MapTile>>();

    private Camera mapCam = null;
    private Camera guiCam = null;
    private GameObject towerMouseSprite = null;
    private DummyTower dummyTowerPrefab;

    private MapTile goalTile = null;
    List<MapTile> spawns = new List<MapTile>();

    public LinkedList<MapObject> objectList = new LinkedList<MapObject>(); //List of all map objects that are to be ticked

    private bool moveCamera = false; //Set to true when moving camera(Middle mouse button down)
    private bool inputOnGui; //Mouse over gui, ignore input
    private MapTile currentTile = null; //Current tile mouse is hovering over
    private TileHover hoverObj = null;  //GameObject placed over current tile hover
    private TowerBase currentPlaceTower = null; //Tower selected for placing by player

    [SerializeField]
    private GameManager gameManager;

    public CollisionManager collisionManager = null;

    public PathFinder walkingPath = new PFDijkstra4Dir();
    public PathFinder flyingPath = new PFDijkstra4Dir();

    public PlayerState currentPlayerState;

    public static MapManager createMapManager(GameManager gameManager)
    {
        MapManager mgr;

        GameObject mapManagerPrefab = Resources.Load<GameObject>("MapManager");
        if (mapManagerPrefab == null)
            Debug.LogError("Failed to load Map Manager prefab!");
        GameObject newMapManager = (GameObject)Instantiate(mapManagerPrefab);
        mgr = newMapManager.GetComponent<MapManager>();

        mgr.gameManager = gameManager;
        instance = mgr;

        return mgr;
    }

    public static MapManager getMapManager()
    {
        return instance;
    }

    void Awake()
    {
        //Initialize the Collision Manager
        GameObject collisionManagerObject = GameObject.Find("CollisionManager");
        if (collisionManagerObject == null)
            Debug.LogError("Failed to find CollisionManager object");
        collisionManager = collisionManagerObject.GetComponent<CollisionManager>();
        if (collisionManager == null)
            Debug.LogError("Failed to get CollisionManager component");
    }

    void Start()
    {
        mapCam = GameObject.Find("MapCam").camera;
        guiCam = GameObject.Find("GUICam").camera;
        towerMouseSprite = GameObject.Find("TowerMouseSprite");
        if (towerMouseSprite == null)
            Debug.LogError("Could not find TowerMouseSprite!");
        towerMouseSprite.SetActive(false); //Hide it until we select a tower

        //Load Dummy tower prefab
        GameObject dummyTowerobj = Library.instance.getPrefab(13); //We know the dummy tower is id 13, and this should not change
        dummyTowerPrefab = dummyTowerobj.GetComponent<DummyTower>();
        if (dummyTowerPrefab == null)
            Debug.LogError("Unable to load Dummy tower prefab!");

        currentPlayerState = PlayerState.Idle;
        Time.fixedDeltaTime = 1f / (float)MapBase.simFramerate;

        //Load TileHover
        GameObject hoverPrefab = (GameObject)Resources.Load("GUI/TileHover");
        hoverObj = ((GameObject)Instantiate(hoverPrefab)).GetComponent<TileHover>();
        hoverObj.gameObject.SetActive(false); //Disable it until we hover over a tile


       
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 worldMousePos = mapCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 guiMousePos = guiCam.ScreenToWorldPoint(Input.mousePosition);

        //Check if we are to check input
        Camera nguiCam = guiCam;
        Ray inputRay = nguiCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(inputRay.origin, inputRay.direction, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("GUIObj")))
            inputOnGui = true;
        else
            inputOnGui = false;

        hoverObj.gameObject.SetActive(false);

        if(currentPlayerState == PlayerState.PlacingTower)
        {
            //Make Tower selection follow the mouse
            towerMouseSprite.transform.position = new Vector3(worldMousePos.x, worldMousePos.y, 0);
            if (inputOnGui)
                Screen.showCursor = true;
            else
                Screen.showCursor = false;
        }

        /*MapTile testTile = getTileWorld((int)worldMousePos.x*200, (int)worldMousePos.y*200);
        if(testTile != null)
            Debug.Log("HoverTile: " + testTile.tileX + " | " + testTile.tileY + "(" + worldMousePos.x*200 + " | " + worldMousePos.y*200 + ")");*/

        //Handle input outside GUI
        if (!inputOnGui)
        {
            //Camera movement
            if (moveCamera == true)
            {
                mapCam.transform.position -= new Vector3(Input.GetAxisRaw("Mouse X") / 2f, Input.GetAxisRaw("Mouse Y") / 2f);
                //TODO: Check for borders
            }

            if (Input.GetMouseButtonDown(2)) //Middle mouse button
            {
                Screen.lockCursor = true;
                moveCamera = true;
            }
            else if (Input.GetMouseButtonUp(2))
            {
                Screen.lockCursor = false;
                moveCamera = false;
            }
            //Camera Zoom
            float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
            if (mouseScroll > 0)
                mapCam.orthographicSize = Mathf.Max(mapCam.orthographicSize - mouseScroll, 3.5f);
            else if (mouseScroll < 0)
                mapCam.orthographicSize = Mathf.Min(mapCam.orthographicSize - mouseScroll, 15f);

            //Tile hover check
            Vector3 tilePos = worldMousePos - this.transform.position;
            //1 Unit = 1 tile
            int tileX = (int)Math.Floor(tilePos.x + 0.5f);
            int tileY = (int)Math.Floor(tilePos.y + 0.5f);

            currentTile = getTile(tileX, tileY);
            if (currentTile != null && moveCamera == false)
            {
                if (currentPlayerState == PlayerState.Idle)
                {
                    //Place hover object on tile
                    hoverObj.gameObject.SetActive(true);
                    hoverObj.transform.parent = currentTile.transform;
                    hoverObj.transform.localPosition = Vector3.zero;
                }
                else if (currentPlayerState == PlayerState.PlacingTower)
                {
                    //Snap tower selection to grid
                    if (currentTile.canBuild(currentPlaceTower)) //Only snap if position is buildable
                    {
                        float offset = (currentPlaceTower.getSize() / 2f) - 0.5f;
                        towerMouseSprite.transform.position = new Vector3(currentTile.transform.position.x + offset, currentTile.transform.position.y + offset);
                    }
                }
            }

            //Handle mouse click
            if (Input.GetMouseButtonDown(0))
            {
                if (currentTile != null)
                {
                    //Debug.Log("Left click on tile: " + currentTile + "(X:" + currentTile.tileX + " Y:"+ currentTile.tileY + ")");
                    if (currentPlayerState == PlayerState.PlacingTower)
                        gameManager.placeTower(currentTile, currentPlaceTower);
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (currentPlayerState == PlayerState.PlacingTower)
                    startPlacingTower(null);
            }
        }


	}

    //Run StepUpdate on all Map objects
    public void updateMapObjects()
    {
        for (LinkedListNode<MapObject> it = objectList.First; it != null; it = it.Next)
        {
            it.Value.StepUpdate();
        }
    }

    public override bool loadMap(BinaryReader stream)
    {
        //TestMap
        /*Debug.Log("Loading test map");

        List<List<MT>> testMap = TestMap1.testMap;
        int sizeY = testMap.Count;
        int sizeX = testMap[0].Count;
        setSize(sizeX, sizeY);

        goalTile = null;

        for(int y = 0; y < testMap.Count; y++)
        {
            List<MT> xList = testMap[y];
            if(xList == null)
                continue;
            for (int x = 0; x < xList.Count; x++)
            {
                MT tile = xList[x];
                switch(tile)
                {
                    case MT.G:
                        addTile(x, (testMap.Count-1)-y, "MapTiles/MapTileGround");
                        break;
                    case MT.N:
                        addTile(x, (testMap.Count-1)-y, "MapTiles/MapTileNoBuild");
                        break;
                    case MT.W:
                        addTile(x, (testMap.Count-1)-y, "MapTiles/MapTileWall");
                        break;
                    case MT.S:
                        spawnTile = addTile(x, (testMap.Count-1)-y, "MapTiles/MapTileSpawn");
                        break;
                    case MT.C:
                        goalTile = addTile(x, (testMap.Count - 1) - y, "MapTiles/MapTileCastle");
                        break;
                }
            }
        }

        if(goalTile == null)
            Debug.LogError("No Goal tile found in testmap!");*/

        if (!base.loadMap(stream))
            return false;

        //Collisions
        collisionManager.initialize(this);

        //Setup pathfinding
        //Test Mob1
        Monster testMob = (Monster)createObject("Mobs/Mob1", null);
        Monster testMobFlying = (Monster)createObject("Mobs/Mob2_Flying", null);

        //Setup pathfinders
        walkingPath.init(goalTile, testMob, this);
        walkingPath.calculatePath();

        flyingPath.init(goalTile, testMobFlying, this);
        flyingPath.calculatePath();

        Destroy(testMob.gameObject); //Don't spawn it(Will be kept in memory for future reference by PathFinder)
        Destroy(testMobFlying.gameObject);

        gameRunning = true;

        return true;
    }

    //Set a new player state(Unsetting the previous state)
    private void setPlayerState(PlayerState newState)
    {
        //Unset previous state, cleaning up whatever is needed
        switch (currentPlayerState)
        {
            case PlayerState.PlacingTower:
                Screen.showCursor = true;
                towerMouseSprite.SetActive(false);
                break;
        }

        //Set the new state
        switch (newState)
        {
            case PlayerState.PlacingTower:
                Screen.showCursor = false;
                towerMouseSprite.SetActive(true);
                break;
        }
        currentPlayerState = newState;
    }

    public void startPlacingTower(TowerBase tower)
    {
        if(tower == null)
        {
            setPlayerState(PlayerState.Idle);
            return;
        }

        setPlayerState(PlayerState.PlacingTower);
        currentPlaceTower = tower;
        towerMouseSprite.GetComponent<SpriteRenderer>().sprite = tower.getPlacementSprite();
        towerMouseSprite.transform.localScale = tower.transform.localScale;
    }

    //Add object to map
    public override MapObject addObject(MapObject obj, FVector2 fixedPosition)
    {
        obj.transform.parent = this.transform;
        obj.transform.localPosition = Vector3.zero;
        obj.init(this, fixedPosition);
        return obj;
    }

    public override void removeObject(MapObject obj)
    {
        obj.transform.parent = null;
        obj.map = null;
    }

    public override MapTile setTile(int x, int y, MapTile tile)
    {
        if (y >= grid.Count || y < 0 || x >= grid[y].Count || x < 0)
        {
            Debug.LogError("Tried placing tile outside grid: " + x + " | " + y);
            return null;
        }

        MapTile oldTile = grid[y][x];
        if (oldTile != null)
            Destroy(oldTile.gameObject);

        tile.transform.parent = this.transform;
        tile.transform.localPosition = new Vector2(x, y);

        grid[y][x] = tile;
        tile.init(this, x, y);

        return tile;
    }

    public override MapTile getTile(int x, int y)
    {
        if (y >= grid.Count || y < 0)
            return null;
        if (x >= grid[y].Count || x < 0)
            return null;

        return grid[y][x];
    }

    public override void setSize(int width, int height)
    {
        clearMap();

        for (int y = 0; y < height; y++)
        {
            List<MapTile> xList = new List<MapTile>();
            grid.Add(xList);
            for (int x = 0; x < width; x++)
                xList.Add(null);
        }
    }

    //Clear every MapTile and MapObject(Delete them)
    public override void clearMap()
    {
        int height = grid.Count;
        for (int y = 0; y < height; y++)
        {
            int width = grid[y].Count;
            for (int x = 0; x < width; x++)
            {
                MapTile tile = grid[y][x];
                if (tile == null)
                    continue;

                //Destroy map object on tile
                ITileObject obj = tile.getMapObject();
                if (obj != null)
                    Destroy(obj.getGameObject());

                //Destroy tile object
                Destroy(tile.gameObject);
            }
        }
    }

    public override uint getSizeFixed()
    {
        uint maxY = (uint)grid.Count*(uint)MapBase.unitSizeFixed;
        uint maxX = 0;
        foreach(List<MapTile> yTile in grid)
        {
            if (yTile.Count > maxX)
                maxX = (uint)yTile.Count;
        }
        maxX *= MapBase.unitSizeFixed;
        if (maxY > maxX)
            return maxY;
        else
            return maxX;
    }

    public override float getSize()
    {
        throw new NotImplementedException();
    }

    public void setGameManager(GameManager newGameManager)
    {
        gameManager = newGameManager;
    }

    public GameManager getGameManager()
    {
        return gameManager;
    }

    //Recalculate paths for the given moveType
    public void calculatePaths(MonsterMoveType moveType)
    {
        walkingPath.calculatePath();
        flyingPath.calculatePath();
    }

    public List<PathFinder> getPaths()
    {
        List<PathFinder> paths = new List<PathFinder>();
        paths.Add(walkingPath);
        paths.Add(flyingPath);
        
        return paths;
    }

    //Set the mapTile that every enemy will walk towards
    public void setGoalTile(MapTile tile)
    {
        goalTile = tile;
    }

    public void addSpawn(MapTile tile)
    {
        spawns.Add(tile);
    }

    public List<MapTile> getSpawns(bool onlyActive = false)
    {
        
        if (!onlyActive)
            return spawns;
        else
            return spawns;//TODO: Only include active(Create list, go through all spawns and check isActive etc.)
    }

    //Check if a tower is blocking a path(Used when placing towers)
    public bool towerIsBlocking()
    {
        //TODO: Some monsters might not require a valid path at all times(I.e, they will create it as they go)

        //Check if it will block any living enemies
        foreach (Monster monster in Monster.monsters)
        {
            PathFinder path = monster.getPath();
            MapTile nextTile = monster.getNextNode();
            if (path == null || nextTile == null)
                continue;
            PathNodeInfo nextNode = path.getNodeInfo(nextTile);
            if (nextNode == null)
                continue;

            if (nextNode.cost == -1)
            {
                return true;
            }
        }

        //TODO: Special spawns might not need a path for all types!
        List<PathFinder> paths = getPaths();
        List<MapTile> spawns = getSpawns(true);

        foreach (PathFinder path in paths)
        {
            foreach (MapTile spawn in spawns)
            {
                if (path.getNodeInfo(spawn).cost == -1)
                {
                    return true;
                }
            }
        }

        return false;
    }



    //Authorative actions
    //Return true if action didn't fail
    public bool authorativePlaceTower(MapTile tile, TowerBase placeTower, Player player)
    {
        //Selling
        if (placeTower is TowerSell)
        {
            //TODO: Implement different selling mechanic, this feels too much like a hack, and can go wrong in so many ways later on
            //TODO: Check if player is allowed to sell this tower
            TowerBase toSell = tile.getMapObject() as TowerBase;
            if (toSell != null && toSell.canSell())
            {
                collisionManager.removeTower(toSell.fixedCollider);
                toSell.removeTower();
                //TODO: Give money to player
                gameManager.Money += (int)((int)toSell.getPrice() * toSell.sellValue());
            }
            else
                return false;

            return true;
        }

        //Building
        //TODO: Check if player has the money/permission to build tower
        TowerBase newTower = null;
        if (placeTower.canBuild(tile) && gameManager.Money >= placeTower.getPrice())
        {
            newTower = placeTower.createTower(tile, placeTower);

            //See if any mobs now have no path to the end, if so destroy the placed tower(Or deny it in some way)
            bool killTower = towerIsBlocking();

            if (killTower)
            {
                newTower.removeTower();
                return false; //Tower would block
            }
            else
            {
                gameManager.Money -= placeTower.getPrice();
                //Add tower to Collision Manager
                collisionManager.addTower(newTower.fixedCollider);
                return true;
            }
        }

        return false;
    }

    //Client actions(Dummy)
    public bool clientPlaceTower(MapTile tile, TowerBase tower)
    {
        //Do local checks before asking the server
        //TODO: Do permission and money check
        bool canPlace = true;

        //Selling
        if(tower is TowerSell)
        {
            TowerBase toSell = tile.getMapObject() as TowerBase;
            if (toSell != null && toSell.canSell())
            {
                //TODO: Check if we are allowed to sell it
                return true;
            }
            else
                return false;
        }

        //Building
        TowerBase newTower = null;
        if (!tower.canBuild(tile))
            return false;

        newTower = tower.createTower(tile, tower);
        if (towerIsBlocking())
            canPlace = false;
        newTower.removeTower();

        if(canPlace)
        {
            //Place dummy
            Debug.Log("PLACE DUMMY");
            dummyTowerPrefab.setOriginal(tower);
            DummyTower dummy = dummyTowerPrefab.createTower(tile, dummyTowerPrefab, true) as DummyTower;
            Debug.Log("CREATED DUMMY");
        }
        
        return canPlace;
    }

    //Server actions(Done by server, so do here too)
    public void serverPlaceTower(MapTile tile, TowerBase tower, Player player)
    {
        if (tower is TowerSell)
        {
            TowerBase toSell = tile.getMapObject() as TowerBase;
            if (toSell != null)
            {
                collisionManager.removeTower(toSell.fixedCollider);
                gameManager.Money += (int)((int)toSell.getPrice() * toSell.sellValue());
                toSell.removeTower();
            }
        }
        else
        {
            TowerBase newTower = tower.createTower(tile, tower);
            collisionManager.addTower(newTower.fixedCollider);
            gameManager.Money -= (int)tower.getPrice();
        }
    }
}
