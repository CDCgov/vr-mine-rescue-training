using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;

public static class NetUtil
{
    public static Vector3 ReadVector3(this CodedInputStream reader)
    {
        Vector3 v = new Vector3();
        v.x = reader.ReadFloat();
        v.y = reader.ReadFloat();
        v.z = reader.ReadFloat();

        return v;
    }

    public static void WriteVector3(this CodedOutputStream writer, Vector3 v)
    {
        writer.WriteFloat(v.x);
        writer.WriteFloat(v.y);
        writer.WriteFloat(v.z);

    }

    public static Quaternion ReadQuaternion(this CodedInputStream reader)
    {
        Quaternion q = new Quaternion();
        q.x = reader.ReadFloat();
        q.y = reader.ReadFloat();
        q.z = reader.ReadFloat();
        q.w = reader.ReadFloat();

        return q;
    }

    public static void WriteQuaternion(this CodedOutputStream writer, Quaternion q)
    {
        writer.WriteFloat(q.x);
        writer.WriteFloat(q.y);
        writer.WriteFloat(q.z);
        writer.WriteFloat(q.w);
    }

}
