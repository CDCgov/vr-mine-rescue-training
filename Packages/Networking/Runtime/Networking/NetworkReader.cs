using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

public class NetworkReader
{
    public NetworkReader(byte[] data)
    {

    }

    public string ReadString()
    {
        return "";
    }

    public float ReadSingle()
    {
        return 0;
    }

    public double ReadDouble()
    {
        return 0;
    }

    public Vector3 ReadVector3()
    {
        return Vector3.zero;
    }

    public Quaternion ReadQuaternion()
    {
        return Quaternion.identity;
    }

    public int ReadInt32()
    {
        return 0;
    }

    public UInt64 ReadUInt64()
    {
        return 0;
    }

    public UInt32 ReadUInt32()
    {
        return 0;
    }

    public UInt16 ReadUInt16()
    {
        return 0;
    }

    public byte ReadByte()
    {
        return 0;
    }

    public bool ReadBoolean()
    {
        return false;
    }

    

    public void SeekZero()
    {

    }
}