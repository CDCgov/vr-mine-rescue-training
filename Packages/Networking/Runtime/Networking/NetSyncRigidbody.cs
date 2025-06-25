using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;


public class NetSyncRigidbody : MonoBehaviour, INetSync
{
    public const float MaxSyncInterval = 5;

    public bool SmoothTranslationUpdate = true;

    private Vector3 _targetPos;
    private Quaternion _targetRot;

    private Rigidbody _rb;
    private bool _receivingData = false;
    private float _lastSyncTime = -500;

    private Vector3 _lastSyncPos;
    private Quaternion _lastSyncRot;
    private Vector3 _lastSyncVelocity;
    private bool _isKinematic = false;
    private bool _useGravity = true;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public bool NeedsUpdate()
    {
        if (_rb == null)
            return false;

        if (Time.time - _lastSyncTime > MaxSyncInterval)
            return true;

        Vector3 posDelta = transform.position - _lastSyncPos;
        float angleDelta = Quaternion.Angle(_rb.rotation, _lastSyncRot);

        if (posDelta.magnitude > 0.005f || angleDelta > 0.5f)
            return true;

        return false;
    }

    public void SyncObjState(CodedInputStream reader)
    {
        _receivingData = true;

        _lastSyncPos = reader.ReadVector3();
        _lastSyncRot = reader.ReadQuaternion();
        _lastSyncVelocity = reader.ReadVector3();
        _isKinematic = reader.ReadBool();
        _useGravity = reader.ReadBool();

        gameObject.SetActive(true);
        transform.position = _lastSyncPos;
        transform.rotation = _lastSyncRot;

        if (_rb != null)
        {
            _rb.velocity = _lastSyncVelocity;
            _rb.useGravity = _useGravity;
            _rb.isKinematic = _isKinematic;
        }
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        _receivingData = false;

        if (_rb != null)
        {
            _lastSyncPos = _rb.position;
            _lastSyncRot = _rb.rotation;
            _lastSyncVelocity = _rb.velocity;
            _isKinematic = _rb.isKinematic;
        }

        writer.WriteVector3(_lastSyncPos);
        writer.WriteQuaternion(_lastSyncRot);
        writer.WriteVector3(_lastSyncVelocity);
        writer.WriteBool(_isKinematic);
        writer.WriteBool(_useGravity);

        _lastSyncTime = Time.time;
    }

    void FixedUpdate()
    {
        
        if (_receivingData)
        {
            var speed = _lastSyncVelocity.magnitude;
            //var dist = Vector3.Distance(_lastSyncPos, _rb.position);
            var delta = _lastSyncPos - _rb.position;
            var dist = delta.magnitude;

            if (dist > 0.5f || speed < 0.1f)
            { 
                    _rb.position = _lastSyncPos;
            }
            else
            {
                delta.Normalize();
                var correctionVel = delta / Time.fixedDeltaTime;
                _rb.velocity += correctionVel;
            }

        }

        //if (_receivingData)
        //{
        //    if (Vector3.Distance(transform.position, _targetPos) > 2.0)
        //    {
        //        transform.position = _targetPos;
        //    }
        //    else
        //    {
        //        transform.position = Vector3.MoveTowards(transform.position, _targetPos, Time.deltaTime * 4.0f);
        //    }
        //    transform.rotation = _targetRot;
        //}
    }
}
