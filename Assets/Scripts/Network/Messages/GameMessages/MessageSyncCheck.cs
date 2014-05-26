using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//Sent by clients to server to check the sync status of the game

//Send/Receive Format:
//tick(uint)
//hash(int)

public class MessageSyncCheck : Message
{
    public static MessageCommand thisCommand = MessageCommand.SyncCheck;
    public static MessageEvent<MessageSyncCheck> messageEvent = new MessageEvent<MessageSyncCheck>();

    private int hash;
    private uint tick;

    //New message Constructor
    public MessageSyncCheck(uint tick, int hash)
        : base(thisCommand, null)
    {
        this.tick = tick;
        this.hash = hash;
    }

    //Parse stream
    public MessageSyncCheck(DataStream stream)
        : base(thisCommand, stream)
    {
        tick = stream.readUInt();
        hash = stream.readInt();
    }

    public int getHash()
    {
        return hash;
    }

    public uint getTick()
    {
        return tick;
    }

    //Register Message
    public static void registerMessage()
    {
        Message.messageParsers.Add(thisCommand, parseMessage);
        NetManager.messageEvents[thisCommand] = messageEvent;
    }

    public static Message parseMessage(DataStream data)
    {
        Message parsedMessage = new MessageSyncCheck(data);
        return parsedMessage;
    }

    public override MessagePermission getPermission()
    {
        return MessagePermission.Both;
    }

    public override byte[] getBytes()
    {
        List<byte[]> byteList = new List<byte[]>();

        byteList.Add(BitConverter.GetBytes((ushort)command));

        byteList.Add(BitConverter.GetBytes(tick));
        byteList.Add(BitConverter.GetBytes(hash));

        return listToArray(byteList);
    }
}