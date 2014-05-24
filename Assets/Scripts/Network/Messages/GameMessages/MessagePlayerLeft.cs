using UnityEngine;

//PlayerLeft includes player id
public class MessagePlayerLeft : Message
{
    public static MessageCommand thisCommand = MessageCommand.PlayerLeft;
    public static MessageEvent<MessagePlayerLeft> messageEvent = new MessageEvent<MessagePlayerLeft>();

    private int playerId;

    //Parse stream
    public MessagePlayerLeft(DataStream stream)
        : base(thisCommand, stream)
    {
        playerId = stream.readInt();
    }

    //Register the message
    public static void registerMessage()
    {
        Message.messageParsers.Add(thisCommand, parseMessage);
        NetManager.messageEvents[thisCommand] = messageEvent;
    }

    public static Message parseMessage(DataStream data)
    {
        Message parsedMessage = new MessagePlayerLeft(data);
        return parsedMessage;
    }

    public int getPlayerId()
    {
        return playerId;
    }

    public override byte[] getBytes()
    {
        return null;
    }

}