//The base class that handles the networking
/*
 * Connecting to server
 * Creating server
 * Send messages
 * Parse messages
 * 
 * Also handles using external plugin for networking on Mobile
 * */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public enum MainServerState
{
    NotConnected,
    ConnectionFailed,
    Disconnected,
    Connecting,
    Connected,
    LoggingIn,
    LoggedIn,
    LoginFailed
}

public enum GameServerState
{
    Hosting,
    NotConnected,
    ConnectionFailed,
    Disconnected,
    Connecting,
    Connected,
    Joining,
    InGame
}

//Causes for disconnecting from server
public enum DisconnectCause
{
    LostConnection, //Lost connection to server
    Timeout, //Timeout while connecting
    UnknownError, //Disconnecting due to error with no defined cause
    CorruptMessage, //Got corrupt message
    UnknownMessageState, //Got unknown results from parsing message
    NoMessageParser, //No parser found for message
    Disconnect //Disconnecting due to user action or other normal causes
}

//Network error resulting in disconnect(Used by events hapening outside main thread)
//TODO: Expand this to include errors other than disconnects
public class NetworkError
{
    public Socket socket;
    public DisconnectCause cause;
    public string message;

    public NetworkError(Socket socket, DisconnectCause cause, string message = "None")
    {
        this.socket = socket;
        this.cause = cause;
        this.message = message;
    }
}

//Data used when sending messages
public class MessageSendData
{
    public Socket socket;
    public byte[] data;

    public MessageSendData(Socket socket, byte[] data)
    {
        this.socket = socket;
        this.data = data;
    }
}

public class ReceiveState
{
    public Socket socket;
    public byte[] buffer;

    public ReceiveState(Socket socket, int receiveBufferSize)
    {
        this.socket = socket;
        buffer = new byte[receiveBufferSize];
    }
}

public class NetManager : MonoBehaviour
{
    private static NetManager instance;
    private GameManager game;

    public static string mainIp = "127.0.0.1";
    //public static string mainIp = "95.85.53.18";
    public static int mainPort = 12000;

    public Socket mainSocket;

    public MainServerState mainState = MainServerState.NotConnected;

    //TODO: Implement error list/queue
    public string mainServerError = ""; //Error messages when dealing with main server
    public string gameServerError = ""; //Error message when dealing with game server
    public bool canRegisterLogHandler = true; //Set to true when the log handler needs to be re-registred(Scene change)

    private string username = "";
    private bool server = false;
    private bool loggedIn = false;

    private DateTime connectStart; //Set when starting connecting to server, used for timeout
    private int connectTimeoutTime = 5; //How long in seconds to try to connect before timing out

    //Thread locks/messaging
    private ManualResetEvent mainConnectEvent = new ManualResetEvent(false);
    private ManualResetEvent sendMessageLock = new ManualResetEvent(true); //When true, send next message
    public ConcurrentQueue<Message> messageReceiveQueue = new ConcurrentQueue<Message>(); //Messages from server
    public ConcurrentQueue<Message> messageSendQueue = new ConcurrentQueue<Message>(); //Messages to server
    private ConcurrentQueue<NetworkError> networkErrorQueue = new ConcurrentQueue<NetworkError>(); //Network errors happening on worker threads(Send/Receive)

    private byte[] receiveBuffer = new byte[0]; //Buffer used for received data while building messages

    //Events
    public delegate void DisconnectEventHandler(Socket socket, DisconnectCause cause, string message); //Disconnected from main server
    public event DisconnectEventHandler DisconnectEvent;

    public delegate void GameExitEventHandler(GameManager game, string message); //Exited from current game
    public event GameExitEventHandler GameExitEvent;

    //Message Events
    public static Dictionary<MessageCommand, MessageEventBase> messageEvents = new Dictionary<MessageCommand, MessageEventBase>();
    public delegate void AnyMessageEventHandler(Message message);
    public event AnyMessageEventHandler AnyMessageEvent; //Event called on every message

    //Add new message event procedure:
    //1. Create message
    //2. Register parser in Message.cs static constructor

