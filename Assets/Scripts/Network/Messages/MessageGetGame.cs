using System;
using System.Collections.Generic;

//GetGame format:
//gameId(string)

public class MessageGetGame : Message
{
    public string gameId;

    //New message Constructor
    public MessageGetGame(string gameId)
        : base(MessageCommand.GetGame, null)
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