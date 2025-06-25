using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum MandoorAnimState
{
	MoveToStartPoint,
	MoveToEndPoint,
	ArrivedAtEndPoint,
	None
}
[RequireComponent(typeof(NetworkedObject))]
public class MandoorInteract : Interactable {

    public float MaximumDoorAngle = 90;
    public bool RotateCounterClockwise = false;

	public Vector3 DoorEntryPointOne;
	public Vector3 DoorEntryPointTwo;

	public GameObject DoorHandle;
	[Range(0,1)]
	public float HeadDuckingDistance = 0.5f;
    public bool MoveActorThroughDoor = true;
    public AudioClip MandoorOpen;
    public AudioClip MandoorClose;

    public MineVentControlHost MineVent;
    public double ClosedResistance = 100;
    public double OpenResistance = 50;

    public bool UseQuaternionLerp = false;
    public Vector3 TargetRotation;
    private Quaternion _fromRotation;
    private Quaternion _toRotation;
    private float _lerpCount = 0;
    

	private bool _IsAnimating = false;
	private bool _RotationDirection = false; //False = door closing, True = door opening
	private bool _MoveThroughDoor = false;
	private bool _MoveThroughDoorComplete = false;
	private bool _AnimateHandle = false;
	private bool _HandleRotated = false;
	private float _TargetDoorAngle = 90;
	private float _StartingAngle = 0;
	private NetworkedObject _netObj;
	private float _degreesMoved = 0;
	private MandoorAnimState _manDoorAnimState = MandoorAnimState.None;
	private Vector3 _startPoint;
	private Vector3 _endPoint;
	private Vector3 _requesterInitPos;
	private float _initTime;
	private ActorHost _requester;
	private Vector3 _requesterHeadInitLocalPos;
    private AudioSource _manDoorAudioSource;
    private float _clockwiseRotation = 1;

    // Use this for initialization
    void Start() {
        //base.Start();
        _netObj = GetComponent<NetworkedObject>();
        _StartingAngle = transform.localRotation.eulerAngles.y;
        //Debug.Log("Start ang: " + _StartingAngle);
        _manDoorAudioSource = gameObject.GetComponent<AudioSource>();
        if (MineVent != null)
        {
            MineVent.MineVentControl.AddedResistance = ClosedResistance;
        }
        _RotationDirection = RotateCounterClockwise;

        _toRotation = Quaternion.Euler(TargetRotation);
        _fromRotation = transform.localRotation;
	}
	
