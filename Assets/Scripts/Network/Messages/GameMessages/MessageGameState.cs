using System;
using System.Collections.Generic;

//Set the current game status

//Send/Receive format:
//New state(ushort)


public class MessageGameState : Message
{
    public static MessageCommand thisCommand = MessageCommand.GameState;
    public static MessageEvent<MessageGameState> messageEvent = new MessageEvent<MessageGameState>();

    private GameState newState;

    //New message Constructor
    public MessageGameState(GameState setState)
        : base(thisCommand, null)
    {
        newState = setState;
    }

    //Parse stream
    public MessageGameState(DataStream stream)
        : base(thisCommand, stream)
    {
        newState = (GameState)stream.readUShort();
    }

    public GameState getGameState()
    {
        return newState;
    }

    //Register Message
    public static void registerMessage()
    {
        Message.messageParsers.Add(thisCommand, parseMessage);
        NetManager.messageEvents[thisCommand] = messageEvent;
    }

    public static Message parseMessage(DataStream data)
    {
        Message parsedMessage = new MessageGameState(data);
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

        byteList.Add(BitConverter.GetBytes((ushort)newState));

        return listToArray(byteList);
    }
}