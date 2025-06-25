using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Google.Protobuf;


[System.Obsolete]
public class NetSyncSocketTarget : MonoBehaviour
{
    public string TargetToSync = "";
    public SocketManager SocketManager;
    private bool _receivingData = false;
    private float _updateTime = 0;
    private string _priorTarget = "";
    private NetworkedObject _netObj;
    public bool NeedsUpdate()
    {
        if (Time.time > _updateTime)
        {
            _updateTime = Time.time + 1;
            return true;
        }
        return false;
    }

    public void SyncObjState(CodedInputStream reader)
    {
        //_receivingData = true;
        //TargetToSync = VRNTextMessage.Parser.ParseDelimitedFrom(reader).Message;
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        //_receivingData = false;
        //if (TargetToSync == null)
        //{
        //    TargetToSync = "";
        //}
        //VRNTextMessage text = new VRNTextMessage
        //{
        //    Message = TargetToSync
        //};
        //text.WriteDelimitedTo(writer);
    }

    private void Start()
    {
        if (SocketManager == null)
            SocketManager = SocketManager.GetDefault(gameObject);

        if (_netObj == null)
        {
            _netObj = GetComponent<NetworkedObject>();
        }
    }

    //private void Update()
    //{
    //    if (_receivingData)
    //    {
    //        if (TargetToSync != _priorTarget)
    //        {
    //            SocketManager.AssignSocket(TargetToSync, _netObj.uniqueID.ToString());
    //        }
    //    }
    //}
}
