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
    LoggedIn
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

public class NetManager : MonoBehaviour
{
    Socket mainSocket;

    public MainServerState mainState = MainServerState.NotConnected;
    public string mainServerError = ""; //Error message for when failing to connect to main server
    public string gameServerError = ""; //Error message when failing to join game

    private string username = "";
    private bool server = false;
    private bool loggedIn = false;

    //Thread locks/messaging
    private ManualResetEvent mainConnectEvent = new ManualResetEvent(false);
    private ManualResetEvent sendMessageLock = new ManualResetEvent(true); //When true, send next message
    private ConcurrentQueue<Message> messageReceivenQueue = new ConcurrentQueue<Message>(); //Messages from server
    private ConcurrentQueue<Message> messageSendQueue = new ConcurrentQueue<Message>(); //Messages to server
    //TODO: Network error queue(I.e failed sends)(Failed send = close socket and disconnect from game/server)

    //Return an instance of netmanager. Create it if it doesn't exist(create=true)
    public static NetManager getNetManager(bool create = true)
    {
        NetManager manager = GameObject.FindObjectOfType<NetManager>();
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

        //Connect to main server
        connectToMainServer("46.9.166.72", 12000);
    }

    void Update()
    {
        if(mainState == MainServerState.Connecting)
        {
            //Check if we have connected
            if(mainConnectEvent.WaitOne(0))
                onMainServerConnected();
        }

        //If done sending previous message, start sending next one
        handleSendingMessage();

        //Check for new messages
    }

    public bool connectToMainServer(string ip, int port)
    {
        if (mainSocket != null)
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

        mainState = MainServerState.Connected;
        Debug.Log("Connected to server: " + mainSocket.RemoteEndPoint.ToString());
    }

    //Try to send message from sending queue
    private void handleSendingMessage()
    {
        if(sendMessageLock.WaitOne(0))
        {
            Message message = messageSendQueue.pop();
            if (message != null)
            {
                byte[] messageData = message.getBytes();

                //Add message length to the front
                byte[] messageLength = BitConverter.GetBytes((ushort)(messageData.Length));

                byte[] sendData = new byte[messageData.Length + 2];
                Array.Copy(messageLength, 0, sendData, 0, 2);
                Array.Copy(messageData, 0, sendData, 2, messageData.Length);

                sendMessageLock.Reset();

                MessageSendData customData = new MessageSendData(message.socket, sendData);

                Debug.Log("Sending " + sendData.Length + " bytes...");
                message.socket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, new AsyncCallback(callbackSendMessage), customData);
            }
        }
    }

    //Manage state changes with main server
    public void setMainState(MainServerState newState)
    {
        //TODO: Make sure we maintain stability with state changes.
        //For example, state change from connected to any non-connected state should always run a socket.disconnect
    }

    //Manage state changes with game server connection
    public void setGameState(GameServerState newState)
    {

    }

    public bool sendMessage(Socket target, Message message)
    {
        if (!target.Connected)
            throw new Exception("Tried to send message to disconnected socket");

        message.socket = target;
        messageSendQueue.push(message);
        Debug.Log("Queued message for sending");
        handleSendingMessage(); //Try to send it now

        return true;
    }

    public void callbackSendMessage(IAsyncResult ar)
    {
        MessageSendData data = (MessageSendData)ar.AsyncState;
        Socket client = data.socket;

        int bytesSent = client.EndSend(ar);

        int remainingLength = data.data.Length - bytesSent;
        if (remainingLength > 0)
        {
            //Re-send remaining data
            byte[] remaining = new byte[remainingLength];
            Array.Copy(data.data, bytesSent, remaining, 0, remainingLength);
            data.data = remaining;
            Debug.Log("Continue to send remaining: " + remainingLength);
            client.BeginSend(remaining, 0, remaining.Length, SocketFlags.None, new AsyncCallback(callbackSendMessage), data);
        }
        else
        {
            //We are done sending this message
            Debug.Log("Full message sent");
            sendMessageLock.Set();
        }
    }

    //Login with the given name
    public bool login(string name)
    {
        if (mainState != MainServerState.Connected)
            return false;

        Message loginMessage = new MessageLogin(name, "");
        if(!sendMessage(mainSocket, loginMessage))
            return false;
        mainState = MainServerState.LoggingIn;

        return true;
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