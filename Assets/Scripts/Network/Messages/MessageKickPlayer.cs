using System;
using System.Collections.Generic;

//Kick can happen either by the host player, or by the main server.

//Send format:
//Id of Player to kick(int)
//reason for kick(string)

//Receive format:
//reason for kick(string)


public class MessageKickPlayer : Message
{
    public static MessageCommand thisCommand = MessageCommand.KickPlayer;
    public static MessageEvent<MessageKickPlayer> messageEvent = new MessageEvent<MessageKickPlayer>();

    private int kickPlayerId;
    private string reason;

    //New message Constructor
    public MessageKickPlayer(Player kickPlayer, string reason)
        : base(thisCommand, null)
    {
        this.kickPlayerId = kickPlayer.getId();
        this.reason = reason;
    }

    //Parse stream
    public MessageKickPlayer(DataStream stream)
        : base(thisCommand, stream)
    {
        reason = stream.readStringUTF8();
    }

    public string getReason()
    {
        return reason;
    }

    //Register Message
    public static void registerMessage()
    {
        Message.messageParsers.Add(thisCommand, parseMessage);
        NetManager.messageEvents[thisCommand] = messageEvent;
    }

    public static Message parseMessage(DataStream data)
    {
        Message parsedMessage = new MessageKickPlayer(data);
        return parsedMessage;
    }

    public override MessagePermission getPermission()
    {
        return MessagePermission.MainServer;
    }

    public override byte[] getBytes()
    {
        List<byte[]> byteList = new List<byte[]>();

        writeString(byteList, reason);

        return listToArray(byteList);
    }
}