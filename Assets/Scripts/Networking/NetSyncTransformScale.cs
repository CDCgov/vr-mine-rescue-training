using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetSyncTransformScale : MonoBehaviour, INetSync
{
    private Vector3 _lastScaleValue;

    private VRNVector3 _vrnScale;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Awake()
    {
        _lastScaleValue = transform.localScale;
        _vrnScale = new VRNVector3();
    }

    public bool NeedsUpdate()
    {
        if (_lastScaleValue != transform.localScale)
            return true;
        else
            return false;
    }

    public void SyncObjState(CodedInputStream reader)
    {
        _vrnScale.ResetData();

        reader.ReadMessage(_vrnScale);

        transform.localScale = _vrnScale.ToVector3();
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        _vrnScale = transform.localScale.ToVRNVector3();
        writer.WriteMessage(_vrnScale);
    }
}
