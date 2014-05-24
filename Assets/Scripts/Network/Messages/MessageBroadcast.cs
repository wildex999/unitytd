using System;
using System.Collections.Generic;

//Send/Receive format:
//0(int) - Player id(Replaced by main server, this is just to reserve the space so the main server doesn't have to reconstruct the message)
//Include sender(byte 1/0) - Whether or not to also send broadcast to sender
//Message(byte[])


public class MessageBroadcast : Message, IMessageInMessage
{
    public static MessageCommand thisCommand = MessageCommand.Broadcast;
    public static MessageEvent<MessageBroadcast> messageEvent = new MessageEvent<MessageBroadcast>();

    private Message message;
    private bool includeSender; 

    //New message Constructor
    public MessageBroadcast(Message message, bool includeSender)
        : base(thisCommand, null)
    {
        this.message = message;
        this.includeSender = includeSender;
    }

    //Parse stream
    public MessageBroadcast(DataStream stream)
        : base(thisCommand, stream)
    {
        //Get the id of the player who sent the message
        senderPlayerId = stream.readInt();
        includeSender = stream.readByte() == 1 ? true : false;

        //Read the message bytes and parse it
        byte[] data = stream.readBytes();
        ParseResult result = Message.parseMessageData(new DataStream(data), out message);
        if(result != ParseResult.Done || message == null)
            throw new Exception("Failed to parse broadcast message(Res: " + result + "). Might be a network error!");

        message.senderPlayerId = senderPlayerId;
    }

    //Register Message
    public static void registerMessage()
    {
        Message.messageParsers.Add(thisCommand, parseMessage);
        NetManager.messageEvents[thisCommand] = messageEvent;
    }

    public static Message parseMessage(DataStream data)
    {
        Message parsedMessage = new MessageBroadcast(data);
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
        byteList.Add(BitConverter.GetBytes((int)0)); //Player id placeholder
        byteList.Add(BitConverter.GetBytes(includeSender));
        byte[] messageBytes = message.getBytes();
        writeBytes(byteList, messageBytes);

        return listToArray(byteList);
    }

    public Message getMessage()
    {
        return message;
    }
}