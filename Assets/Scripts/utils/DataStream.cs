using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


//Class for easy reading of a bytestream
//TODO: Just move on to MemoryStream and BinaryReader

public class DataStream
{
    private byte[] data;
    private List<byte[]> writeList; //Data to be written
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
        int byteSize = BitConverter.ToInt32(data, offset);
        offset += 4;
        //TODO: Security: Set limit on byteSize so as not to crash the computer(If that can happen with for example a 2GB allocation from int)
        byte[] readData = new byte[byteSize];

        Array.Copy(data, offset, readData, 0, byteSize);

        offset += byteSize;

        return readData;
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

    public static string readStreamStringUTF8(BinaryReader reader)
    {
        ushort strLength = reader.ReadUInt16();
        byte[] strBytes = reader.ReadBytes(strLength);
        string outString = Encoding.UTF8.GetString(strBytes);

        return outString;
    }

    //Write string to binary stream
    public static void writeStringUTF8ToStream(BinaryWriter stream, string str)
    {
        byte[] strBytes = Encoding.UTF8.GetBytes(str);
        stream.Write((ushort)strBytes.Length);
        stream.Write(strBytes);
    }
}