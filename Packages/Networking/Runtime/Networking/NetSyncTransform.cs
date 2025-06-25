using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Google.Protobuf;

public class NetSyncTransform : MonoBehaviour, INetSync
{
    public const float MaxSyncInterval = 5;

    public bool SmoothTranslationUpdate = true;
	private VRNTransformData _xform;

	private Vector3 _targetPos;
	private Quaternion _targetRot;

	private bool _receivingData = false;

    //private float _velocity = 0;

    private Vector3 _lastSyncPos;
    private Quaternion _lastSyncRot;
    private float _lastSyncTime = -500;
    private NetworkedObject _netObj = null;

    public bool NeedsUpdate()
	{
        if (Time.time - _lastSyncTime > MaxSyncInterval)
            return true;

        Vector3 posDelta = transform.position - _lastSyncPos;
        float angleDelta = Quaternion.Angle(transform.rotation, _lastSyncRot);

        if (posDelta.magnitude > 0.005f || angleDelta > 0.5f)
            return true;

        return false;
    }

	public void SyncObjState(CodedInputStream reader)
	{
		_receivingData = true;
		
        //_xform = VRNTransformData.Parser.ParseDelimitedFrom(reader);
        if (_xform == null)
        {
            _xform = new VRNTransformData();
        }

        reader.ReadMessage(_xform);

        if (_xform.Position != null)
		{
			//transform.position = _xform.Position.ToVector3();
			_targetPos = _xform.Position.ToVector3();
		}

		if (_xform.Rotation != null)
		{
			//transform.rotation = _xform.Rotation.ToQuaternion();
			_targetRot = _xform.Rotation.ToQuaternion();
		}
	}

	public void WriteObjState(CodedOutputStream writer)
	{
		_receivingData = false;
        //VRNTransformData xform = new VRNTransformData
        //{
        //	Rotation = transform.rotation.ToVRNQuaternion(),
        //	Position = transform.position.ToVRNVector3(),
        //};
        if (_xform == null)
            _xform = new VRNTransformData();

        _xform.Rotation = transform.rotation.ToVRNQuaternion();
        _xform.Position = transform.position.ToVRNVector3();

        //xform.WriteDelimitedTo(writer);
        writer.WriteMessage(_xform);

        _lastSyncTime = Time.time;
        _lastSyncPos = transform.position;
        _lastSyncRot = transform.rotation;
    }

	void Start ()
	{
		_xform = new VRNTransformData();
		_targetPos = transform.position;
		_targetRot = transform.rotation;

        _netObj = GetComponent<NetworkedObject>();
	}
	
	void Update ()
	{
		if (_receivingData && (_netObj == null || !_netObj.HasAuthority))
		{
            //if (SmoothTranslationUpdate)
            //{
            //	float dist = Vector3.Distance(transform.position, _targetPos);

            //	if (dist < 0.1)
            //	{
            //		transform.position = _targetPos;
            //	}
            //	else
            //	{
            //		//float vel = dist * 4; //cover the distance in 0.25 seconds
            //		//transform.position = Vector3.MoveTowards(transform.position, _targetPos, Time.deltaTime * vel);

            //		transform.position = Vector3.Lerp(transform.position, _targetPos, 0.5f);

            //	}
            //}
            //else
            //{
            //	transform.position = _targetPos;
            //}
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRot, Time.deltaTime * 1200);

            if (Vector3.Distance(transform.position, _targetPos) > 2.0)
            {
                transform.position = _targetPos;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, _targetPos, Time.deltaTime * 4.0f);
            }
            transform.rotation = _targetRot;
		}
	}
}