using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class ProximityData : LogPacket {

    //public double TimeStamp;	
    public int ProxID;
    public int PlayerID;
    public MobileEqupmentType EquipmentType;
    public ProxZone ZoneState;
    public Vector3 Position;

    public override void WriteData(BinaryWriter writer)
    {
        writer.Write(TimeStamp);
        writer.Write(IsEvent);
        writer.Write(ProxID);
        writer.Write(PlayerID);
        writer.Write((int)EquipmentType);
        writer.Write((int)ZoneState);
        writer.Write(Position.x);
        writer.Write(Position.y);
        writer.Write(Position.z);
    }

    public override void ReadData(BinaryReader reader)
    {
        TimeStamp = reader.ReadDouble();
        IsEvent = reader.ReadBoolean();
        ProxID = reader.ReadInt32();
        PlayerID = reader.ReadInt32();
        EquipmentType = (MobileEqupmentType)reader.ReadInt32();
        ZoneState = (ProxZone)reader.ReadInt32();
        Position.x = reader.ReadSingle();
        Position.y = reader.ReadSingle();
        Position.z = reader.ReadSingle();
    }

    public override string ToString()
    {
        return TimeStamp + ", Is Event: " + IsEvent + ", ProxID: " + ProxID + ", Zone: " + ZoneState.ToString() + ", Equipment Type: " + EquipmentType.ToString() + ", Player: " + PlayerID + ", Position: " + Position.ToString();
    }
}