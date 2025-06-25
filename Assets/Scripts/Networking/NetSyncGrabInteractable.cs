using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Google.Protobuf;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;


[RequireComponent(typeof(CustomXRInteractable))]
[RequireComponent(typeof(NetworkedObject))]
public class NetSyncGrabInteractable : MonoBehaviour, INetSync
{
    public const float MaxSyncInterval = 5;

	public NetworkManager NetworkManager;
    public NetworkedObjectManager NetworkedObjectManager;
	public bool SmoothTranslationUpdate = true;
    public bool LockPosition = false;
    public bool SyncLocalPosition = false;

	private VRNTransformData _xform;    

	private Vector3 _targetPos;
	private Quaternion _targetRot;

	private bool _receivingData = false;

	private CustomXRInteractable _grabInteractable;
	private NetworkedObject _netObj;
	private Rigidbody _rb;

    private Vector3 _lastSyncPos;
    private Quaternion _lastSyncRot;
    private float _lastSyncTime = -500;

	//private float _velocity = 0;

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
        //if (_xform == null)
        //	_xform = VRNTransformData.Parser.ParseDelimitedFrom(reader);
        //else
        //	_xform.MergeDelimitedFrom(reader);        
        //_xform = VRNTransformData.Parser.ParseDelimitedFrom(reader);
        if (_xform == null)
            _xform = new VRNTransformData();

        _xform.ResetTransformData();
        
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
		//If we want an approximate predictive engine here we can probably do a projectile calculation, thanks to the Rigidbody we hav the initial velocity, we can assume there is only 1g acceleration in the y direction
		//If we can get t from the network manager where t is the average latency time we can do a light prediction
		// Xf = X0 + Vx*t
		// Zf = Z0 + Vz*t
		// Yf = Y0 + Vy*t - 0.5*9.8*t^2
		// One question I have is this: What is the predictive positioning for? We don't have any behavior that requires this (i.e. we are not actually tossing items to each other, handoffs yes)
		_receivingData = false;
        //VRNTransformData xform = new VRNTransformData();
        if (_xform == null)
            _xform = new VRNTransformData();

        if (SyncLocalPosition)
        {
            _xform.Rotation = transform.localRotation.ToVRNQuaternion();
            _xform.Position = transform.localPosition.ToVRNVector3();
        }
        else
        {
            _xform.Rotation = transform.rotation.ToVRNQuaternion();
            _xform.Position = transform.position.ToVRNVector3();
        }

        //_xform.WriteDelimitedTo(writer);
        writer.WriteMessage(_xform);

        _lastSyncTime = Time.time;
        _lastSyncPos = transform.position;
        _lastSyncRot = transform.rotation;

        //if (name.Contains("CareVent"))
        //    Debug.Log($"{name}: {_lastSyncPos} {_lastSyncRot}");
	}

	void Start()
	{
		if (NetworkManager == null)
			NetworkManager = NetworkManager.GetDefault(gameObject);
        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);

		_rb = GetComponent<Rigidbody>();

		//_xform = new VRNTransformData();
		//_xform.Rotation = new VRNQuaternion();
		//_xform.Position = new VRNVector3();
		_xform = null;

		_targetPos = transform.position;
		_targetRot = transform.rotation;

		_grabInteractable = GetComponent<CustomXRInteractable>();
		//_grabInteractable.onSelectEnter.AddListener(OnSelectEnter);

		_netObj = GetComponent<NetworkedObject>();
	}

	void Update()
	{
        if (!_netObj.HasAuthority)
        {
            _rb.isKinematic = true;
        }

        bool isSocketed = false;
        if (_grabInteractable != null && _grabInteractable.CurrentOwner is CustomXRSocket)
            isSocketed = true;

		if (_receivingData && !LockPosition && !_netObj.HasAuthority && !isSocketed)
		{
            Vector3 curPos;
            Quaternion curRot;

            if (SyncLocalPosition)
            {
                curPos = transform.localPosition;
                curRot = transform.localRotation;
            }
            else
            {
                curPos = transform.position;
                curRot = transform.rotation;
            }

            if (SmoothTranslationUpdate)
			{
				float dist = Vector3.Distance(curPos, _targetPos);

				if (dist < 0.1 || dist > 2)
				{
					curPos = _targetPos;
				}
				else
				{
					//float vel = dist * 4; //cover the distance in 0.25 seconds
					//transform.position = Vector3.MoveTowards(transform.position, _targetPos, Time.deltaTime * vel);

					curPos = Vector3.Lerp(curPos, _targetPos, 0.5f);
				}
			}
			else
			{
				curPos = _targetPos;
			}

			//transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRot, Time.deltaTime * 1200);
			curRot = _targetRot;

            if (SyncLocalPosition)
            {
                transform.localPosition = curPos;
                transform.localRotation = curRot;
            }
            else
            {
                transform.position = curPos;
                transform.rotation = curRot;
            }
		}
	}
}
