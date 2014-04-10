

//The return from a login request is an player id(-1 if error) and an error string(Empty if no error)
public class MessageLoginReply : Message
{
    private uint playerId;
    private string errorMsg;

    //Parsing constructor
    public MessageLoginReply(DataStream stream)
        : base(MessageCommand.LoginReply, stream)
    {
        //Parse the data stream
        playerId = stream.readUInt();
    }

    public uint getPlayerId()
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