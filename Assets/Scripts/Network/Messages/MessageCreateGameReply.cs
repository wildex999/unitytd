using System.Collections.Generic;
using UnityEngine;

public class MessageCreateGameReply : Message
{
    public static MessageCommand thisCommand = MessageCommand.CreateGameReply;
    public static MessageEvent<MessageCreateGameReply> messageEvent = new MessageEvent<MessageCreateGameReply>();

    bool gameCreated;
    int gameId;
    string errorMsg;

    //Parse stream
    public MessageCreateGameReply(DataStream stream)
        : base(thisCommand, stream)
    {
        gameCreated = true;
        if (stream.readByte() == 0)
        {
            gameCreated = false;

            errorMsg = stream.readStringUTF8();

            Debug.Log("Failed to create game with error: " + errorMsg);

            return;
        }

        gameId = stream.readInt();
        Debug.Log("Created game successfully with gameId: " + gameId);
    }

    //Register the message
    public static void registerMessage()
    {
        Message.messageParsers.Add(thisCommand, parseMessage);
        NetManager.messageEvents[thisCommand] = messageEvent;
    }

    public static Message parseMessage(DataStream data)
    {
        Message parsedMessage = new MessageCreateGameReply(data);
        return parsedMessage;
    }

    public override byte[] getBytes()
    {
        return null;
    }

    public bool gameIsCreated()
    {
        return gameCreated;
    }

    public int getGameId()
    {
        return gameId;
    }

    public string getErrorMessage()
    {
        return errorMsg;
    }


}