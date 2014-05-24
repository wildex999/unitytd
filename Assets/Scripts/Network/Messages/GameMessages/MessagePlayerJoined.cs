using UnityEngine;

//PlayerJoined includes player name and player id
public class MessagePlayerJoined : Message
{
    public static MessageCommand thisCommand = MessageCommand.PlayerJoined;
    public static MessageEvent<MessagePlayerJoined> messageEvent = new MessageEvent<MessagePlayerJoined>();

    private string playerName;
    private int playerId;

    //Parse stream
    public MessagePlayerJoined(DataStream stream)
        : base(thisCommand, stream)
    {
        playerName = stream.readStringUTF8();
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
        Message parsedMessage = new MessagePlayerJoined(data);
        return parsedMessage;
    }

    public string getPlayerName()
    {
        return playerName;
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