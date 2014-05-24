//Read complete file using a Async delegate.

using System;
using System.IO;
public class AsyncFileReader
{
    public enum ReadStatus
    {
        Incomplete, //Still reading
        Complete, //Reading complete
        Error //Ended with error
    }

    private ReadStatus readStatus;
    private Exception error;
    private byte[] data;

    private delegate byte[] AsyncMethodCaller(string filename);
    private AsyncMethodCaller caller;
    private IAsyncResult result;

    //Start reading the given file
    public AsyncFileReader(string filename)
    {
        readStatus = ReadStatus.Error;

        caller = new AsyncMethodCaller(asyncReadFile);
        readStatus = ReadStatus.Incomplete;
        result = caller.BeginInvoke(filename, null, null);
    }

    public ReadStatus getReadStatus()
    {
        lock(this)
        {
            return readStatus;
        }
    }

    private void setReadStatus(ReadStatus newStatus)
    {
        lock(this)
        {
            readStatus = newStatus;
        }
    }

    public Exception getError()
    {
        return error;
    }

    //Returns the data of the read.
    //Warning: If called while getReadStatus returns Incomplete, this will block until the data is ready!
    //Returns null on error.
    public byte[] getData()
    {
        if (caller == null)
            return null;

        if (data == null)
            data = caller.EndInvoke(result); //Will block if not done

        return data;
    }

    private byte[] asyncReadFile(string filename)
    {
        byte[] fileData = null;

        setReadStatus(ReadStatus.Incomplete);

        try
        {
            fileData = File.ReadAllBytes(filename);
            setReadStatus(ReadStatus.Complete);
        }
        catch(Exception ex)
        {
            error = ex;
            setReadStatus(ReadStatus.Error);
        }

        return fileData;
    }


}