    //Return an instance of netmanager. Create it if it doesn't exist(create=true)
    public static NetManager getNetManager(bool create = true)
    {
        NetManager manager = instance;
        if(manager == null && create)
        {
            GameObject prefab = Resources.Load<GameObject>("Network/NetworkManager");
            if(prefab == null)
            {
                Debug.LogError("Failed to get NetworkManager resource");
                return null;
            }
            GameObject newObj = (GameObject)Instantiate(prefab);
            if(newObj == null)
            {
                Debug.LogError("Failed to instantiate NetworkManager");
                return null;
            }
            manager = newObj.GetComponent<NetManager>();
            instance = manager;
        }
        return manager;
    }

    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        //Only have one NetManager up at any time
        if (GameObject.FindObjectsOfType<NetManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        //Register event handlers
        MessageLoginReply.messageEvent += onLoginReply;
        MessageSendMessage.messageEvent += onMessageInMessage;
        MessageBroadcast.messageEvent += onMessageInMessage;
        DisconnectEvent += onDisconnect;

        //Connect to main server
        //connectToMainServer("82.194.218.201", 12000);
        connectToMainServer(mainIp, mainPort);

        registerLogHandler();
    }

    void OnDestroy()
    {
        if (mainSocket != null)
            mainSocket.Close();
    }

    void OnLevelWasLoaded(int level)
    {
        canRegisterLogHandler = true;
    }

    private void registerLogHandler()
    {
        if (!canRegisterLogHandler)
            return;
        //Forward Log output to the main server
        Application.RegisterLogCallbackThreaded(LogHandler);
        canRegisterLogHandler = false;
    }

    public void LogHandler(string logString, string stackTrace, LogType type)
    {
        if (mainSocket == null || !mainSocket.Connected)
            return;

        /*if (type == LogType.Warning)
            return;*/

        MessageLog logMessage;
        switch(type)
        {
            case LogType.Exception:
                logMessage = new MessageLog("Exception", logString + "\n\n" + stackTrace);
                break;
            default:
                logMessage = new MessageLog(type.ToString(), logString);
                break;
        }

        sendMessage(mainSocket, logMessage, false);
    }

    void Update()
    {
        registerLogHandler();

        if(mainState == MainServerState.Connecting)
        {
            //Check if we have connected
            if(mainConnectEvent.WaitOne(0))
                onMainServerConnected();

            //Check if we have reached timeout
            if((DateTime.Now - connectStart).TotalSeconds > connectTimeoutTime)
            {
                mainState = MainServerState.ConnectionFailed;
                mainServerError = "Timed out while connecting to main server. Server might be offline!";
                mainSocket.Close();
            }
        }

        //Check network error queue
        //TODO: Handle more than disconnects
        List<NetworkError> networkErrors = networkErrorQueue.popAll();
        foreach(NetworkError error in networkErrors)
        {
            closeConnection(error.socket, error.cause, error.message);

            //For now we just handle the first error and ignore the rest, since we only handle disconnects.
            break;
        }

        //If done sending previous message, start sending next one
        handleSendingMessage();
    }

    //Handle new messages before fixed game logic
    void FixedUpdate()
    {
        //Check for new messages
        handleNewMessages();
    }

    public GameManager getGame()
    {
        if (game == null)
            Debug.LogError("Assert: Tried to get game from net, but it was null at this point!");
        return game;
    }

    public void setGame(GameManager game)
    {
        this.game = game;
    }

