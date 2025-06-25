using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class StringMessageData : LogPacket {

    //public double TimeStamp;
    public string Message;

    public override void WriteData(BinaryWriter writer)
    {
        writer.Write(TimeStamp);
        writer.Write(Message);
    }

    public override void ReadData(BinaryReader reader)
    {
        TimeStamp = reader.ReadDouble();
        Message = reader.ReadString();
    }

    public override string ToString()
    {
        return "Message: " + Message + " @ " + TimeStamp;
    }
}
