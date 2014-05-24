using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

//Acts as the manager for network games.
/*
 * For clients, any input decision they do will be sent to the GameManager, who will relay it to the authorative game manager(server).
 * The game manager will then relay any decisive decisions(Tick, creation, destruction, message etc.) back to the MapManager.
 * 
 * For server, all external(client) gameManagers will speak to this authorative game manager. This game manager again will speak to the hosts
 * MapManager to request actions, and get results.
 * 
 * Example:
 * clientPlaceTower(Input on MapManager) -> placeTower(Local GameManager) -> network message -> requestPlaceTower(Server GameManager) -> authorativePlaceTower(Authorative MapManager) -> respond with failure if not possible
 * authorativePlaceTower(Authorative MapManager) -> placeTower(Server GameManager) -> network message(broadcast) -> requestPlaceTower(Local GameManager) -> serverPlaceTower(Client MapManager) 
 * Plase note from example, if placing tower did not fail, we don't send a specific success message, since the server will broadcast the placing of the tower anyway.
 * 
 * 
 * 
 * NOTE: All confirmed actions only happen in batches as the step increments.
 * So if a player places 3 towers in less than the time of a step, no reply will come untill the next step, bundled with any other
 * actions by other players.
 * Depending on lag it might therefore take some time until tower placements actually happen(~lagms + 50 ms with 20 FPS, worst case).
 * TODO: Maybe show shadow success(Place tower sprite, and replace with functional tower once confirmed. Might look weird with monsters walking through tho)
 */
public enum GameState
{
    PreGame, //Waiting for players to join and ready up
    Running, //Game is in progress
    Ended //Game has finished. Waiting for new map/ready
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    private MapManager map;
    private NetManager net;
    private GameState gameState;
    private PreGamePanel preGame;
    private UITextList currentChatOutput;

    //Loaded map data(For map id 0 when authorative)
    private byte[] loadedMapData;

    //Game info
    private string gameName = "";
    private int maxPlayers = 1;

    private List<Action> actionQueue = new List<Action>(); //Actions to be performed for next tick

    public List<Player> players = new List<Player>();
    public Player thisPlayer;
    public Player gameHost; //Hosting player(Authorative)
    public bool isAuthorative; //Whether this GamerManager is authorative(We are authorative player)

    private uint serverFixedStep; //The last step update received from the authorative server
    private uint currentFixedStep; //Our current step

    private MapInfo loadMapSceneChange = null;

    //Disconnect error
    bool gotDisconnected = false;
    string disconnectMessage;

    //Constructor multiplayer
    public static GameManager createGameManager(NetManager net, bool isAuthorative, MapManager map = null, string gameName = "", int maxPlayers = 1)
    {
        GameManager mgr;

        GameObject prefab = Resources.Load<GameObject>("Network/GameManager");
        if (prefab == null)
            Debug.LogError("Failed to get GameManager prefab");
        GameObject newObj = (GameObject)Instantiate(prefab);
        mgr = newObj.GetComponent<GameManager>();
        if (mgr == null)
            Debug.LogError("Failed to get GameManager from instance");

        mgr.net = net;
        mgr.isAuthorative = isAuthorative;
        mgr.map = map;

        if(mgr.net != null)
        {
            //Look for events
            mgr.net.DisconnectEvent += mgr.OnDisconnect;
            MessagePlayerJoined.messageEvent += mgr.OnPlayerJoined;
            MessageChat.messageEvent += mgr.onChatMessage;
            GameChatInput.ChatEvent += mgr.onChatInput;
        }

        Player localPlayer = Player.getLocalPlayer();
        mgr.players.Add(localPlayer);
        mgr.thisPlayer = localPlayer;
        if (isAuthorative)
            mgr.gameHost = localPlayer;

        mgr.gameName = gameName;
        mgr.maxPlayers = maxPlayers;
        mgr.gameState = GameState.Ended;

        //TODO: Get remote host and players when joining

        instance = mgr;
        DontDestroyOnLoad(mgr.gameObject);

        return mgr;
    }

    void OnDestroy()
    {
        Time.timeScale = 1.0f;

        //Cleanup events
        MessagePlayerJoined.messageEvent -= OnPlayerJoined;
        MessageChat.messageEvent -= onChatMessage;
        GameChatInput.ChatEvent -= onChatInput;

        if (net == null)
            return;
        net.DisconnectEvent -= OnDisconnect;
    }