    public bool connectToMainServer(string ip, int port)
    {
        Debug.Log("Try connect");
        if (mainSocket != null && mainSocket.Connected)
            mainSocket.Disconnect(true);
        else
        {
            try
            {
                mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch(SocketException e)
            {
                mainState = MainServerState.ConnectionFailed;
                mainServerError = "Failed to create IPv4 TCP socket.";
                Debug.LogError(e);
                return false;
            }
        }

        Debug.Log("Start connecting to server: " + ip + ":" + port);
        mainState = MainServerState.Connecting;
        mainConnectEvent.Reset();

        connectStart = DateTime.Now;
        mainSocket.BeginConnect(ip, port, new AsyncCallback(callbackConnectToMainServer), mainSocket);

        return true;
    }

    //Handle connecting to the main server
    private void callbackConnectToMainServer(IAsyncResult ar)
    {
        Socket client = (Socket)ar.AsyncState;
        try
        {
            client.EndConnect(ar);
        }
        catch(Exception e)
        {
            mainServerError = e.ToString();
            return;
        }
        mainServerError = null;
        mainConnectEvent.Set();
    }

    //Called in main thread after connecting to the main server
    private void onMainServerConnected()
    {
        if(mainServerError != null)
        {
            mainState = MainServerState.ConnectionFailed;
            Debug.Log("Failed to connect: " + mainServerError);
            return;
        }

        mainSocket.NoDelay = true;

        mainState = MainServerState.Connected;
        Debug.Log("Connected to server: " + mainSocket.RemoteEndPoint.ToString());

        //Start setting up receiving of messages
        ReceiveState receiveState = new ReceiveState(mainSocket, 1500);
        mainSocket.BeginReceive(receiveState.buffer, 0, receiveState.buffer.Length, 0, new AsyncCallback(callbackReceive), receiveState);
    }

    //Try to send message from sending queue
    //TODO: Move onto another thread so it doesn't just send one message per update, or start next send inside handler of previous(Worker thread)
    private void handleSendingMessage()
    {
        if (!canSend())
        {
            messageSendQueue.clear(); //We don't allow queuing messages when not in a state to send
            return; //TODO: Add to network error queue
        }

        if(sendMessageLock.WaitOne(0))
        {
            Message message = messageSendQueue.pop();

            if (message != null)
            {
                //Debug.LogWarning("Handle send Message: " + message.getCommand());

                byte[] messageData = message.getBytes();

                if(messageData.Length > ushort.MaxValue-1)
                {
                    //Split?
                    Debug.LogError("Message too large: " + messageData.Length);
                    return;
                }

                //Add message length to the front
                byte[] messageLength = BitConverter.GetBytes((ushort)(messageData.Length));

                byte[] sendData = new byte[messageData.Length + 2];
                Array.Copy(messageLength, 0, sendData, 0, 2);
                Array.Copy(messageData, 0, sendData, 2, messageData.Length);

                sendMessageLock.Reset();

                MessageSendData customData = new MessageSendData(message.socket, sendData);

                //Debug.LogWarning("Sending " + sendData.Length + " bytes(C: " + message.getCommand() + ")");
                message.socket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, new AsyncCallback(callbackSendMessage), customData);
            }
        }

    }

    //Check if connection to main server is in a state to allow sending messages
    private bool canSend()
    {
        if (mainState == MainServerState.Connecting || mainState == MainServerState.ConnectionFailed || mainState == MainServerState.Disconnected
            || mainState == MainServerState.NotConnected)
            return false;

        return true;
    }

    //Get new messages, and call their events
    private void handleNewMessages()
    {

        List<Message> newMessages = messageReceiveQueue.popAll();
        foreach(Message newMessage in newMessages)
        {
            if (newMessage == null)
                Debug.LogError("Got null message on new messages!");

            //Check permissions, so players can not send messages only MainServer is allowed to send
            if(newMessage.getPermission() == MessagePermission.MainServer && newMessage.senderPlayerId != -1)
            {
                Debug.LogError("Got message from player that only MainServer is allowed to send. Command: " + newMessage.getCommand());
                continue;
            }

            try
            {
                //Call specific handlers first
                MessageEventBase messageEvent = null;
                messageEvents.TryGetValue(newMessage.getCommand(), out messageEvent);

                if (messageEvent != null)
                    messageEvent.Dispatch(newMessage);

                //Call generic handlers last
                if (AnyMessageEvent != null)
                    AnyMessageEvent(newMessage);
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
                closeConnection(mainSocket, DisconnectCause.CorruptMessage, "Error while handling network message event(" + newMessage.getCommand() + "). Might be a corrupt game state or network error.\n" + ex.Message);
                newMessages.Clear();
                return;
            }
        }
    }

