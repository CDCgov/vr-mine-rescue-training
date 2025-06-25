using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using Google.Protobuf;

public class ActorHost : MonoBehaviour, INetSync, IInputTarget, ISelectableObject
{
    public SystemManager SystemManager;
    public Actor HostedActor;
    public Transform HeadTransform;
    public GameObject BodyModel;
    public float Speed = 3.0f;

    public GameObject HandSprite;

    [NonSerialized]
    public string ActorName;

    /// <summary>
    /// the intended direction of motion set from the input device
    /// </summary>
    public Vector3 InputMoveVector;
    /// <summary>
    /// the intended direction of view euler angles (pitch, yaw, roll) set by the input device
    /// </summary>
    public Vector3 InputLookEuler;

    public bool LockMotion = false;

    protected LineRenderer _lineRenderer;
    protected GameObject _camera;
    protected Rigidbody _rigidbody;
    protected InputTargetOptions _inputOptions = null;
    protected Animator _animator;
    protected NetworkedObject _netObj;

    private float _animWalkSpeed = 0;

    private bool _initialized = false;

    public ActorHost()
    {
        ActorName = System.Environment.MachineName;
        _inputOptions = new InputTargetOptions();
        _inputOptions.ToggleMouseCapture = true;
    }

    public void BindToActor(Actor actor)
    {
        HostedActor = actor;
    }


    protected virtual void ShowBody(bool bShow)
    {
        if (BodyModel != null)
        {
            BodyModel.SetActive(bShow);
        }
    }

    public void ShowViewDirection(bool bShow)
    {
        if (bShow && _lineRenderer == null)
        {
            //_lineRenderer = gameObject.AddComponent<LineRenderer>();
            
            
        }
    }

    public virtual void EnableCamera(bool bEnable)
    {
        if (bEnable)
        {
            
            if (_camera == null)
            {
                // _camera = Resources.Load<GameObject>("FirstPersonCamera");
                // _camera = Instantiate<GameObject>(_camera);
                _camera = SystemManager.CreateCamera();

                _camera.transform.SetParent(HeadTransform, false);
            }
        }
        else
        {
            if (_camera != null)
                _camera.SetActive(false);
        }

        ShowBody(!bEnable); // hide body when in first-person camera mode
    }

    public void EnableCapLamp(bool bEnable)
    {

    }

    void OnDrawGizmos()
    {
        if (HeadTransform != null)
            Gizmos.DrawLine(HeadTransform.position, HeadTransform.position + HeadTransform.forward * 2);
    }

    private void Awake() {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
    }

    protected virtual void Start () 
    {
        
        Debug.Assert(HeadTransform != null);
        _rigidbody = GetComponent<Rigidbody>();

        MasterControl.SceneControl.AddActorHost(this);

        _animator = GetComponentInChildren<Animator>();
        _netObj = GetComponent<NetworkedObject>();

        InputLookEuler = transform.rotation.eulerAngles;
    }

    protected virtual void OnDestroy()
    {
        MasterControl.SceneControl.RemoveActorHost(this);
    }
    
