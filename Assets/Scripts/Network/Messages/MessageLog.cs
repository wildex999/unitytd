using System;
using System.Collections.Generic;

public class MessageLog : Message
{
    string logType;
    string logMessage;

    //New message Constructor
    public MessageLog(string type, string message)
        : base(MessageCommand.Log, null)
    {
        logType = type;
        logMessage = message;
    }

    public override byte[] getBytes()
    {
        List<byte[]> byteList = new List<byte[]>();

        byteList.Add(BitConverter.GetBytes((ushort)command));
        writeString(byteList, logType);
        writeString(byteList, logMessage);

        return listToArray(byteList);
    }
}