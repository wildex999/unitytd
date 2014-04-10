

/*
 * Handles the building and parsing of messages.
 * 
 * */

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
public enum MessageCommand
{
    MultiCommand = 0, //Message containing multiple commands

    //Main server(MUST be numbered like in the main server code!!!)
    Login = 1, //Login request(Variables: string name, string password)
    LoginReply = 1, //Reply to login command(Variables: int id, string error)

    //Game server(As long as everyone runs same version, numbers should be the same)
}

public enum ParseResult
{
    Done,
    NeedMoreData,
    Corrupt
}

public abstract class Message
{
    protected DataStream data;
    protected MessageCommand command;
    public Socket socket;

    public Message(MessageCommand command, DataStream stream)
    {
        this.command = command;
        data = stream;
    }

    public MessageCommand getCommand()
    {
        return command;
    }

    public virtual void handle()
    {
        //Handle the message
        //Override this for each command
    }

    //Get bytes for sending message
    //Does NOT include the message size(First two bytes should be the command)
    public abstract byte[] getBytes();

    //Parse data to get message
    public static ParseResult parseMessage(byte[] data, out Message message)
    {
        ushort messageLength;

        message = null;
        //Read the first two bytes to get length
        if (data.Length < 2)
            return ParseResult.NeedMoreData;
        messageLength = BitConverter.ToUInt16(data, 0);

        if (data.Length < messageLength)
            return ParseResult.NeedMoreData;

        //Parse message
        byte[] messageData = new byte[messageLength];
        Array.Copy(data, 2, messageData, 0, messageLength-2);
        DataStream stream = new DataStream(messageData);

        MessageCommand command = (MessageCommand)stream.readShort();

        Message newMessage;
        switch(command)
        {
            case MessageCommand.LoginReply:
                newMessage = new MessageLoginReply(stream);
                break;
        }

        return ParseResult.Done;
    }

    //Write string to List
    protected static void writeString(List<byte[]> list, string str)
    {
        byte[] strBytes = Encoding.UTF8.GetBytes(str);
        list.Add(BitConverter.GetBytes((ushort)strBytes.Length));
        list.Add(strBytes);
    }

    //Merge multiple byte arrays into a single array for sending
    protected static byte[] listToArray(List<byte[]> list)
    {
        uint finalSize = 0;
        foreach(byte[] data in list)
            finalSize += (uint)data.Length;

        if (finalSize > ushort.MaxValue)
            throw new Exception("Message size too large!");

        byte[] newData = new byte[finalSize];


        int offset = 0;
        foreach(byte[] data in list)
        {
            Array.Copy(data, 0, newData, offset, data.Length);
            offset += data.Length;
        }

        return newData;

    }
}

