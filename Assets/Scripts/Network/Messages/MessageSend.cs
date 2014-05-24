using System;
using System.Collections.Generic;

//Send format:
//0(int) - Sender Player id(Replaced by main server, this is just to reserve the space so the main server doesn't have to reconstruct the message)
//PlayerId(int) - Receiver player id
//Message(byte[])

//Receive Format:
//Sender Player id(int) - This is set by the main server as it is broadcast
//Receiver player id(int) - Redundant realy, but leave it in to avoid reconstructing the message when passing through main server
//Message(byte[]) - Message that was broadcast, serialized

//Send message to player in current game
public class MessageSendMessage : Message, IMessageInMessage
{
    public static MessageCommand thisCommand = MessageCommand.SendMessage;
    public static MessageEvent<MessageSendMessage> messageEvent = new MessageEvent<MessageSendMessage>();

    private Message message;
    private int recipientPlayerId; //Id of player who receives the message

    //New message Constructor
    public MessageSendMessage(Player recipient, Message message)
        : base(thisCommand, null)
    {
        this.message = message;
        recipientPlayerId = recipient.getId();
    }

    //Parse stream
    public MessageSendMessage(DataStream stream)
        : base(thisCommand, stream)
    {
        //Get the id of the player who sent the message, and recipient(Us)
        senderPlayerId = stream.readInt();
        recipientPlayerId = stream.readInt();

        //Read the message bytes and parse it
        byte[] data = stream.readBytes();
        ParseResult result = Message.parseMessageData(new DataStream(data), out message);
        if (result != ParseResult.Done || message == null)
            throw new Exception("Failed to parse sent message(Res: " + result + "). Might be a network error!");

        message.senderPlayerId = senderPlayerId;
    }

    public Message getMessage()
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
        Message parsedMessage = new MessageSendMessage(data);
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
        byteList.Add(BitConverter.GetBytes((int)0)); //Sender Player id placeholder
        byteList.Add(BitConverter.GetBytes(recipientPlayerId)); //Recipient player id
        byte[] messageBytes = message.getBytes();
        writeBytes(byteList, messageBytes);

        return listToArray(byteList);
    }

}