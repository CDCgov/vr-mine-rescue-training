using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;

public interface INetSync
{
    bool NeedsUpdate();
    void WriteObjState(CodedOutputStream writer);
    void SyncObjState(CodedInputStream reader);
}