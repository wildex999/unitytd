using System;
using System.Collections.Generic;
public class MessageLogin : Message
{
    public string username;
    public string password;


    //New message Constructor
    public MessageLogin(string username, string password)
        : base(MessageCommand.Login, null)
    {
        this.username = username;
        this.password = password;
    }

    public override byte[] getBytes()
    {
        List<byte[]> byteList = new List<byte[]>();

        byteList.Add(BitConverter.GetBytes((ushort)command));
        writeString(byteList, username);
        writeString(byteList, password);

        return listToArray(byteList);
    }
}