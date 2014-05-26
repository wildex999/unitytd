using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//Send/Receive Format:
//Action(ushort)
//Action data(byte[])

public class MessageAction : Message
{
    public static MessageCommand thisCommand = MessageCommand.Action;
    public static MessageEvent<MessageAction> messageEvent = new MessageEvent<MessageAction>();

    private byte[] actionData;
    private Actions actionType;

    private Action action;

    //New message Constructor
    public MessageAction(Action action)
        : base(thisCommand, null)
    {
        this.action = action;
        this.actionType = action.action;
    }

    //Parse stream
    public MessageAction(DataStream stream)
        : base(thisCommand, stream)
    {
        Debug.Log("Parse MessageAction");

        //Read byte stream
        actionType = (Actions)stream.readUShort();
        actionData = stream.readBytes();
    }

    public byte[] getActionData()
    {
        return actionData;
    }

    public Actions getActionType()
    {
        return actionType;
    }

    //Register Message
    public static void registerMessage()
    {
        Message.messageParsers.Add(thisCommand, parseMessage);
        NetManager.messageEvents[thisCommand] = messageEvent;
    }

    public static Message parseMessage(DataStream data)
    {
        Message parsedMessage = new MessageAction(data);
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

        byteList.Add(BitConverter.GetBytes((ushort)actionType));
        writeBytes(byteList, action.getBytes());

        return listToArray(byteList);
    }
}