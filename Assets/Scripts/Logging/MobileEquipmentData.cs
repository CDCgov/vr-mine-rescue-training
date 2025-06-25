using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class MobileEquipmentData : LogPacket 
{
    //public double TimeStamp;
    public int MobileEquipmentType;
    public int MobileEquipmentID;
    public Vector3 Position;
    public Quaternion Rotation;
    public float Velocity;

    public override void WriteData(BinaryWriter writer)
    {
        writer.Write(TimeStamp);
        writer.Write(IsEvent);
        writer.Write(MobileEquipmentType);
        writer.Write(MobileEquipmentID);
        writer.Write(Position.x);
        writer.Write(Position.y);
        writer.Write(Position.z);
        writer.Write(Rotation.x);
        writer.Write(Rotation.y);
        writer.Write(Rotation.z);
        writer.Write(Rotation.w);
        writer.Write(Velocity);
    }

    public override void ReadData(BinaryReader reader)
    {
        TimeStamp = reader.ReadDouble();
        IsEvent = reader.ReadBoolean();
        MobileEquipmentType = reader.ReadInt32();
        MobileEquipmentID = reader.ReadInt32();
        Position.x = reader.ReadSingle();
        Position.y = reader.ReadSingle();
        Position.z = reader.ReadSingle();
        Rotation.x = reader.ReadSingle();
        Rotation.y = reader.ReadSingle();
        Rotation.z = reader.ReadSingle();
        Rotation.w = reader.ReadSingle();
        Velocity = reader.ReadSingle();
    }

    public override string ToString()
    {
        return TimeStamp + ": Mobile Equipment " + MobileEquipmentID + ": " + (MobileEqupmentType)MobileEquipmentType + ", Position " + Position.ToString() + ", Rotation " + Rotation.ToString() + ", Velocity " + Velocity.ToString();
    }
}