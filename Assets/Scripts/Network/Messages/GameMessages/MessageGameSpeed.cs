using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//Set the game speed

//Send/Receive Format:
//game speed(FInt)

public class MessageGameSpeed : Message
{
    public static MessageCommand thisCommand = MessageCommand.GameSpeed;
    public static MessageEvent<MessageGameSpeed> messageEvent = new MessageEvent<MessageGameSpeed>();

    private FInt gameSpeed;

    //New message Constructor
    public MessageGameSpeed(FInt gameSpeed)
        : base(thisCommand, null)
    {
        this.gameSpeed = gameSpeed;
    }

    //Parse stream
    public MessageGameSpeed(DataStream stream)
        : base(thisCommand, stream)
    {
    }

    //Register Message
    public static void registerMessage()
    {
        Message.messageParsers.Add(thisCommand, parseMessage);
        NetManager.messageEvents[thisCommand] = messageEvent;
    }

    public static Message parseMessage(DataStream data)
    {
        Message parsedMessage = new MessageGameSpeed(data);
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


        return listToArray(byteList);
    }
}