    protected virtual void Update () 
    {
        if (_netObj.HasAuthority)
        {
            InputMoveVector.y = 0;
            _animWalkSpeed = InputMoveVector.magnitude;
            Vector3 motion = InputMoveVector * Speed * Time.deltaTime;
            motion = transform.rotation * motion;


            if (!LockMotion)
            {
                transform.position += motion;
            }
            RaycastHit hit;
            if (Physics.Raycast(HeadTransform.position, HeadTransform.forward, out hit, 10f))
            {
                Interactable inter = hit.collider.GetComponent<Interactable>();
                if (inter != null)
                {
                    if(HandSprite != null)
                    {
                        HandSprite.SetActive(true);
                        HandSprite.transform.forward = hit.normal;
                        HandSprite.transform.position = hit.point + hit.normal.normalized * 0.01f;                        
                        //HandSprite.transform.Translate(new Vector3(0, 0, 0.001f), Space.Self);
                    }
                    if (Input.GetButtonUp("Fire1"))
                    {
                        inter.Interact(this);
                    }
                }
                else
                {
                    if (HandSprite != null)
                    {
                        HandSprite.SetActive(false);
                    }
                }
            }
            else
            {
                if (HandSprite != null)
                {
                    HandSprite.SetActive(false);
                }
            }
        }
        if (!LockMotion)
        {
            transform.rotation = Quaternion.AngleAxis(InputLookEuler.y, Vector3.up);
            HeadTransform.localRotation = Quaternion.AngleAxis(InputLookEuler.x, Vector3.right);
        }
        if (_animator != null)
        {
            
            _animator.SetFloat("Walking", _animWalkSpeed);
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (_animator != null)
        {
            _animator.SetLookAtPosition(HeadTransform.position + HeadTransform.forward * 3);
        }
    }

    public virtual bool NeedsUpdate()
    {
        return true;
    }

    public virtual void WriteObjState(CodedOutputStream writer)
    {
        /*writer.Write(ActorName);
        writer.Write(transform.position);
        writer.Write(_animWalkSpeed);
        //writer.Write(transform.rotation);
        //writer.Write(HeadTransform.rotation);
        writer.Write(InputLookEuler);*/
    }

    public virtual void SyncObjState(CodedInputStream reader)
    {
        InitializeActorHost();

        //ActorName = reader.ReadString();
        //Vector3 rootPos = reader.ReadVector3();
        //_animWalkSpeed = reader.ReadSingle();
        //Vector3 lookEuler = reader.ReadVector3();		
        ////Quaternion rootRot = reader.ReadQuaternion();
        ////Quaternion headRot = reader.ReadQuaternion();

        //transform.position = rootPos;
        //InputLookEuler = lookEuler;
        ////transform.rotation = rootRot;
        ////HeadTransform.rotation = headRot;
    }

    public Vector3 GetMovementVector()
    {
        return InputMoveVector;
    }

    public void SetMovementVector(Vector3 moveVector)
    {
        InputMoveVector = moveVector;
    }

    public void SetLookEuler(Vector3 eulerAngles)
    {
        InputLookEuler = eulerAngles;
    }

    public Vector3 GetLookEuler()
    {
        return InputLookEuler;
    }

    public void ProcessKeyboardInput()
    {
        
    }

    public InputTargetOptions GetInputTargetOptions()
    {
        return _inputOptions;
    }

    public virtual void ProcessCustomInput()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            RaycastHit hit;
            if (Physics.Raycast(HeadTransform.position, HeadTransform.forward, out hit))
            {

                GameObject cableTest = new GameObject("TensionedCableTest");
                MeshRenderer mr = cableTest.AddComponent<MeshRenderer>();
                mr.material = Resources.Load<Material>("CableTestMat");

                TensionedCable tcable = cableTest.AddComponent<TensionedCable>();
                GameObject cableAnchor = new GameObject("CableAnchor");
                cableAnchor.transform.position = hit.point;
                tcable.CableAnchorPoint = cableAnchor.transform;
                tcable.CableTarget = transform;
            }
        }
    }

    private StringBuilder _stringBuilder;

    public void GetObjectInfo(StringBuilder sb)
    {
        sb.AppendFormat("Name: {0}", ActorName);
        sb.AppendLine();
        sb.AppendFormat("Position: {0}", transform.position.GetColoredText());
        sb.AppendLine();
        sb.AppendFormat("Rotation: {0}", transform.rotation.GetColoredText());
        sb.AppendLine();
    }

    public string GetObjectDisplayName()
    {
        return ActorName;
    }

    protected void InitializeActorHost()
    {
        if (_initialized)
            return;

        _initialized = true;

        Debug.Log("Spawning Actor");

        if (MasterControl.ActiveClientRole == ClientRole.Researcher)
        {
            GameObject sceneCamParent = GameObject.Find("SceneHighlightCams");
            if (sceneCamParent != null)
            {
                Debug.Log("Adding player orbit cam");
                var orbitCam = Util.InstantiateResource("OrbitCam");
                orbitCam.transform.SetParent(sceneCamParent.transform, false);
                var vcam = orbitCam.GetComponent<Cinemachine.CinemachineVirtualCamera>();
                vcam.transform.position = transform.position;
                vcam.Follow = transform;
                vcam.LookAt = transform;
                vcam.enabled = false;
            }
        }
    }

    public void SetAnimWalkSpeed(float speed)
    {
        _animWalkSpeed = speed;
    }
}