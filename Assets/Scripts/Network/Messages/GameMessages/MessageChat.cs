using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//Send/Receive Format:
//Chat message(string)
//private(byte) - Whether or not the message is a private message(1/0)

public class MessageChat : Message
{
    public static MessageCommand thisCommand = MessageCommand.Chat;
    public static MessageEvent<MessageChat> messageEvent = new MessageEvent<MessageChat>();

    private string message;
    private bool isPrivate;

    //New message Constructor
    public MessageChat(string message, bool isPrivate)
        : base(thisCommand, null)
    {
        this.message = message;
        this.isPrivate = isPrivate;
    }

    //Parse stream
    public MessageChat(DataStream stream)
        : base(thisCommand, stream)
    {
        message = stream.readStringUTF8();
        isPrivate = stream.readByte() == 1 ? true : false;
    }

    public string getMessage()
    {
        return message;
    }

    //Register Message
    public static void registerMessage()
    {
        Message.messageParsers.Add(thisCommand, parseMessage);
        NetManager.messageEvents[thisCommand] = messageEvent;
    }

    public static Message parseMessage(DataStream data)
    {
        Message parsedMessage = new MessageChat(data);
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

        writeString(byteList, message);
        byteList.Add(BitConverter.GetBytes(isPrivate));

        return listToArray(byteList);
    }
}