    //Called on joining game(For now called by the join button code)
    public void onJoin(MessageJoinGameReply message)
    {
        //Clear any potentially old data
        players.Clear();
        serverFixedStep = 0;

        gameName = message.getGameName();
        maxPlayers = message.getMaxPlayers();

        gameHost = message.getOwner();
        players.AddRange(message.getPlayers()); //We are included in the message.getPlayers()
    }

    void Update()
    {
        //When game is paused, we still need to update GameManager
        if(Time.timeScale == 0.0)
        {
            FixedUpdate();
            //Check for new network messages since NetManager also runs on FixedUpdate
            if (net != null)
            {
                net.SendMessage("FixedUpdate");
            }
        }
    }

    public void FixedUpdate()
    {
        Monster.lastMonsterId = 0;

        if(isAuthorative)
        {
            //If authorative, send step update to everyone in game
            //Gather everything that has happened the last step, and send it(Tower placement etc.)
            //MessageBroadcast broadcast = new MessageBroadcast();

            //Perform all queued actions
            foreach(Action action in actionQueue)
            {
                action.run();
            }
            //TODO: Actions might cause actions to be added?
            actionQueue.Clear();
        }
        else
        {
            //If we are bypassing the server, pause the game and wait for more data
            if(currentFixedStep+1 > serverFixedStep)
                Time.timeScale = 0;
            else
                currentFixedStep += 1;
        }
    }

    //Constructor single player
    public static GameManager createGameManager(MapManager map = null)
    {
        return createGameManager(null, true, map);
    }

    public static GameManager getGameManager()
    {
        return instance;
    }

    //Called whenever connection to the main server is lost
    void OnDisconnect(Socket socket, DisconnectCause cause, string message)
    {
        if(isAuthorative && maxPlayers == 1)
        {
            //Inform the player that we have lost connection to server
            //TODO: Try to re-connect
            MessageBox.createMessageBox("Lost Connection", "Lost connection to main server! Online features are disabled.\n" + message);
        }
        else
        {
            //Kick to menu, inform player of disconnect, then destroy GameManager
            gotDisconnected = true;
            disconnectMessage = message;
            Application.LoadLevel(0);
        }
    }

    //Handle new player joining
    void OnPlayerJoined(MessagePlayerJoined message)
    {
        Debug.Log("Player joined: " + message.getPlayerName());

        Player newPlayer = new Player(message.getPlayerName(), message.getPlayerId());

        //If We are host, and the map is a 0-id, send the map to the new player
        if(isAuthorative)
        {
            if (map == null || loadedMapData == null)
            {
                MessageBox.createMessageBox("Error", "Tried to send map to joining player, but map was not in memory.");
                //TODO: Kick new player
            }
            else
            {
                MapInfo mapInfo = map.getMapInfo();
                Debug.Log("Map Id:" + mapInfo.id);
                if (mapInfo.id == 0)
                {
                    Debug.Log("Sent map download");
                    MessageMapDownload mapDownload = new MessageMapDownload(loadedMapData);
                    MessageSendMessage newMessage = new MessageSendMessage(newPlayer, mapDownload);
                    net.sendMessage(net.mainSocket, newMessage);
                }
            }
        }

        players.Add(newPlayer);
        //If we are in PreGame
        if (gameState == GameState.PreGame)
            preGame.infoUpdated();

        if(currentChatOutput != null)
            currentChatOutput.Add("[" + newPlayer.getName() + " joined the game!]");
    }

    //Chat message from other player
    void onChatMessage(MessageChat message)
    {
        Player sender = getPlayer(message.senderPlayerId);
        if (sender == null)
        {
            Debug.LogError("Got chat message from player who is not in game: " + message.senderPlayerId);
            return;
        }

        Debug.Log("Got chat: " + message.getMessage() + " Output: " + currentChatOutput);

        string chatMessage = message.getMessage();
        //TODO: Filter message for bad stuff

        if (currentChatOutput != null)
            currentChatOutput.Add("[" + sender.getName() + "] " + chatMessage);
    }

    //Input from local player
    void onChatInput(string message)
    {
        if (net == null)
            return;

        Debug.Log("Send chat message");

        //Broadcast to all players
        MessageChat chatMessage = new MessageChat(message, false);
        MessageBroadcast broadcast = new MessageBroadcast(chatMessage, true);
        net.sendMessage(net.mainSocket, broadcast);

        //For now, wait to add it until we get the broadcast back from server, so the ordering is the same on all clients.
        /*if (currentChatOutput != null)
            currentChatOutput.Add("[" + Player.getLocalPlayer().getName() + "] " + message);*/
    }