	// Update is called once per frame
	void Update () {

		if (true)
		{
			if (_AnimateHandle)
			{
				Vector3 rot = DoorHandle.transform.localRotation.eulerAngles;
				if (!_HandleRotated)
				{                    
					rot.z += Time.deltaTime*50;
					if (rot.z >= 90)
					{
						rot.z = 90;
						_HandleRotated = true;
					}
					DoorHandle.transform.localRotation = Quaternion.Euler(rot);                    
				}
				else
				{
					rot.z -= Time.deltaTime * 50;
					if (rot.z <= 0)
					{
						rot.z = 0;
						_AnimateHandle = false;
						_HandleRotated = false;
					}
					DoorHandle.transform.localRotation = Quaternion.Euler(rot);                    
				}
			}
            if (UseQuaternionLerp)
            {
                if (_IsAnimating) {
                    transform.localRotation = Quaternion.Lerp(_fromRotation, _toRotation, _lerpCount);
                    if (!_RotationDirection)
                    {
                        _lerpCount += Time.deltaTime;
                        if (_lerpCount > 1)
                        {
                            _IsAnimating = false;
                            _lerpCount = 1;
                        }
                    }
                    else
                    {
                        _lerpCount -= Time.deltaTime;
                        if (_lerpCount < 0)
                        {
                            _IsAnimating = false;
                            _lerpCount = 0;
                        }
                    }
                }
            }
            else
            {
                if (_IsAnimating)
                {

                    if (_RotationDirection)
                    {
                        if (_degreesMoved <= MaximumDoorAngle)
                        {
                            //Debug.Log("rotating" + transform.localRotation.eulerAngles.y);
                            transform.Rotate(0, Time.deltaTime * 20, 0);
                            _degreesMoved += Time.deltaTime * 20;
                        }
                        else
                        {

                            //Debug.Log("Stopped rotating" + transform.localRotation.eulerAngles.y);
                            _initTime = Time.time;

                            if (MoveActorThroughDoor)
                            {
                                _requesterInitPos = _requester.transform.position;
                                _MoveThroughDoor = true;
                                _manDoorAnimState = MandoorAnimState.MoveToStartPoint;
                            }
                            _IsAnimating = false;
                            //MineVent.SetResistance(OpenResistance);
                            if (MineVent != null)
                            {
                                MineVent.MineVentControl.AddedResistance = OpenResistance;
                            }
                        }
                    }
                    else
                    {
                        if (_degreesMoved > 0)
                        {
                            transform.Rotate(0, -Time.deltaTime * 20, 0);
                            _degreesMoved -= Time.deltaTime * 20;
                        }
                        else
                        {
                            _IsAnimating = false;
                            //MineVent.SetResistance(ClosedResistance);
                            if (MineVent != null)
                            {
                                MineVent.MineVentControl.AddedResistance = ClosedResistance;
                            }
                        }
                    }
                }
                if (_MoveThroughDoor)
                {
                    switch (_manDoorAnimState)
                    {
                        case MandoorAnimState.MoveToStartPoint:
                            _requester.SetAnimWalkSpeed(1);
                            Debug.Log("Init pos: " + _requesterInitPos);
                            _requester.transform.position = Vector3.Lerp(_requesterInitPos, _startPoint, (Time.time - _initTime) / 2);
                            _requester.HeadTransform.localPosition = Vector3.Lerp(_requesterHeadInitLocalPos, (_requesterHeadInitLocalPos - new Vector3(0, HeadDuckingDistance, 0)), (Time.time - _initTime) / 2);
                            if (Vector3.Distance(_requester.transform.position, _startPoint) < 0.01f)
                            {
                                _manDoorAnimState = MandoorAnimState.MoveToEndPoint;
                                Debug.Log("Actor pos: " + _requester.transform.position + ", start point: " + _startPoint + ", end point? " + _endPoint);
                                _initTime = Time.time;
                            }
                            break;
                        case MandoorAnimState.MoveToEndPoint:
                            _requester.transform.position = Vector3.Lerp(_startPoint, _endPoint, (Time.time - _initTime) / 2);
                            Debug.Log("Actor pos: " + _requester.transform.position + ", start point: " + _startPoint + ", end point? " + _endPoint);
                            if (Vector3.Distance(_requester.transform.position, _endPoint) < 0.01f)
                            {
                                _manDoorAnimState = MandoorAnimState.ArrivedAtEndPoint;
                                Debug.Log("Actor pos: " + _requester.transform.position + ", start point: " + _startPoint + ", end point? " + _endPoint);
                            }
                            break;
                        case MandoorAnimState.ArrivedAtEndPoint:
                            _MoveThroughDoor = false;
                            _IsAnimating = false;
                            _requester.SetAnimWalkSpeed(0);
                            _requester.HeadTransform.localPosition = Vector3.Lerp((_requesterHeadInitLocalPos - new Vector3(0, HeadDuckingDistance, 0)), _requesterHeadInitLocalPos, (Time.time - _initTime) / 2);
                            if (Vector3.Distance(_requesterHeadInitLocalPos, _requester.HeadTransform.localPosition) < 0.01f)
                            {
                                _requester.HeadTransform.localPosition = _requesterHeadInitLocalPos;
                                _manDoorAnimState = MandoorAnimState.None;
                            }
                            break;
                        case MandoorAnimState.None:
                            //_requester.LockMotion = false;
                            break;
                        default:
                            break;
                    }

                }
            }
		}
	}
	public bool NeedsUpdate()
	{
		return true;
	}

	public string GetObjectDisplayName()
	{
		return gameObject.name;
	}

    //private void OnMouseUp()
    //{
    //    if (_netObj.HasAuthority)
    //    {
    //        _RotationDirection = !_RotationDirection;
    //        _IsAnimating = true;


    //    }        
    //}

    //public void WriteObjState(NetworkWriter writer)
    //{
    //    writer.Write(transform.localRotation);
    //}
    //public void SyncObjState(NetworkReader reader)
    //{
    //    transform.localRotation = reader.ReadQuaternion();
    //}
    public override void Interact()
    {
        Debug.Log("Door clicked");
        if (_RotationDirection)
        {
            _manDoorAudioSource.clip = MandoorOpen;
        }
        else
        {
            _manDoorAudioSource.clip = MandoorClose;
        }
        _manDoorAudioSource.Play();
        _RotationDirection = !_RotationDirection;
        MoveActorThroughDoor = false;
        _IsAnimating = true;
        _AnimateHandle = true;
        _initTime = Time.time;
    }
    public override void Interact(ActorHost requester)
	{
		//base.Interact();
		Debug.Log("Do Stuff");
		_RotationDirection = !_RotationDirection;
        MoveActorThroughDoor = true;
        _IsAnimating = true;
		_AnimateHandle = true;
		_requester = requester;
		_requesterInitPos = requester.transform.position;
		_requesterHeadInitLocalPos = requester.HeadTransform.localPosition;
		_initTime = Time.time;
		DetermineStartPoint();
		Debug.Log("ActorPos? " + _requester.transform.position);
	}

	void DetermineStartPoint()
	{
		if(Vector3.Distance(_requester.transform.position, transform.TransformPoint(DoorEntryPointOne)) < Vector3.Distance(_requester.transform.position, transform.TransformPoint(DoorEntryPointTwo)))
		{
			_startPoint = transform.TransformPoint(DoorEntryPointOne);
			_endPoint = transform.TransformPoint(DoorEntryPointTwo);
		}
		else
		{
			_startPoint = transform.TransformPoint(DoorEntryPointTwo);
			_endPoint = transform.TransformPoint(DoorEntryPointOne);
		}
	}
}
