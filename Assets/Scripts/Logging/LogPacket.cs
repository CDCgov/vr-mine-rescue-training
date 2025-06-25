using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
/// <summary>
/// Base class for the packets of data that will be logged by the program. Each type of data is a child class of LogPacket as each data type might have different types of data associated with it.
/// </summary>
public abstract class LogPacket {

    private static Dictionary<int, Type> _packetIDToType;
    private static Dictionary<Type, int> _typeToPacketTypeID;

    public bool IsEvent = false;
    public double TimeStamp;
    public int Frame;

    static LogPacket()
    {
        _packetIDToType = new Dictionary<int, Type>();
        _typeToPacketTypeID = new Dictionary<Type, int>();

        //Register ID types
        RegisterPacketID(1, typeof(PlayerData));
        RegisterPacketID(2, typeof(MobileEquipmentData));
        RegisterPacketID(3, typeof(ProximityData));
        RegisterPacketID(99, typeof(EventLogData));
        RegisterPacketID(100, typeof(StringMessageData));		
    }

    public static void RegisterPacketID(int packetID, Type type)
    {
        _packetIDToType.Add(packetID, type);
        _typeToPacketTypeID.Add(type, packetID);
    }

    public static int GetPacketTypeID(Type type)
    {
        try
        {
            return _typeToPacketTypeID[type];
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public static Type GetPacketType(int packetID)
    {
        try
        {
            return _packetIDToType[packetID];
        }
        catch (Exception)
        {
            return null;
        }
    }

    public abstract void WriteData(BinaryWriter writer);
    public abstract void ReadData(BinaryReader reader);

    public byte[] GetBytes()
    {
        using (MemoryStream memStream = new MemoryStream())
            using(BinaryWriter writer = new BinaryWriter(memStream))
        {
            writer.Write(GetPacketTypeID(GetType()));
            WriteData(writer);

            writer.Flush();
            return memStream.ToArray();
        }
    }

    public static LogPacket DecodePacket(byte[] data)
    {
        using (MemoryStream memStream = new MemoryStream(data))
            using(BinaryReader reader = new BinaryReader(memStream))
        {
            //identifying packet type
            int packetTypeID = reader.ReadInt32();
            Type packetType = GetPacketType(packetTypeID);

            LogPacket packet = (LogPacket)Activator.CreateInstance(packetType);

            packet.ReadData(reader);

            

            return packet;
        }
    }	
}
