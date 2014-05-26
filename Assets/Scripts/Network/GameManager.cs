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
    Paused, //Game is paused
    Ended //Game has finished. Waiting for new map/ready
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    private MapManager map;
    private NetManager net;
    private GameState gameState;
    private UITextList currentChatOutput;

    //GUI
    private PreGamePanel preGame;
    private MessageBox infoBox; //Used to show "Waiting for ..."

    //Loaded map data(For map id 0 when authorative)
    private byte[] loadedMapData;

    //Game info
    private string gameName = "";
    private int maxPlayers = 1;

    //Temp Lives and money. Move this into per player/per team
    private int lives;
    private int money;
    private UILabel livesLabel;
    private UILabel moneyLabel;

    private Dictionary<uint, List<Action>> actionQueue = new Dictionary<uint, List<Action>>(); //Actions to be performed for specific tick
    public List<Action> outgoingActions = new List<Action>(); //Actions performed this step which are to be sent(Authorative)
    private Dictionary<uint, int> hashHistory = new Dictionary<uint, int>(); //Hash history for the different steps

    public List<Player> players = new List<Player>();
    public Player thisPlayer;
    public Player gameHost; //Hosting player(Authorative)
    public bool isAuthorative; //Whether this GamerManager is authorative(We are authorative player)

    private uint serverFixedStep = 0; //The last step update received from the authorative server
    private uint currentFixedStep = 0; //Our current step
    private bool waitingServerStep = false; //When true, waiting for server to progress game tick

    private MapInfo loadMapSceneChange = null;

    private bool doLaunch = false;
    private bool launched = false;

    //Disconnect error
    bool gotDisconnected = false;
    string disconnectTitle = "Disconnected from server";
    string disconnectMessage;

    bool normalExit = false; //When set to true, player exited, so we just kill GameManager

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
            MessagePlayerLeft.messageEvent += mgr.OnPlayerLeft;
            MessageChat.messageEvent += mgr.onChatMessage;
            MessageKickPlayer.messageEvent += mgr.onKickPlayer;
            MessageGameState.messageEvent += mgr.onSetGameState;
            MessageGameTick.messageEvent += mgr.onAuthorativeGameTick;
            MessageAction.messageEvent += mgr.onAction;
            MessageSyncCheck.messageEvent += mgr.onSyncCheck;
            GameChatInput.ChatEvent += mgr.onChatInput;

            //Set the NetManagers game to this
            mgr.net.setGame(mgr);

            //Pause the game until launched
            Time.timeScale = 0.0f;
        }

        Player localPlayer = Player.getLocalPlayer();
        mgr.players.Add(localPlayer);
        mgr.thisPlayer = localPlayer;
        if (isAuthorative)
            mgr.gameHost = localPlayer;

        mgr.gameName = gameName;
        mgr.maxPlayers = maxPlayers;
        mgr.gameState = GameState.Ended;

        mgr.currentFixedStep = 0;
        mgr.serverFixedStep = 0;

        if (maxPlayers == 1)
            mgr.launched = true;

        mgr.actionQueue[mgr.currentFixedStep+1] = new List<Action>();

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
        MessagePlayerLeft.messageEvent -= OnPlayerLeft;
        MessageChat.messageEvent -= onChatMessage;
        MessageKickPlayer.messageEvent -= onKickPlayer;
        MessageGameState.messageEvent -= onSetGameState;
        MessageGameTick.messageEvent -= onAuthorativeGameTick;
        MessageAction.messageEvent -= onAction;
        MessageSyncCheck.messageEvent -= onSyncCheck;
        GameChatInput.ChatEvent -= onChatInput;

        if (net == null)
            return;

        net.DisconnectEvent -= OnDisconnect;

        net.setGame(null);
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
        //List for exit(TODO: Replace with a proper menu)
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (net != null)
                net.closeConnection(net.mainSocket, DisconnectCause.Disconnect, "Player exited!");

            normalExit = true;
            Application.LoadLevel(0);
        }

        //Temp: Get Money and lives label
        //TODO: Move this on to a better system
        if(moneyLabel == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("MoneyLabel");
            if (obj != null)
                moneyLabel = obj.GetComponent<UILabel>();
            obj = GameObject.FindGameObjectWithTag("LivesLabel");
            if (obj != null)
                livesLabel = obj.GetComponent<UILabel>();
        }

        //When game is paused, we still need to update GameManager
        if(Time.timeScale == 0.0)
        {
            if(!launched)
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
        if (!launched)
        {
            if(!doLaunch)
                return;
            Time.timeScale = 1; //Start game logic
            launched = true;
        }

        if (gameState == GameState.PreGame || gameState == GameState.Paused)
            return;

        if(isAuthorative)
        {
            currentFixedStep++;

            //Perform all queued actions
            List<Action> currentActions = getActions(currentFixedStep);
            foreach (Action action in currentActions)
                action.run();

            //Remove actions
            actionQueue.Remove(currentFixedStep);

            //Send actions for this tick
            if (net != null)
            {
                foreach (Action action in outgoingActions)
                    net.broadcastAction(action);

                //Send tick update
                MessageGameTick tickMessage = new MessageGameTick(currentFixedStep);
                MessageBroadcast broadcast = new MessageBroadcast(tickMessage, false);
                net.sendMessage(net.mainSocket, broadcast);
            }
            outgoingActions.Clear();

            //Set-up next action queue
            actionQueue[currentFixedStep + 1] = new List<Action>();

            //Update all Map Objects
            map.updateMapObjects();
            //Do collision checks(Targeting)
            map.collisionManager.doCollisionCheck();

            //Hash in case of HashSync
            if((currentFixedStep % MapBase.simFramerate) == 0)
            {
                int hash = getMapHash();
                //Store it for later comparison
                hashHistory[currentFixedStep] = hash;
            }
        }
        else
        {
            //If we are bypassing the server, pause the game and wait for more data
            if (currentFixedStep + 1 > serverFixedStep)
            {
                if(!waitingServerStep)
                {
                    //Inform player
                    infoBox = MessageBox.createMessageBox("Waiting", "Waiting for server tick...");
                    infoBox.setButtonVisible(false);
                }

                //TODO: If we are hitting the limit a lot, allow it to fall a bit behind(Bad connection)

                waitingServerStep = true;
                Time.timeScale = 0;
            }
            else
            {
                //If too much behind, speed up the game a bit
                if (serverFixedStep - (currentFixedStep + 1) > 2)
                    Time.timeScale = 1.1f;
                else if (Time.timeScale > 1.0f) //TODO: Don't use timeScale to check this, as we want to be able to speed up the waves independently of this
                    Time.timeScale = 1.0f;

                currentFixedStep += 1;

                //TODO: Can server skip steps? If so this might throw. For example if joining game in progress.
                //List<Action> currentActions = actionQueue[currentFixedStep];
                List<Action> currentActions = getActions(currentFixedStep);
                if (currentActions != null)
                {
                    //Perform queued actions
                    foreach (Action action in currentActions)
                        action.run();

                    //Remove actions
                    actionQueue.Remove(currentFixedStep);
                }

                //Next action queue is set up when receiving the step from server


                //Update all Map Objects
                map.updateMapObjects();
                //Do collision checks(Targeting)
                map.collisionManager.doCollisionCheck();

                //Hash in case of HashSync
                if ((currentFixedStep % MapBase.simFramerate) == 0)
                {
                    int hash = getMapHash();

                    //Send it to host
                    MessageSyncCheck syncMessage = new MessageSyncCheck(currentFixedStep, hash);
                    MessageSendMessage message = new MessageSendMessage(gameHost, syncMessage);
                    net.sendMessage(net.mainSocket, message);
                }

            }
        }

    }

    //Get sync hash of all Map Objects
    public int getMapHash()
    {
        int hash = 0;
        foreach (MapObject obj in map.objectList)
            hash += obj.getSyncHash();
        return hash;
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

    public uint getCurrentFixedStep()
    {
        return currentFixedStep;
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

    //Server has progressed game tick
    void onAuthorativeGameTick(MessageGameTick message)
    {
        if (!isAuthorative)
        {
            //If we are a client, update server step
            serverFixedStep = message.getTick();
            //Debug.LogWarning("ServerGameTick: " + serverFixedStep);

            //Set-up action queue for step
            actionQueue[serverFixedStep+1] = new List<Action>();

            if (currentFixedStep + 1 > serverFixedStep)
            {
                //Assert: This should not happen. That would either mean the server tick didn't move or moved backwards. Or we moved ahead
                Debug.LogWarning("Strange server tick event: Didn't move? " + currentFixedStep + " | " + serverFixedStep);
                return;
            }

            if (waitingServerStep)
            {
                waitingServerStep = false;

                //Resume the game
                Time.timeScale = 1.0f;
                if (infoBox != null)
                    Destroy(infoBox.gameObject);
            }
        }
        else
        {
            //If we are the host, keep a watch on the other players step
            //(If they fall behind, we slow down the game a bit)
            //TODO:
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
                if (mapInfo.id == 0)
                {
                    MessageMapDownload mapDownload = new MessageMapDownload(loadedMapData);
                    MessageSendMessage newMessage = new MessageSendMessage(newPlayer, mapDownload);
                    if(!net.sendMessage(net.mainSocket, newMessage))
                    {
                        Debug.LogError("Failed to send map!");
                        //TODO: Disconnect?
                    }
                    Debug.Log("Sent map download");
                }
            }
        }

        players.Add(newPlayer);
        //If we are in PreGame
        if (gameState == GameState.PreGame)
            preGame.infoUpdated();

        chatAddSystemMessage(newPlayer.getName() + " joined the game!");
    }

    //Handle player leaving
    void OnPlayerLeft(MessagePlayerLeft message)
    {
        Player player = getPlayer(message.getPlayerId());
        if(player == null)
        {
            Debug.LogError("Got player left for player which was not in game: " + message.getPlayerId());
            return;
        }
        Debug.Log("Player left: " + player.getName());

        //TODO: Game logic of player leaving if we are in game

        players.Remove(player);

        if (gameState == GameState.PreGame)
            preGame.infoUpdated();

        chatAddSystemMessage(player.getName() + " left the game!");
    }

    //We have been kicked from the game
    void onKickPlayer(MessageKickPlayer message)
    {
        //Kick to menu, inform player of kick reason
        gotDisconnected = true;
        disconnectTitle = "Kicked from game";
        disconnectMessage = message.getReason();
        Application.LoadLevel(0);
    }

    //Action from player
    void onAction(MessageAction message)
    {
        byte[] actionData = message.getActionData();
        Actions actionType = message.getActionType();

        Player player = getPlayer(message.senderPlayerId);
        if (player == null)
        {
            Debug.LogError("Got action(" + actionType + ") from player which is not in game! Id: " + message.senderPlayerId);
            return;
        }

        bool valid;
        BinaryReader stream = new BinaryReader(new MemoryStream(actionData));
        Action theAction = Action.parseAction(stream, actionType, player, this, out valid);

        if (!valid)
            return; //TODO: If host, kick player

        Debug.Log("Got Action: " + theAction.action);

        //The action should itself verify permissions, so we simply queue the action for next tick
        if (isAuthorative)
            getActions(currentFixedStep + 1).Add(theAction);
        else
            getActions(serverFixedStep + 1).Add(theAction);
    }

    void onSyncCheck(MessageSyncCheck message)
    {
        if(!isAuthorative)
        {
            Debug.LogError("Got sync from player, when not authorative!");
            return;
        }

        int serverHash = hashHistory[message.getTick()];
        if(serverHash != message.getHash())
        {
            string syncMessage = "Game is out of sync at tick " + message.getTick() + "!!!  Server: " + serverHash + " Client: " + message.getHash() + "  Client: " + getPlayer(message.senderPlayerId);
            if (currentChatOutput != null)
                currentChatOutput.Add(syncMessage);

            MessageChat chatMessage = new MessageChat(syncMessage, false);
            MessageBroadcast broadcast = new MessageBroadcast(chatMessage, false);
            net.sendMessage(net.mainSocket, broadcast);

            //Debug.LogError(syncMessage);
        }

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

        //Debug.Log("Got chat: " + message.getMessage() + " Output: " + currentChatOutput);

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

        //Debug.Log("Send chat message");

        //Broadcast to all players
        MessageChat chatMessage = new MessageChat(message, false);
        MessageBroadcast broadcast = new MessageBroadcast(chatMessage, true);
        net.sendMessage(net.mainSocket, broadcast);

        //For now, wait to add it until we get the broadcast back from server, so the ordering is the same on all clients.
        /*if (currentChatOutput != null)
            currentChatOutput.Add("[" + Player.getLocalPlayer().getName() + "] " + message);*/
    }

    //Host has changed GameState
    void onSetGameState(MessageGameState message)
    {
        //Only listen to Host
        if (message.senderPlayerId != gameHost.getId())
        {
            Debug.LogError("Non-host tried to set Game State! Id: " + message.senderPlayerId);
            return;
        }

        setGameState(message.getGameState());
    }

    public void chatAddSystemMessage(string message)
    {
        if (currentChatOutput != null)
            currentChatOutput.Add("[F7FE2E][" + message + "][-]"); //TODO: Color it gray-ish
    }

    //Switch to new gamestate, and if authorative we inform the players. 
    public void setGameState(GameState newState)
    {
        if (newState == gameState)
            return;

        if(isAuthorative && net != null)
        {
            MessageGameState messageState = new MessageGameState(newState);
            MessageBroadcast broadcast = new MessageBroadcast(messageState, false);
            net.sendMessage(net.mainSocket, broadcast);
        }

        //Clean up existing game state
        switch(gameState)
        {
            case GameState.PreGame:
                //Close pregamepanel
                Destroy(preGame.gameObject);
                break;
        }

        //Set-up new state
        switch(newState)
        {
            case GameState.PreGame:
                //Stop game logic while in pre-game
                doLaunch = false;
                Time.timeScale = 0;
                //Show Pre-Game panel
                preGame = PreGamePanel.createPreGamePanel(GameObject.FindGameObjectWithTag("MessagePanel"), this);
                preGame.transform.localScale = new Vector3(2, 2, 1);
                preGame.transform.localPosition = new Vector3(0, 0, -10);
                break;
            case GameState.Running:
                if (gameState == GameState.PreGame)
                {
                    doLaunch = true;

                    //Create in-game chat
                    GameChat chat = GameChat.createGameChat(GameObject.FindGameObjectWithTag("MessagePanel"));
                    chat.transform.localPosition = new Vector3(-440, -296, 0);
                    setCurrentChatOutput(chat.output);

                    //Reset step counters
                    currentFixedStep = 0;
                    serverFixedStep = 0;
                }

                Lives = 100;
                Money = 10000;

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
        Debug.Log("LOADMAP");
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
        if(normalExit)
        {
            Destroy(gameObject);
            return;
        }

        if(gotDisconnected)
        {
            MessageBox box = MessageBox.createMessageBox(disconnectTitle, disconnectMessage);
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

    public int Lives
    {
        get { return lives; }
        set 
        {
            lives = value;
            if (livesLabel != null)
                livesLabel.text = "[B40404]Lives:[-] " + lives; 
        }
    }

    public int Money
    {
        get { return money; }
        set
        {
            money = value;
            if (moneyLabel != null)
                moneyLabel.text = "[FACC2E]Money:[-] " + money;
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

    //--In-Game Actions--

    public List<Action> getActions(uint step)
    {
        List<Action> list;
        if(!actionQueue.TryGetValue(step, out list))
        {
            list = new List<Action>();
            actionQueue[step] = list;
        }
        return list;
    }

    //Queue the action for next step
    public void queueAction(Action action)
    {
        if(isAuthorative)
        {
            //Queue for ourself to execute
            actionQueue[currentFixedStep + 1].Add(action);
        }
        else
        {
            //Send action(Queue at Host)
            net.sendAction(action, gameHost);
            Debug.Log("Sent action: " + action.action);
        }
    }

    //Local request to place tower
    public void placeTower(MapTile tile, TowerBase tower)
    {
        if(!isAuthorative)
        {
            if (!map.clientPlaceTower(tile, tower))
                return;
        }
        ActionPlaceTower placeTowerAction = new ActionPlaceTower(this, tile, tower, thisPlayer);
        queueAction(placeTowerAction);
    }

    public NetManager getNetManager()
    {
        return net;
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