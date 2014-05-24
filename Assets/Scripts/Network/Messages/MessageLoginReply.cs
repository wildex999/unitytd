using UnityEngine;

//The return from a login request is an player id(-1 if error) and an error string(Empty if no error)
public class MessageLoginReply : Message
{
    public static MessageCommand thisCommand = MessageCommand.LoginReply;
    public static MessageEvent<MessageLoginReply> messageEvent = new MessageEvent<MessageLoginReply>();

    private int playerId;
    private string errorMsg;

    //Parse stream
    public MessageLoginReply(DataStream stream)
        : base(thisCommand, stream)
    {
        playerId = stream.readInt();
        Debug.Log("Player Id: " + playerId);
        if(playerId == -1)
        {
            errorMsg = stream.readStringUTF8();
            Debug.Log("Login error: " + errorMsg);
        }
    }

    //Register the message
    public static void registerMessage()
    {
        Message.messageParsers.Add(thisCommand, parseMessage);
        NetManager.messageEvents[thisCommand] = messageEvent;
    }

    public static Message parseMessage(DataStream data)
    {
        Message parsedMessage = new MessageLoginReply(data);
        return parsedMessage;
    }

    public int getPlayerId()
    {
        return playerId;
    }

    public string getErrorMessage()
    {
        return errorMsg;
    }

    public bool loginOk()
    {
        if(playerId < 0)
            return false;
        return true;
    }

    public override byte[] getBytes()
    {
        return null;
    }


}