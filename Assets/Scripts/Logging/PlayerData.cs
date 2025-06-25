using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class PlayerData : LogPacket {

    //public double TimeStamp;
    public int PlayerID;
    public Vector3 Position;
    public Quaternion Rotation;

    public override void WriteData(BinaryWriter writer)
    {
        writer.Write(TimeStamp);
        writer.Write(PlayerID);
        writer.Write(Position.x);
        writer.Write(Position.y);
        writer.Write(Position.z);
        writer.Write(Rotation.x);
        writer.Write(Rotation.y);
        writer.Write(Rotation.z);
        writer.Write(Rotation.w);
    }

    public override void ReadData(BinaryReader reader)
    {		
        TimeStamp = reader.ReadDouble();
        PlayerID = reader.ReadInt32();
        Position.x = reader.ReadSingle();
        Position.y = reader.ReadSingle();
        Position.z = reader.ReadSingle();
        Rotation.x = reader.ReadSingle();
        Rotation.y = reader.ReadSingle();
        Rotation.z = reader.ReadSingle();
        Rotation.w = reader.ReadSingle();
    }

    public override string ToString()
    {
        return "Player " + PlayerID + ": Position " + Position.ToString() + ", Rotation " + Rotation.ToString() + " @ " + TimeStamp;
    }
}
