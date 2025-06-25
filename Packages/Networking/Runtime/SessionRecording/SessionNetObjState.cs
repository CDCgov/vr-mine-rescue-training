using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Google.Protobuf;
using System.Collections.Immutable;

public class SessionNetObjState : ISessionTimeSeriesData<SessionNetObjState>
{
    public struct NetObjState
    {
        public VRNSpawnObject SpawnData;
        public MemoryStream SyncData;
        public bool ObjectAlive;
        public string SocketID;

        public VRNNetObjMessage NetObjMessage;
        public VRNHeader NetObjMessageHeader;
        public byte[] NetObjMessageBuffer;

        public void ClearNetObjMessage()
        {
            NetObjMessageHeader = null;
            NetObjMessage = null;
            NetObjMessageBuffer = null;
        }
    }

    public Dictionary<System.Guid, NetObjState> Objects;
    public Dictionary<System.Guid, VRNSymbolManagerState> MineMaps;

    public SessionNetObjState()
    {
        Objects = new Dictionary<System.Guid, NetObjState>();
        MineMaps = new Dictionary<System.Guid, VRNSymbolManagerState>();
    }

    public void CopyTo(SessionNetObjState dest)
    {
        CopyTo(dest, true);
    }

    public void CopyTo(SessionNetObjState dest, bool includeNetObjMessages)
    {
        dest.MineMaps = new Dictionary<System.Guid, VRNSymbolManagerState>(MineMaps);

        if (includeNetObjMessages)
        {
            dest.Objects = new Dictionary<System.Guid, NetObjState>(Objects);
        }
        else
        {
            if (Objects != null)
            {
                dest.Objects = new Dictionary<System.Guid, NetObjState>();
                foreach (var kvp in Objects)
                {
                    var state = kvp.Value;

                    //temporary hack to keep clock sync messages
                    if (state.NetObjMessage ==  null || state.NetObjMessage.MessageType != "TIMER_SYNC")
                        state.ClearNetObjMessage();

                    dest.Objects[kvp.Key] = state;
                }
            }
            else
            {
                dest.Objects = null;
            }
        }

        //foreach (var kvp in Objects)
        //{
        //    dest.Objects.Add(kvp.Key, kvp.Value);
        //}

        //foreach (var kvp in MineMaps)
        //{
        //    dest.MineMaps.Add(kvp.Key, kvp.Value);
        //}
    }

    public void Interpolate(SessionNetObjState next, float interp, ref SessionNetObjState result)
    {
        if (result == null)
            result = new SessionNetObjState();

        CopyTo(result, true);
    }

    public void ClearNetObjMessages()
    {
        var objs = Objects.ToImmutableArray();
        foreach (var kvp in objs)
        {
            var state = kvp.Value;
            state.ClearNetObjMessage();
            Objects[kvp.Key] = state;
        }
    }

}
