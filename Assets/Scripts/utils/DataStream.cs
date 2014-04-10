using System;
using System.Text;


//Class for easy reading of a bytestream

public class DataStream
{
    private byte[] data;
    private int offset; //Current offset in message data

    public DataStream(byte[] data)
    {
        this.data = data;
        offset = 0;
    }

    public int getOffset()
    {
        return offset;
    }

    public void setOffset(int newOffset)
    {
        if (newOffset >= data.Length)
            return;
        offset = newOffset;
    }

    public int getSize()
    {
        return data.Length;
    }

    //Read from message
    public byte readByte()
    {
        return data[offset++];
    }

    public byte[] readBytes()
    {
        return null;
    }

    public short readShort()
    {
        short outData = BitConverter.ToInt16(data, offset);
        offset += 2;
        return outData;
    }

    public int readInt()
    {
        int outData = BitConverter.ToInt32(data, offset);
        offset += 4;
        return outData;
    }

    public string readStringUTF8()
    {
        int strLength = readUShort();
        string outString = Encoding.UTF8.GetString(data, offset, strLength);
        offset += strLength;

        return outString;
    }

    public ushort readUShort()
    {
        ushort outData = BitConverter.ToUInt16(data, offset);
        offset += 2;
        return outData;
    }

    public uint readUInt()
    {
        uint outData = BitConverter.ToUInt32(data, offset);
        offset += 4;
        return outData;
    }
}