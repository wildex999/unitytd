using System;
using System.Collections.Generic;

//Send/Recieve format:
//Disconnect reason(string)

public class MessageDisconnect : Message
{
    public static MessageCommand thisCommand = MessageCommand.Disconnect;
    public static MessageEvent<MessageDisconnect> messageEvent = new MessageEvent<MessageDisconnect>();

    private string reason;

    //New message Constructor
    public MessageDisconnect(string reason)
        : base(thisCommand, null)
    {
        this.reason = reason;
    }

    //Parse stream
    public MessageDisconnect(DataStream stream)
        : base(thisCommand, stream)
    {
        reason = stream.readStringUTF8();
    }

    //Register Message
    public static void registerMessage()
    {
        Message.messageParsers.Add(thisCommand, parseMessage);
        NetManager.messageEvents[thisCommand] = messageEvent;
    }

    public static Message parseMessage(DataStream data)
    {
        Message parsedMessage = new MessageDisconnect(data);
        return parsedMessage;
    }

    public override byte[] getBytes()
    {
        List<byte[]> byteList = new List<byte[]>();

        writeString(byteList, reason);

        return listToArray(byteList);
    }
}