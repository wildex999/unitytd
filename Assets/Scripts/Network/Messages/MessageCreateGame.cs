using System;
using System.Collections.Generic;

//CreateGame format:
//gameId(string)
//gameName(string)
//gameVersion(int)
//maxPlayers(int)
//gameMode(string)
//mapId(int)

public class MessageCreateGame : Message
{
    public string gameName;
    public int maxPlayers;
    public string gameMode;
    public int mapId;
    public string mapName; //Empty if mapId != 0

    //New message Constructor
    public MessageCreateGame(string gameName, int maxPlayers, string gameMode, int mapId, string mapName = "")
        : base(MessageCommand.CreateGame, null)
    {
        this.gameName = gameName;
        this.maxPlayers = maxPlayers;
        this.gameMode = gameMode;
        this.mapId = mapId;
        this.mapName = mapName;
    }

    public override byte[] getBytes()
    {
        List<byte[]> byteList = new List<byte[]>();

        byteList.Add(BitConverter.GetBytes((ushort)command));
        writeString(byteList, gameName);
        byteList.Add(BitConverter.GetBytes(GameVersion.getBuild()));
        byteList.Add(BitConverter.GetBytes(maxPlayers));
        writeString(byteList, gameMode);
        byteList.Add(BitConverter.GetBytes(mapId));
        writeString(byteList, mapName);

        return listToArray(byteList);
    }
}