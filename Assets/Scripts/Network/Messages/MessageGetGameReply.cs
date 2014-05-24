using System.Collections.Generic;
using UnityEngine;

public class MessageGetGameReply : Message
{
    public static MessageCommand thisCommand = MessageCommand.GetGameReply;
    public static MessageEvent<MessageGetGameReply> messageEvent = new MessageEvent<MessageGetGameReply>();

    bool gameFound;
    string gameName;
    int gameVersion;
    Player owner;
    int maxPlayers;
    List<Player> players = new List<Player>();
    string gameMode;
    string mapName; //TODO: make MapInfo class
    int mapId;
    int mapVersion;

    //Parse stream
    public MessageGetGameReply(DataStream stream)
        : base(thisCommand, stream)
    {
        gameFound = true;
        if (stream.readByte() == 0)
        {
            gameFound = false;
            return;
        }

        gameName = stream.readStringUTF8();
        Debug.Log("Game name: " + gameName);
        gameVersion = stream.readInt();
        Debug.Log("Game version: " + gameVersion);

        int ownerId = stream.readInt();
        
        maxPlayers = stream.readInt();
        Debug.Log("Max players: " + maxPlayers);
        int playerCount = stream.readInt();
        Debug.Log("Player count: " + playerCount);
        for(int player = 0; player < playerCount; player++)
        {
            string playerName = stream.readStringUTF8();
            Debug.Log("Player(" + player + ") name: " + playerName);
            int playerId = stream.readInt();
            Debug.Log("Player(" + player + ") id: " + playerId);
            players.Add(new Player(playerName, playerId));

            if(playerId == ownerId)
            {
                Debug.Log("Is Owner: " + playerName);
                owner = players[players.Count - 1];
            }
        }

        gameMode = stream.readStringUTF8();
        Debug.Log("Game mode: " + gameMode);
        mapName = stream.readStringUTF8();
        Debug.Log("Map name: " + mapName);
        mapId = stream.readInt();
        Debug.Log("Map id: " + mapId);
        mapVersion = stream.readInt();
    }

    //Register Message
    public static void registerMessage()
    {
        Message.messageParsers.Add(thisCommand, parseMessage);
        NetManager.messageEvents[thisCommand] = messageEvent;
    }

    public static Message parseMessage(DataStream data)
    {
        Message parsedMessage = new MessageGetGameReply(data);
        return parsedMessage;
    }

    public override byte[] getBytes()
    {
        return null;
    }

    public bool gotGame()
    {
        return gameFound;
    }

    public string getGameName()
    {
        return gameName;
    }

    public int getGameVersion()
    {
        return gameVersion;
    }

    public Player getOwner()
    {
        return owner;
    }

    public int getMaxPlayers()
    {
        return maxPlayers;
    }

    public List<Player> getPlayers()
    {
        return players;
    }

    public string getGameMode()
    {
        return gameMode;
    }

    public string getMapName()
    {
        return mapName;
    }


}