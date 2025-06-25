using UnityEngine;
using System.Collections;
using System;
using System.IO;

/// <summary>
/// Class to handle log packets for events that happen once, rather than frame by frame
/// </summary>
public class EventLogData : LogPacket {

    public int EventID;
    public string EventDescriptor; //TO DO: Replace with Event Type?
    public Vector3 EventLocation;

    public override void WriteData(BinaryWriter writer)
    {
        writer.Write(TimeStamp);
        writer.Write(EventID);
        writer.Write(EventDescriptor);
        writer.Write(EventLocation.x);
        writer.Write(EventLocation.y);
        writer.Write(EventLocation.z);
    }

    public override void ReadData(BinaryReader reader)
    {
        TimeStamp = reader.ReadDouble();
        EventID = reader.ReadInt32();
        EventDescriptor = reader.ReadString();
        EventLocation.x = reader.ReadSingle();
        EventLocation.y = reader.ReadSingle();
        EventLocation.z = reader.ReadSingle();
        IsEvent = true;
    }

    public override string ToString()
    {
        return TimeStamp + ", EventID: " + EventID + ", Event Description: " + EventDescriptor + ", Location: " + EventLocation; 
    }
}
