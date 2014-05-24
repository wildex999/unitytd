using System;
using System.Collections.Generic;

//JoinGame format:
//gameId(string)

public class MessageJoinGame : Message
{
    public string gameId;

    //New message Constructor
    public MessageJoinGame(string gameId)
        : base(MessageCommand.JoinGame, null)
    {
        this.gameId = gameId;
    }

    public override byte[] getBytes()
    {
        List<byte[]> byteList = new List<byte[]>();

        byteList.Add(BitConverter.GetBytes((ushort)command));
        writeString(byteList, gameId);

        return listToArray(byteList);
    }
}