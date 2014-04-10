using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

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
    public static MapManager instance;
    //public static uint maxSpeed = 200; //This is the maximum speed a unit can move(Distance between two nodes)
    public static bool isClient; //When true, check with server for events
    public static bool isServer; //When true, send network updates to clients

    public List<List<MapTile>> grid = new List<List<MapTile>>();

    private Camera mapCam = null;
    private Camera guiCam = null;
    private GameObject towerMouseSprite = null;

    private MapTile goalTile = null;
    private MapTile spawnTile = null;

    private bool moveCamera = false; //Set to true when moving camera(Middle mouse button down)
    private bool inputOnGui; //Mouse over gui, ignore input
    private MapTile currentTile = null; //Current tile mouse is hovering over
    private TileHover hoverObj = null;  //GameObject placed over current tile hover
    private TowerBase currentPlaceTower = null; //Tower selected for placing by player

    public PathFinder walkingPath = new PFDijkstra4Dir();
    public PathFinder flyingPath = new PFDijkstra4Dir();

    public PlayerState currentPlayerState;

    public MapManager()
    {
        instance = this;
    }

    void Start()
    {
        mapCam = GameObject.Find("MapCam").camera;
        guiCam = GameObject.Find("GUICam").camera;
        towerMouseSprite = GameObject.Find("TowerMouseSprite");
        towerMouseSprite.SetActive(false); //Hide it until we select a tower

        currentPlayerState = PlayerState.Idle;

        //If mapData is not null, load it
        if (mapData != null)
        {
            loadMap(mapData);
        }

        //Load TileHover
        GameObject hoverPrefab = (GameObject)Resources.Load("GUI/TileHover");
        hoverObj = ((GameObject)Instantiate(hoverPrefab)).GetComponent<TileHover>();
        hoverObj.gameObject.SetActive(false); //Disable it until we hover over a tile

        //TestMap
        Debug.Log("Loading test map");

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
            Debug.LogError("No Goal tile found in testmap!");

        //Test Mob1
        Monster testMob = (Monster)createObject("Mobs/Mob1");
        Monster testMobFlying = (Monster)createObject("Mobs/Mob2_Flying");

        //Setup pathfinders
        walkingPath.init(goalTile, testMob, this);
        walkingPath.calculatePath();

        flyingPath.init(goalTile, testMobFlying, this);
        flyingPath.calculatePath();

        Destroy(testMob.gameObject); //Don't spawn it(Will be kept in memory for future reference by PathFinder)
        Destroy(testMobFlying.gameObject);

        gameRunning = true;
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
                    float offset = (currentPlaceTower.getSize() / 2f) - 0.5f;
                    towerMouseSprite.transform.position = new Vector3(currentTile.transform.position.x + offset, currentTile.transform.position.y + offset);
                }
            }

            //Handle mouse click
            if (Input.GetMouseButtonDown(0))
            {
                if (currentTile != null)
                {
                    //Debug.Log("Left click on tile: " + currentTile + "(X:" + currentTile.tileX + " Y:"+ currentTile.tileY + ")");
                    if (currentPlayerState == PlayerState.PlacingTower)
                    {
                        int towerSize = currentPlaceTower.getSize();
                        bool canBuild = true;

                        if (currentPlaceTower is TowerSell)
                        {
                            canBuild = false;
                            //TODO: Implement different selling mechanic, this feels too much like a hack, and can go wrong in so many ways later on
                            ITileObject obj = currentTile.getMapObject();
                            if (obj != null && obj is TowerBase)
                            {
                                Destroy(obj.getGameObject());
                                obj.getTileGroup().removeFromGroup();

                                walkingPath.calculatePath();
                                flyingPath.calculatePath();
                            }
                        }

                        for (int x = 0; x < towerSize && canBuild; x++)
                        {
                            for (int y = 0; y < towerSize; y++)
                            {
                                MapTile checkTile = getTile(currentTile.tileX + x, currentTile.tileY + y);
                                if (checkTile == null)
                                    continue;
                                if (!checkTile.canBuild(currentPlaceTower) || checkTile.getMapObject() != null)
                                {
                                    canBuild = false;
                                    break;
                                }
                            }
                        }

                        if (canBuild)
                        {
                            TowerBase newTower = (TowerBase)createObject(currentPlaceTower.gameObject);
                            newTower.getTileGroup().setGroup(currentTile, towerSize);

                            //Recalculate all paths
                            walkingPath.calculatePath();
                            flyingPath.calculatePath();

                            //See if any mobs now have no path to the end, if so destroy the placed tower(Or deny it in some way)
                            bool killTower = false;
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
                                    killTower = true;
                                    break;
                                }
                            }

                            //TODO: Check multiple spawns for multiple paths
                            //checkAllSpawn()
                            if (walkingPath.getNodeInfo(spawnTile).cost == -1)
                                killTower = true;
                            if (flyingPath.getNodeInfo(spawnTile).cost == -1)
                                killTower = true;

                            if (killTower)
                            {
                                Destroy(currentTile.getMapObject().getGameObject());
                                currentTile.getMapObject().getTileGroup().removeFromGroup();

                                //Recalculate all paths
                                //TODO: Use function recalculateAllPaths()
                                walkingPath.calculatePath();
                                flyingPath.calculatePath();
                            }
                        } //canBuild
                    }
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (currentPlayerState == PlayerState.PlacingTower)
                    startPlacingTower(null);
            }
        }


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
    public override MapObject addObject(MapObject obj)
    {
        obj.transform.parent = this.transform;
        obj.transform.localPosition = Vector3.zero;
        obj.init(this);
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
            return null;

        MapTile oldTile = grid[y][x];
        if (oldTile != null)
            Destroy(oldTile.gameObject);

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
}