    //Handle new data from socket
    private void callbackReceive(IAsyncResult ar)
    {
        ReceiveState state = (ReceiveState)ar.AsyncState;

        if (!state.socket.Connected)
            return;

        try
        {
            int bytesReceived = state.socket.EndReceive(ar);
            //Debug.Log("Received bytes: " + bytesReceived);
            if(bytesReceived > 0)
            {
                //Create new receiveBuffer, copy remaining old data, copy new buffer data,
                //and check this new buffer for a message.
                int newBufferSize = receiveBuffer.Length + bytesReceived;
                byte[] newBuffer = new byte[newBufferSize];
                receiveBuffer.CopyTo(newBuffer, 0);
                Array.Copy(state.buffer, 0, newBuffer, receiveBuffer.Length, bytesReceived);
                receiveBuffer = newBuffer;

                //Parse buffer to get a message
                List<Message> parsedList = new List<Message>();
                Message parsedMessage = null;
                bool continueParse = true;
                int messageSize = 0;

                while (continueParse)
                {
                    ParseResult parseResult = ParseResult.Corrupt;
                    try
                    {
                        parseResult = Message.parseMessage(receiveBuffer, out parsedMessage, out messageSize);
                    }
                    catch(Exception ex)
                    {
                        Debug.LogException(ex);
                        closeConnection(mainSocket, DisconnectCause.CorruptMessage, "Error while parsing network message. Might be a corrupt game state or network error.\n" + ex.Message);
                        return;
                    }

                    switch (parseResult)
                    {
                        case ParseResult.Done:
                            if (parsedMessage == null)
                                Debug.LogError("Got null message from parser!");
                            messageReceiveQueue.push(parsedMessage);

                            //Create a new buffer with the read data removed, as we might have received part or whole of the next message
                            //TODO: move this outside the while loop to avoid multiple allocations and moving of data when multiple messages received.
                            messageSize += Message.messageLengthSize; //Include the size prefix
                            newBuffer = new byte[receiveBuffer.Length - messageSize];
                            Array.Copy(receiveBuffer, messageSize, newBuffer, 0, receiveBuffer.Length - messageSize);
                            receiveBuffer = newBuffer;

                            break;
                        case ParseResult.NeedMoreData:
                            //Do nothing, let it ask for more data
                            continueParse = false;
                            break;
                        case ParseResult.Corrupt:
                            //Something went wrong. Break connection and show message
                            //TODO: Control and show error
                            continueParse = false;
                            closeConnection(state.socket, DisconnectCause.CorruptMessage, "Got corrupt message");
                            break;
                        case ParseResult.NoParser:
                            continueParse = false;
                            closeConnection(state.socket, DisconnectCause.NoMessageParser, "Got message with no parser to handle it!");
                            break;
                        default:
                            Debug.LogError("Got unknown results from parsing message");
                            continueParse = false;
                            closeConnection(state.socket, DisconnectCause.UnknownMessageState, "Got unknown results from parsing message!");
                            break;
                    }
                }

            }
            else
            {
                //Got no data, end of connection?
                Debug.Log("Got no data on receive!");
            }

            if (state.socket.Connected != true)
                return; //Stop receiving data

            //Wait for more data
            mainSocket.BeginReceive(state.buffer, 0, state.buffer.Length, 0, new AsyncCallback(callbackReceive), state);
        }
        catch (SocketException ex)
        {
            networkErrorQueue.push(new NetworkError(state.socket, DisconnectCause.UnknownError, ex.Message));
            return;
        }
        catch (ObjectDisposedException)
        {
            //Connection has been closed
            networkErrorQueue.push(new NetworkError(state.socket, DisconnectCause.LostConnection));
            return;
        }
    }

    //Manage state changes with main server
    public void setMainState(MainServerState newState)
    {
        //TODO: Make sure we maintain stability with state changes.
        //For example, state change from connected to any non-connected state should always run a socket.disconnect

        //From
        switch(mainState)
        {
            case MainServerState.LoggedIn:
                loggedIn = false;
                break;
        }

        //To
        switch(newState)
        {
            case MainServerState.LoggedIn:
                loggedIn = true;
                break;
        }

        mainState = newState;
    }

    //Manage state changes with game server connection
    public void setGameState(GameServerState newState)
    {
    }

    public bool sendMessage(Socket target, Message message, bool sendNow = true)
    {
        //Debug.LogWarning("sendMessage1");
        if (!canSend())
            return false;

        if (!target.Connected)
            throw new Exception("Tried to send message to disconnected socket");

        message.socket = target;
        messageSendQueue.push(message);

        //Debug.LogWarning("SendMessage, Queue: " + messageSendQueue.count());

        if (sendNow)
            handleSendingMessage(); //Try to send it now

        return true;
    }

    //Send an action to a player
    public bool sendAction(Action action, Player player)
    {
        MessageAction actionMessage = new MessageAction(action);
        MessageSendMessage message = new MessageSendMessage(player, actionMessage);
        return sendMessage(mainSocket, message);
    }

    //Send an action to all players
    public bool broadcastAction(Action action)
    {
        MessageAction actionMessage = new MessageAction(action);
        MessageBroadcast broadcast = new MessageBroadcast(actionMessage, false);
        return sendMessage(mainSocket, broadcast);
    }

