using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//Send/Receive Format:
//Map data(bytes)

public class MessageMapDownload : Message
{
    public static MessageCommand thisCommand = MessageCommand.MapDownload;
    public static MessageEvent<MessageMapDownload> messageEvent = new MessageEvent<MessageMapDownload>();

    private byte[] data;

    //New message Constructor
    public MessageMapDownload(byte[] mapData)
        : base(thisCommand, null)
    {
        data = mapData;
    }

    //Parse stream
    public MessageMapDownload(DataStream stream)
        : base(thisCommand, stream)
    {
        data = stream.readBytes();
        Debug.Log("Got map download of size: " + data.Length);
    }

    public byte[] getMapData()
    {
        return data;
    }

    //Register Message
    public static void registerMessage()
    {
        Message.messageParsers.Add(thisCommand, parseMessage);
        NetManager.messageEvents[thisCommand] = messageEvent;
    }

    public static Message parseMessage(DataStream data)
    {
        Message parsedMessage = new MessageMapDownload(data);
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
        writeBytes(byteList, data);

        return listToArray(byteList);
    }
}