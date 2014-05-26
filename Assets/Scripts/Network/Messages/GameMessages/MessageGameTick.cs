using System;
using System.Collections.Generic;

//Format:
//current tick(uint)

public class MessageGameTick : Message
{
    public static MessageCommand thisCommand = MessageCommand.GameTick;
    public static MessageEvent<MessageGameTick> messageEvent = new MessageEvent<MessageGameTick>();

    private uint currentTick;

    //New message
    public MessageGameTick(uint tick)
        : base(thisCommand, null)
    {
        currentTick = tick;
    }

    //Parse stream
    public MessageGameTick(DataStream stream)
        : base(thisCommand, stream)
    {
        currentTick = stream.readUInt();
    }

    public uint getTick()
    {
        return currentTick;
    }

    //Register Message
    public static void registerMessage()
    {
        Message.messageParsers.Add(thisCommand, parseMessage);
        NetManager.messageEvents[thisCommand] = messageEvent;
    }

    public static Message parseMessage(DataStream data)
    {
        Message parsedMessage = new MessageGameTick(data);
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

        byteList.Add(BitConverter.GetBytes(currentTick));

        return listToArray(byteList);
    }
}