    public void setGameState(GameState newState)
    {
        if (newState == gameState)
            return;

        //Clean up existing game state
        switch(gameState)
        {
            case GameState.PreGame:
                //TODO: Close pregamepanel
                break;
        }

        //Set-up new state
        switch(newState)
        {
            case GameState.PreGame:
                //Stop game logic while in pre-game
                Time.timeScale = 0;
                //Show Pre-Game panel
                preGame = PreGamePanel.createPreGamePanel(GameObject.FindGameObjectWithTag("MessagePanel"), this);
                preGame.transform.localScale = new Vector3(2, 2, 1);
                preGame.transform.localPosition = new Vector3(0, 0, -10);
                break;
            case GameState.Running:
                if (gameState == GameState.PreGame)
                    Time.timeScale = 1; //Start game logic
                break;
        }

        gameState = newState;
    }

    public GameState getGameState()
    {
        return gameState;
    }

    //Load a new map. 
    //If authorative, send changemap message to clients.
    public void loadMap(MapInfo newMap)
    {
        //TODO: Handle online map

        //Load local map
        AsyncFileReader mapLoader = new AsyncFileReader(MapInfo.getMapsPath() + newMap.filename);
        byte[] mapData = mapLoader.getData();
        if(mapData == null)
        {
            Exception ex = mapLoader.getError();
            MessageBox.createMessageBox("Error loading map", ex.Message + "\n" + ex.StackTrace);
            Debug.LogException(ex);
            return;
        }

        loadMap(new MemoryStream(mapData, 0, mapData.Length, true, true), newMap);
    }

    //Called once the map is in memory
    //Note: stream must have publiclyVisible constructor = true for authorative
    public void loadMap(MemoryStream stream, MapInfo mapInfo)
    {
        if (isAuthorative && mapInfo.id == 0 && maxPlayers > 1)
            loadedMapData = stream.GetBuffer(); //Store for sending to players

        if (isAuthorative)
        {
            //TODO: Changemap message
            //TODO: Send map data if id = 0
        }

        //Load map, create MapManager if missing
        if (map == null)
        {
            map = MapManager.createMapManager(this);
        }
        else
        {
            map.clearMap();
        }

        try
        {
            BinaryReader binaryStream = new BinaryReader(stream);
            if (!map.loadMap(binaryStream))
            {
                return;
            }
        }
        catch (Exception ex)
        {
            MessageBox.createMessageBox("Error loading map", ex.Message + "\n" + ex.StackTrace);
            Debug.LogException(ex);
            return;
        }

        //If online game, go to PreGame
        if (maxPlayers > 1)
            setGameState(GameState.PreGame);
        else
            setGameState(GameState.Running);
    }

    //Load map after scene change
    public void loadMapOnSceneChange(MapInfo newMap)
    {
        loadMapSceneChange = newMap;
    }

    void OnLevelWasLoaded(int level)
    {
        if(gotDisconnected)
        {
            MessageBox box = MessageBox.createMessageBox("Lost Connection", "Lost connection to server: " + disconnectMessage);
            Destroy(gameObject);
            return;
        }

        if (loadMapSceneChange != null)
        {
            if (loadMapSceneChange.mapData == null)
                loadMap(loadMapSceneChange);
            else
                loadMap(new MemoryStream(loadMapSceneChange.mapData), loadMapSceneChange);
            loadMapSceneChange = null;
        }
    }

    public MapManager getMap()
    {
        return map;
    }

    //Return player in game with the given userid
    //Return: Player if found, null if not
    public Player getPlayer(int id)
    {
        foreach(Player player in players)
        {
            if (player.getId() == id)
                return player;
        }

        return null;
    }

    //In-Game Actions

    //Local request to place tower
    public void placeTower(MapTile tile, TowerBase tower)
    {
        if(isAuthorative)
        {
            //Create action and add to queue
            ActionPlaceTower placeTowerAction = new ActionPlaceTower(this, tile, tower, thisPlayer);
            actionQueue.Add(placeTowerAction);
        }
        else
        {
            if (map.clientPlaceTower(tile, tower))
            {
                //Create action and send to authorative GameManager
            }
        }
    }

    public void setCurrentChatOutput(UITextList chatOutput)
    {
        currentChatOutput = chatOutput;
    }

    public UITextList getCurrentChatOutput()
    {
        return currentChatOutput;
    }

    //Game Info
    public void setGameName(string name)
    {
        gameName = name;
    }

    public String getGameName()
    {
        return gameName;
    }

}