    public void callbackSendMessage(IAsyncResult ar)
    {
        MessageSendData data = (MessageSendData)ar.AsyncState;
        Socket client = data.socket;

        //Debug.LogWarning("Start send message");

        if (!client.Connected)
        {
            Debug.LogWarning("Aborted send due to client not connected.");
            return;
        }

        int bytesSent = 0;
        try
        {
            bytesSent = client.EndSend(ar);
            //Debug.LogWarning("Send complete: " + bytesSent);
        }
        catch(SocketException ex)
        {
            networkErrorQueue.push(new NetworkError(client, DisconnectCause.UnknownError, ex.Message));
            Debug.LogWarning("Send error: " + ex.Message);
            return;
        }
        catch(ObjectDisposedException)
        {
            //Connection has been closed
            networkErrorQueue.push(new NetworkError(client, DisconnectCause.LostConnection));
            Debug.LogWarning("Send error: Lost connection");
            return;
        }

        int remainingLength = data.data.Length - bytesSent;
        if (remainingLength > 0)
        {
            //Re-send remaining data
            byte[] remaining = new byte[remainingLength];
            Array.Copy(data.data, bytesSent, remaining, 0, remainingLength);
            data.data = remaining;
            Debug.LogWarning("Continue to send remaining: " + remainingLength);
            client.BeginSend(remaining, 0, remaining.Length, SocketFlags.None, new AsyncCallback(callbackSendMessage), data);
        }
        else
        {
            //We are done sending this message
            //Debug.Log("Full message sent");
            sendMessageLock.Set();
        }
    }

    //Send a message to a player in the current game
    public void gameSendMessage(Player recipient, Message message)
    {
        if (GameManager.getGameManager() == null)
            return;

        MessageSendMessage messageSend = new MessageSendMessage(recipient, message);
        sendMessage(mainSocket, messageSend);
    }

    //Login with the given name
    public bool login(string name)
    {
        if (mainState != MainServerState.Connected)
            return false;

        Message loginMessage = new MessageLogin(name, "loladmin");
        if(!sendMessage(mainSocket, loginMessage))
            return false;
        setMainState(MainServerState.LoggingIn);
        username = name;
        Player.setLocalPlayer(new Player(username, -1, Player.PlayerState.Offline));

        return true;
    }

    //Login Reply event handler
    private void onLoginReply(MessageLoginReply message)
    {
        if (message.loginOk())
        {
            //Update local player
            Player localPlayer = Player.getLocalPlayer();
            localPlayer.setId(message.getPlayerId());
            localPlayer.setState(Player.PlayerState.Online);
            
            setMainState(MainServerState.LoggedIn);
        }
        else
        {
            setMainState(MainServerState.LoginFailed);
            mainServerError = message.getErrorMessage();
            Player.setLocalPlayer(null);
        }
    }

    //Handler for messages contained inside messages
    private void onMessageInMessage(Message message)
    {
        IMessageInMessage container = message as IMessageInMessage;
        if (container == null)
            return;

        messageReceiveQueue.push(container.getMessage());
    }

    //Close connection with closing message
    public void closeConnection(Socket socket, DisconnectCause cause, string message = "No reason")
    {
        if(cause == DisconnectCause.LostConnection)
            message = "Lost connection to server!";

        //Inform event handlers of disconnect
        if (DisconnectEvent != null)
            DisconnectEvent(socket, cause, message);

        socket.Close();
    }

    //Handle main server disconnects
    private void onDisconnect(Socket socket, DisconnectCause cause, string message)
    {
        loggedIn = false;

        MessageBox.createMessageBox("Network Error", message);

        Debug.Log("Cause: " + cause);

        //Logout player
        Player.setLocalPlayer(null);

        if (socket != mainSocket)
            return;

        switch(cause)
        {
            case DisconnectCause.Disconnect:
                mainState = MainServerState.NotConnected;
                break;
            case DisconnectCause.LostConnection:
                mainState = MainServerState.Disconnected;
                break;
            default:
                mainState = MainServerState.ConnectionFailed;
                mainServerError = message;
                break;
        }
    }

    public string getUsername()
    {
        return username;
    }

    public bool isServer()
    {
        return server;
    }

    public bool isLoggedIn()
    {
        return loggedIn;
    }
}