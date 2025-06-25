using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HalfCurtainBehavior : MonoBehaviour, IInteractableObject, INetSync
{
    public bool PlayOnSpawn = true;
    public PlayerManager PlayerManager;
    
    public GameObject HalfCurtainSource;
    public HangingCurtain HangCurtain;
    public CustomXRInteractable InteractableObject;

    public float VentResistance = 20;
    public string EventLogMessage = "";
    
    //public GameObject DynamicCurtainObj;
    //public GameObject ValidPositionIndicatorObject;
    //public GameObject ValidPosObjPrefab;
    public string FullCurtainAddress = "Curtain_Single-PieceBrattice_MP_Scaled";
    public string RolledCurtainAddress = "Curtain_Folded_MP";
    //public float InteractableSize = 0.1f;//meters
    //public NetworkManager NetworkManager;
    //public PlayerManager PlayerManager;
    public AudioSource PullingAudioSfxSource;
    public AudioSource SpawningAudioSource;

    //private bool _canAttach = false;    
    private bool _grabbed = false;
    private CustomXRInteractable _xrInteract;
    private Vector3 _startPosition;
    
    //private NetSyncGrabState _netGrabState;
    private NetworkedObject _netObj;
    //private NetSyncCurtain _netCurtainHandler;
    //private GameObject _spawnedIndicator;

    private bool _destroyed = false;
    private bool _lastGrabState = false;
    private GameObject _target;
    private HangingCurtain.CableNode _nodeInHand;
    private MeshRenderer _hangCurtainMeshRenderer;
    private VRNHalfCurtainData _vrnHalfCurtainData;
    private Vector3 _hangPosition;

    private Rigidbody _interactableRB;
    private BoxCollider _interactableCol;

    private GameObject _ventPlaneObj;
    private VentResistancePlane _ventPlane;

    private void Awake()
    {
        _vrnHalfCurtainData = new VRNHalfCurtainData();
        _vrnHalfCurtainData.HangPosition = new VRNVector3();
        _hangPosition = Vector3.zero;
    }

    private void Start()
    {
        if (HalfCurtainSource == null || InteractableObject == null || HangCurtain == null)
        {
            Debug.LogError($"HalfCurtain missing required components");
            this.enabled = false;
            return;
        }

        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        _netObj = HalfCurtainSource.GetComponent<NetworkedObject>();
        _interactableCol = InteractableObject.GetComponent<BoxCollider>();
        _interactableRB = InteractableObject.GetComponent<Rigidbody>();

        _hangCurtainMeshRenderer = HangCurtain.GetComponent<MeshRenderer>();

        //_netCurtainHandler = GetComponentInParent<NetSyncCurtain>();
        //if (_netObj == null)
        //{
        //    Debug.LogError("Half curtain network components not found!");
        //}


        //HangCurtainMeshRenderer.enabled = false;
        HangCurtain.gameObject.SetActive(false);
        //_boxSize = _col.size;
        //_boxCenter = _col.center;
        InitiateCurtain();
        _xrInteract = gameObject.GetComponent<CustomXRInteractable>();
        _startPosition = HalfCurtainSource.transform.position;
       
        CheckForTarget();


        bool isEditorMode = false;
        if (ScenarioSaveLoad.IsScenarioEditor)
        {
            isEditorMode = true;
        }
        if (SpawningAudioSource != null && PlayOnSpawn && !isEditorMode)
        {
            SpawningAudioSource.Play();
        }
    }

    public void InitiateCurtain()
    {
        HangingCurtain.CableNode source = new HangingGeometry.CableNode();
        source.FixedPosition = true;
        source.Position = HalfCurtainSource.transform.position;
        source.ShowCableHanger = false;

        if (_ventPlaneObj == null || _ventPlane == null)
        {
            _ventPlaneObj = new GameObject("HalfCurtainVentPlane");
            _ventPlaneObj.transform.SetParent(HalfCurtainSource.transform, false);
            _ventPlaneObj.transform.localPosition = Vector3.zero;

            _ventPlane = _ventPlaneObj.AddComponent<VentResistancePlane>();
        }

        _ventPlane.PlaneWidth = 0.1f;
        _ventPlane.PlaneHeight = 10.0f;
        _ventPlane.AddedResistance = 0;

        _nodeInHand = new HangingGeometry.CableNode();
        _nodeInHand.FixedPosition = false;
        _nodeInHand.ShowCableHanger = false;
        _nodeInHand.Position = HalfCurtainSource.transform.position;

        HangCurtain.CableNodes.Add(source);
        HangCurtain.CableNodes.Add(_nodeInHand);


    }

    private void Update()
    {
        if (_netObj == null)
            return;

        CheckForTarget();
        
        if (_grabbed)
        {

            //_netCurtainHandler.Grabbed = true;
            //_netCurtainHandler.HangPosition = transform.position;

            UpdateDynamicCurtain();

            if (_netObj.HasAuthority && CheckIfCanAttach())
            {
                SpawnFullCurtain();
            }

        }
        else
        {
            HideDynamicCurtain();

            //if (_netCurtainHandler.Grabbed)
            //{
            //    UpdateDynamicCurtain(_netCurtainHandler.HangPosition);
            //}
            //else
            //{
            //    HideDynamicCurtain();
            //}
        }
    }

    private bool CheckIfCanAttach()
    {
        CheckForTarget();
        if (_target == null)
            return false;
        Vector3 pos;

        pos = InteractableObject.transform.position;
        
        if (_xrInteract != null)
        {
            var interactor = _xrInteract.CurrentOwner as CustomXRInteractor;
            if (interactor != null && interactor.IsTrackedController)
                pos = interactor.transform.position;
        }

        Vector3 targetPos = _target.transform.position;

        pos.y = 0;
        targetPos.y = 0;

        var dist = Vector3.Distance(pos, targetPos);
        if (dist < 0.3f)
            return true;
        
        return false;
    }

    private void UpdateDynamicCurtain()
    {
        if (_netObj == null || _netObj.HasAuthority)
        {
            _hangPosition = InteractableObject.transform.position;
        }

        HangCurtain.gameObject.SetActive(true);
        _nodeInHand.Position = _hangPosition;
        HangCurtain.UpdateMaterial();
        HangCurtain.RegenerateMesh();
        if (!PullingAudioSfxSource.isPlaying)
        {
            PullingAudioSfxSource.Play();
        }

        if (_ventPlaneObj != null && _ventPlane != null)
        {
            var dir = _hangPosition - HalfCurtainSource.transform.position;
            var width = dir.magnitude;

            if (float.IsNaN(width) || width == 0)
                return;

            dir.Normalize();

            var normal = Vector3.Cross(Vector3.up, dir);
            _ventPlaneObj.transform.rotation = Quaternion.LookRotation(normal, Vector3.up);
            _ventPlaneObj.transform.position = HalfCurtainSource.transform.position + dir * (0.5f * width);

            _ventPlane.PlaneWidth = width;
            _ventPlane.PlaneHeight = 3.0f;
            _ventPlane.AddedResistance = VentResistance;
        }
    }

    private void HideDynamicCurtain()
    {
        HangCurtain.gameObject.SetActive(false);
        _nodeInHand.Position = _startPosition;
        if (PullingAudioSfxSource.isPlaying)
        {
            PullingAudioSfxSource.Stop();
        }

        if (_ventPlane != null)
        {
            _ventPlane.AddedResistance = 0;
            _ventPlane.PlaneWidth = 0.1f;
            _ventPlane.PlaneHeight = 0.1f;
        }
    }

    private void CheckForTarget()
    {
        if(_target != null)
        {
            return;
        }
        var pos = InteractableObject.transform.position;

        Collider[] cols = Physics.OverlapSphere(pos, 0.5f);
        float distance = Mathf.Infinity;
        foreach (Collider col in cols)
        {
            CurtainReceiver cr = col.GetComponent<CurtainReceiver>();
            if (cr != null)
            {
                float newDist = Vector3.Distance(cr.transform.position, pos);
                if (newDist < distance)
                {
                    _target = cr.PairedReceiver.gameObject;
                    distance = newDist;
                }
            }
        }
    }


    private void DropCurtain()
    {
        if (_destroyed)
            return;

        Debug.Log($"Curtain: Destroying half hung curtain {_netObj.gameObject.name} - {_netObj.uniqueID}");
        _netObj.NetManager.DestroyObject(_netObj.uniqueID);
        _destroyed = true;

        Vector3 spawnPos = HalfCurtainSource.transform.position - new Vector3(0, 1, 0);
        spawnPos.y = 0.1f;
        RaycastHit hit;
        if (Physics.Raycast(HalfCurtainSource.transform.position, Vector3.down, out hit, 10, LayerMask.NameToLayer("Floor")))
        {
            //Debug.Log("Did a hit happen?");
            spawnPos = hit.point;
        }
        _netObj.NetManager.SpawnObject(RolledCurtainAddress, System.Guid.NewGuid(), spawnPos, Quaternion.identity, true);

        if (EventLogMessage == null)
            EventLogMessage = "";

        _netObj.NetManager.LogSessionEvent(new VRNLogEvent
        {
            EventType = VRNLogEventType.RolledCurtainDropped,
            ObjectType = VRNLogObjectType.Curtain,
            ObjectName = "Curtain Take Down",
            Message = EventLogMessage,
            Position = transform.position.ToVRNVector3(),
            Rotation = transform.rotation.ToVRNQuaternion(),
            SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID,
        });

    }

    private void SpawnFullCurtain()
    {
        Vector3 midpoint = (HalfCurtainSource.transform.position + _target.transform.position) / 2;
        Vector3 midpointYUp = midpoint;
        midpointYUp.y = midpointYUp.y + 1;
        Vector3 dir = Vector3.Cross((midpointYUp - midpoint), (_target.transform.position - midpoint)).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);
        Vector3 pos = midpoint;
        pos.y = 0;
        _netObj.NetManager.SpawnObject(FullCurtainAddress, System.Guid.NewGuid(), pos, rot, true);
        _netObj.NetManager.LogSessionEvent(new VRNLogEvent
        {
            EventType = VRNLogEventType.CurtainFullHang,
            ObjectType = VRNLogObjectType.Curtain,
            ObjectName = "Curtain Fully Hanged",
            Position = HalfCurtainSource.transform.position.ToVRNVector3(),
            Rotation = HalfCurtainSource.transform.rotation.ToVRNQuaternion(),
            SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID,
        });
        CurtainReceiver cr = _target.GetComponent<CurtainReceiver>();
        if (cr != null)
        {
            cr.DeactivateIndicator();
        }
        CustomXRInteractable xr = gameObject.GetComponent<CustomXRInteractable>();

        xr.ChangeOwnership(null);

        Debug.Log($"Curtain: Destroying half hung curtain {_netObj.gameObject.name} - {_netObj.uniqueID}");
        _netObj.NetManager.DestroyObject(_netObj.uniqueID);

    }

    private void ActivateCurtain()
    {
        if (_xrInteract.CurrentOwner == null && !_grabbed)
        {
            DropCurtain();
            return;
        }
      
    }

    private void OnDisable()
    {
        if (_target != null && _target.TryGetComponent<CurtainReceiver>(out var receiver))
        {
            if (receiver != null && receiver.gameObject != null)
                receiver.DeactivateIndicator();
        }
    }

    private void OnDestroy()
    {
      
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (_xrInteract != null && _xrInteract.CurrentOwner != null)
        {
            Vector3 pos = _xrInteract.CurrentOwner.transform.position;
            Vector3 dir = _xrInteract.CurrentOwner.transform.TransformDirection(Vector3.forward) * 5;
            Gizmos.DrawRay(pos, dir);
        }
    }

    public void OnPickedUp(Transform interactor)
    {
        if (!_netObj.HasAuthority)
            _netObj.RequestOwnership();

        //_col.size = new Vector3(InteractableSize, InteractableSize, InteractableSize);
        //_col.center = Vector3.zero;
        //HangCurtainMeshRenderer.enabled = true;
        _interactableCol.enabled = false;
        HangCurtain.gameObject.SetActive(true);
        _grabbed = true;
        //_netCurtainHandler.Grabbed = true;
        CurtainReceiver cr = _target.GetComponent<CurtainReceiver>();
        if (cr != null)
        {
            cr.ActivateIndicator();
        }

    }

    public void OnDropped(Transform interactor)
    {

        //_col.size = _boxSize;
        //_col.center = _boxCenter;
        _interactableCol.enabled = true;
        _interactableRB.isKinematic = true;
        InteractableObject.transform.localPosition = Vector3.zero;
        InteractableObject.transform.localRotation = Quaternion.identity;

        HangCurtain.gameObject.SetActive(false);
        _grabbed = false;
        //_netCurtainHandler.Grabbed = false;
        CurtainReceiver cr = _target.GetComponent<CurtainReceiver>();
        if (cr != null)
        {
            cr.DeactivateIndicator();
        }
    }

    public void OnJoystickPressed(Transform interactor, bool pressed)
    {
        
    }

    public void OnPrimaryButtonPressed(Transform interactor, bool pressed)
    {
        
    }

    public void OnSecondaryButtonPressed(Transform interactor, bool pressed)
    {
        
    }

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {
        ActivateCurtain();
    }

    public void OnDeactivated(Transform interactor)
    {
        
    }

    public bool NeedsUpdate()
    {
        return _grabbed || _lastGrabState != _grabbed;
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        _vrnHalfCurtainData.Grabbed = _grabbed;
        _vrnHalfCurtainData.HangPosition = InteractableObject.transform.position.ToVRNVector3();

        writer.WriteMessage(_vrnHalfCurtainData);

        _lastGrabState = _grabbed;
    }

    public void SyncObjState(CodedInputStream reader)
    {
        _vrnHalfCurtainData.Grabbed = false;
        _vrnHalfCurtainData.HangPosition.X = 0;
        _vrnHalfCurtainData.HangPosition.Y = 0;
        _vrnHalfCurtainData.HangPosition.Z = 0;

        reader.ReadMessage(_vrnHalfCurtainData);

        _grabbed = _vrnHalfCurtainData.Grabbed;
        _hangPosition = _vrnHalfCurtainData.HangPosition.ToVector3();
    }
}
