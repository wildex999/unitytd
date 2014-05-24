

/*
 * Handles the building and parsing of messages.
 * 
 * */

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public enum MessageCommand
{
    MultiCommand = 0, //Message containing multiple commands

    //Main server(MUST be numbered like in the main server code!!!)
    Login = 1, //Login request(Variables: string name, string password)
    LoginReply = 1, //Reply to login command(Variables: int id, string error)
    GetGame = 4, //Request game info for a given GameId
    GetGameReply = 4, //Reply to GetGame request, containing Game info
    JoinGame = 5, //Requets to join a game
    JoinGameReply = 5, //Reply to join, with info about game owner, map and players
    CreateGame = 6, //Request to create a game
    CreateGameReply = 6,
    MapDownload = 11, //Map data sent from either authorative player for 0-id maps, or from main server for published maps.
    PlayerJoined = 12, //New player joined the current game
    PlayerLeft = 13, //Player left the game
    Broadcast = 14, //Send message to every other player in a game
    SendMessage = 15, //Send message to specific player in a game
    Disconnect = 18, //Reason for disconnect, sent as socket disconnecting.
    Log = 21, //Logging to server side file for debugging


    //Game server(As long as everyone runs same version, numbers should be the same)
    GameTick, //Broadcast by the authorative player, telling clients to move forward to this tick
    Chat, //Chat message
}

//Who is allowed to send a message(Used to check senderPlayerId)
public enum MessagePermission
{
    MainServer, //Only Main server can send this message
    Player, //Only players can send this message
    Both //Both Players and main server can send this message
}

public enum ParseResult
{
    Done,
    NeedMoreData,
    Corrupt,
    NoParser
}

public abstract class Message
{
    protected DataStream data;
    protected MessageCommand command;
    public Socket socket;
    public int senderPlayerId = -1; //Id of player who sent the message, -1 if from main server(Not a player)(Set by MessageBroadcast and MessageSendMessage)

    //List of parsers
    public delegate Message messageParser(DataStream data);
    public static Dictionary<MessageCommand, messageParser> messageParsers = new Dictionary<MessageCommand, messageParser>();

    public const int messageLengthSize = 2; //How many bytes used to describe the message length(prefix)

    static Message()
    {
        //Register parsers
        MessageLoginReply.registerMessage();
        MessageGetGameReply.registerMessage();
        MessageCreateGameReply.registerMessage();
        MessageJoinGameReply.registerMessage();
        MessagePlayerJoined.registerMessage();
        MessagePlayerLeft.registerMessage();
        MessageBroadcast.registerMessage();
        MessageSendMessage.registerMessage();
        MessageDisconnect.registerMessage();
        MessageMapDownload.registerMessage();


        MessageChat.registerMessage();

    }
    
    public Message(MessageCommand command, DataStream stream)
    {
        this.command = command;
        data = stream;
    }

    public MessageCommand getCommand()
    {
        return command;
    }

    public virtual void handle()
    {
        //Handle the message
        //Override this for each command
    }

    public virtual MessagePermission getPermission()
    {
        return MessagePermission.MainServer;
    }

    //Return the size of the message(Internal byte[] size)
    public virtual int getSize()
    {
        return data.getSize();
    }

    //Get bytes for sending message
    //Does NOT include the message size(First two bytes should be the command)
    public abstract byte[] getBytes();

    //Parse data to get message
    public static ParseResult parseMessage(byte[] data, out Message message, out int messageLengthOut)
    {
        message = null;
        messageLengthOut = 0;

        ushort messageLength;
        
        //Read the first two bytes to get length
        if (data.Length < messageLengthSize)
            return ParseResult.NeedMoreData;
        messageLength = BitConverter.ToUInt16(data, 0);

        if (data.Length < messageLength + messageLengthSize)
            return ParseResult.NeedMoreData;

        messageLengthOut = messageLength;

        //Parse message
        byte[] messageData = new byte[messageLength];
        Array.Copy(data, messageLengthSize, messageData, 0, messageLength);
        DataStream stream = new DataStream(messageData);

        return parseMessageData(stream, out message);
    }

    //Parse complete message with known length from stream
    protected static ParseResult parseMessageData(DataStream stream, out Message message)
    {
        message = null;
        MessageCommand command = (MessageCommand)stream.readShort();

        Debug.Log("Got message with command: " + command);

        messageParser parser;
        messageParsers.TryGetValue(command, out parser);
        if (parser != null)
            message = messageParsers[command](stream);
        else
        {
            Debug.LogError("No parser for command: " + command);
            return ParseResult.NoParser;
        }

        return ParseResult.Done;
    }

    //Write string to List
    protected static void writeString(List<byte[]> list, string str)
    {
        byte[] strBytes = Encoding.UTF8.GetBytes(str);
        list.Add(BitConverter.GetBytes((ushort)strBytes.Length));
        list.Add(strBytes);
    }

    //Write byte array to list
    protected static void writeBytes(List<byte[]> list, byte[] bytes)
    {
        //Write length of array in the firts 4 bytes(int), and then the bytes themself
        list.Add(BitConverter.GetBytes((int)bytes.Length));
        list.Add(bytes);
    }

    //Merge multiple byte arrays into a single array for sending
    protected static byte[] listToArray(List<byte[]> list)
    {
        uint finalSize = 0;
        foreach(byte[] data in list)
            finalSize += (uint)data.Length;

        if (finalSize > ushort.MaxValue)
            throw new Exception("Message size too large!");

        byte[] newData = new byte[finalSize];


        int offset = 0;
        foreach(byte[] data in list)
        {
            Array.Copy(data, 0, newData, offset, data.Length);
            offset += data.Length;
        }

        return newData;

    }
}

