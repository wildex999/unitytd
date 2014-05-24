using System;
using System.Collections.Generic;

public class MessageGameTick : Message
{


    //New message Constructor
    public MessageGameTick(string type, string message)
        : base(MessageCommand.GameTick, null)
    {

    }

    public override byte[] getBytes()
    {
        List<byte[]> byteList = new List<byte[]>();

        byteList.Add(BitConverter.GetBytes((ushort)command));
        //writeString(byteList, logType);
        //writeString(byteList, logMessage);

        return listToArray(byteList);
    }
}