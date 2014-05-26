using System;
using System.Collections.Generic;

//Set the ready state of player

//Send/Receive format:
//new state(byte. Ready(1)/Not ready(0))


public class MessageSetReady : Message
{
    public static MessageCommand thisCommand = MessageCommand.SetReady;
    public static MessageEvent<MessageSetReady> messageEvent = new MessageEvent<MessageSetReady>();

    private bool readyState;

    //New message Constructor
    public MessageSetReady(bool setReady)
        : base(thisCommand, null)
    {
        readyState = setReady;
    }

    //Parse stream
    public MessageSetReady(DataStream stream)
        : base(thisCommand, stream)
    {
        readyState = stream.readByte() == 1 ? true : false;
    }

    public bool getReadyState()
    {
        return readyState;
    }

    //Register Message
    public static void registerMessage()
    {
        Message.messageParsers.Add(thisCommand, parseMessage);
        NetManager.messageEvents[thisCommand] = messageEvent;
    }

    public static Message parseMessage(DataStream data)
    {
        Message parsedMessage = new MessageSetReady(data);
        return parsedMessage;
    }

    public override MessagePermission getPermission()
    {
        return MessagePermission.Player;
    }

    public override byte[] getBytes()
    {
        List<byte[]> byteList = new List<byte[]>();

        byteList.Add(BitConverter.GetBytes((ushort)command));

        byteList.Add(BitConverter.GetBytes(readyState));

        return listToArray(byteList);
    }
}