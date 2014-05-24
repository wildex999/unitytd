using System.Collections.Generic;
using System.Threading;
using UnityEngine;

//Almost like GetGameReply, but also has an error in case it fails.

public class MessageJoinGameReply : Message
{
    public static MessageCommand thisCommand = MessageCommand.JoinGameReply;
    public static MessageEvent<MessageJoinGameReply> messageEvent = new MessageEvent<MessageJoinGameReply>();

    bool gameJoined;
    string error;

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
    public MessageJoinGameReply(DataStream stream)
        : base(thisCommand, stream)
    {
        gameJoined = true;
        if (stream.readByte() == 0)
        {
            gameJoined = false;
            error = stream.readStringUTF8();
            return;
        }

        gameName = stream.readStringUTF8();
        gameVersion = stream.readInt();

        int ownerId = stream.readInt();

        maxPlayers = stream.readInt();
        int playerCount = stream.readInt();
        for (int player = 0; player < playerCount; player++)
        {
            string playerName = stream.readStringUTF8();
            int playerId = stream.readInt();
            players.Add(new Player(playerName, playerId));

            if (playerId == ownerId)
                owner = players[players.Count - 1];
        }

        gameMode = stream.readStringUTF8();
        mapName = stream.readStringUTF8();
        mapId = stream.readInt();
        mapVersion = stream.readInt();
    }

    //Register the message
    public static void registerMessage()
    {
        Message.messageParsers.Add(thisCommand, parseMessage);
        NetManager.messageEvents[thisCommand] = messageEvent;
    }

    public static Message parseMessage(DataStream data)
    {
        Message parsedMessage = new MessageJoinGameReply(data);
        return parsedMessage;
    }

    public override byte[] getBytes()
    {
        return null;
    }

    public bool joinedGame()
    {
        return gameJoined;
    }

    public string getErrorMessage()
    {
        return error;
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

    public int getMapId()
    {
        return mapId;
    }


}