using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostBehavior : MonoBehaviour, IInteractableObject
{
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;
    public NetworkedObjectManager NetworkedObjectManager;

    public MeshRenderer PostMeshRenderer;
    public Material ValidColor;
    public Rigidbody PostRigidbody;
    public AudioSource InstallSource;
    public SoundCollection PostInstallCollection;
    public float PostRadius = 0.0762f;
    public float PostLength = 2.134f;

    private Material _postOrigMaterial;
    private bool _allowPostActivate = false;
    private Vector3 _hitPoint;
    private CustomXRInteractable _customXRInteractable;
    private Collider _col;
    private bool _canDetect = false;
    private float postOrigHeight;
    private NetworkedObject _networkedObject;
    private CapsuleCollider _capCollider;
    private int _layerMask;
    private Vector3 _initialScale = Vector3.one;
    private bool _installed = false;
    private NetworkedObjectManager.NetObjData _netObjData;
    private float _grabEnableTime = -1;

    private Collider[] _colliders = null;

    private VRNTextMessage _txtMessage = new VRNTextMessage();

    void Awake()
    {
        _colliders = new Collider[25];
    }

    // Start is called before the first frame update
    void Start()
    {
        _initialScale = transform.localScale;
        //_postOrigMaterial = PostMeshRenderer.material;
        _hitPoint = Vector3.zero;
        _customXRInteractable = GetComponent<CustomXRInteractable>();
        _col = GetComponent<Collider>();

        //postOrigHeight = GetComponent<CapsuleCollider>().height;
        postOrigHeight = GetComponentInChildren<BoxCollider>().size.y;
        _networkedObject = GetComponent<NetworkedObject>();
        _capCollider = GetComponentInChildren<CapsuleCollider>();
        if (PostMeshRenderer == null)
        {
            PostMeshRenderer = GetComponentInChildren<MeshRenderer>();
        }
        if (_networkedObject != null)
            _networkedObject.RegisterMessageHandler(OnNetObjMessage);

        _layerMask = LayerMask.GetMask("Floor");

        if (PostMeshRenderer != null)
        {
            _postOrigMaterial = PostMeshRenderer.sharedMaterial;
        }

        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        _netObjData = NetworkedObjectManager.GetObjectData(_networkedObject.uniqueID);

        NetworkedObjectManager.ObjectHeldStateChanged += OnHeldStateChanged;
    }

    private void OnHeldStateChanged(System.Guid obj)
    {
        if (_installed && _netObjData != null && _netObjData.HeldState != null && _netObjData.HeldState.ObjectHeld)
        {
            UninstallPost();

            var playerID = PlayerManager.LookupPlayerID(_netObjData.HeldState.HeldByClientID);
            LogPostEvent(VRNLogEventType.PostRemoved, playerID);
        }
    }

    void OnDestroy()
    {
        if (NetworkedObjectManager != null)
            NetworkedObjectManager.ObjectHeldStateChanged -= OnHeldStateChanged;
    }

    private void Update()
    {
        if (_grabEnableTime > 0 && Time.time > _grabEnableTime && _customXRInteractable != null)
        {
            _grabEnableTime = -1;
            _customXRInteractable.IsGrabbable = true;
        }

        if (!_canDetect)
        {
            return;
        }

        var postAngle = Mathf.Abs(Vector3.Dot(Vector3.up, transform.up));

        //Collider[] cols = Physics.OverlapCapsule(transform.TransformPoint(0, 0, 0), transform.TransformPoint(0, PostLength, 0), PostRadius, _layerMask);
        int numHits = Physics.OverlapCapsuleNonAlloc(transform.TransformPoint(0, 0, 0),
            transform.TransformPoint(0, PostLength, 0), PostRadius, _colliders, _layerMask);

        if (numHits > 0 && postAngle > 0.9397f)
        {
            SetValidColor();
            _allowPostActivate = true;
            //Debug.Log("Colliders found should allow to install!");
        }
        else
        {
            RestorePostColor();
            _allowPostActivate = false;
        }
    }

    private void OnNetObjMessage(string messageType, CodedInputStream reader)
    {
        if (_txtMessage == null)
            _txtMessage = new VRNTextMessage();

        _txtMessage.Message = "";

        if (messageType == "POSTSFX")
        {
            if (!InstallSource.isPlaying)
            {
                if (PostInstallCollection != null)
                {
                    InstallSource.clip = PostInstallCollection.Sounds[Random.Range(0, PostInstallCollection.Sounds.Length)];
                }
                Debug.Log($"Play post install: {gameObject.name}");
                InstallSource.Play();
            }

        }

        if (messageType == "INSTALLPOST")
        {
            //Debug.Log("Received install message!");
            if (_networkedObject.NetManager.IsServer)
            {
                //Debug.Log("Trying to install now.");

                reader.ReadMessage(_txtMessage);
                if (int.TryParse(_txtMessage.Message, out var playerID))
                {
                    Debug.Log($"Received INSTALLPOST from player ID {playerID}");
                    InstallPost(playerID);
                }
            }
        }
    }

    public void UninstallPost()
    {
        _installed = false;
        transform.localScale = _initialScale;

        if (TryGetComponent<NetSyncServerPhysicsInteractable>(out var netSync))
        {
            netSync.ServerSideKinematic = false;
            netSync.IgnorePositionSync = false;
        }
    }

    private bool RaycastInstallPost(Vector3 pos, Vector3 raycastDir)
    {
        RaycastHit hit;

        if (Physics.Raycast(pos, raycastDir, out hit, 20, _layerMask))
        {
            Debug.Log($"Raycast hit up!: {hit.point}, {hit.collider.name}");
            RaycastHit floorHit;

            if (Physics.Raycast(hit.point + new Vector3(0, -0.15f, 0), Vector3.down, out floorHit, 20, _layerMask))
            {
                Vector3 floorPos = floorHit.point + new Vector3(0, -0.15f, 0);
                Vector3 ceilingPos = hit.point + new Vector3(0, 0.15f, 0);
                float postHeight = Vector3.Distance(floorPos, ceilingPos);

                float scaleVal = (postHeight / postOrigHeight) * 1.1f;
                Vector3 scale = new Vector3(1, scaleVal, 1);

                transform.position = floorHit.point;
                transform.rotation = Quaternion.identity;
                transform.localScale = scale;

                PostRigidbody.isKinematic = true;
                _installed = true;
                RestorePostColor();

                return true;
            }
        }

        return false;
    }

    private void InstallPost(int playerID)
    {
        var pos = transform.TransformPoint(0, PostLength / 2.0f, 0);

        bool installed = false;
        installed = RaycastInstallPost(pos, Vector3.up);

        if (!installed)
            installed = RaycastInstallPost(pos, transform.up * -1);
        if (!installed)
            installed = RaycastInstallPost(pos, transform.up);

        if (!installed)
        {
            Debug.LogError("Post install: Raycasts failed to find install location");
            return;
        }


        PlayInstallSfx();

        if (TryGetComponent<NetSyncServerPhysicsInteractable>(out var netSync))
        {
            netSync.ServerSideKinematic = true;
            netSync.IgnorePositionSync = true;
        }

        if (_networkedObject.HasAuthority)
        {
            _networkedObject.SendMessage("POSTSFX", new VRNTextMessage());
        }

        LogPostEvent(VRNLogEventType.PostInstall, playerID);

        //if (RaycastInstallPost(pos, transform.up))
        //    return;
        //else if (RaycastInstallPost(pos, transform.up * -1))
        //    return;
        //else if (RaycastInstallPost(pos, Vector3.up))
        //    return;



        

    }

    private void LogPostEvent(VRNLogEventType eventType, int playerID = -1)
    {

        if (playerID < 0 && PlayerManager.CurrentPlayer != null)
        {
            playerID = PlayerManager.CurrentPlayer.PlayerID;
        }

        VRNLogEvent postEvent = new VRNLogEvent
        {
            EventType = eventType,
            Message = "",
            Position = transform.position.ToVRNVector3(),
            Rotation = transform.rotation.ToVRNQuaternion(),
            ObjectType = VRNLogObjectType.Unknown,
            ObjectName = "Post",
            SourcePlayerID = playerID,
            PositionMetadata = "",
        };

        NetworkManager.LogSessionEvent(postEvent);
    }

    private void SetValidColor()
    {
        if (_customXRInteractable == null)
            return;

        ObjectHighlightController.ShowHighlight(_customXRInteractable.gameObject, this, ObjectHighlightColor.ActivateHighlight);
        //if (PostMeshRenderer == null || _postOrigMaterial == null)
        //    return;

        //PostMeshRenderer.material = ValidColor;

    }
    private void RestorePostColor()
    {
        if (_customXRInteractable == null)
            return;

        ObjectHighlightController.ClearHighlight(_customXRInteractable.gameObject, this);
        //if (PostMeshRenderer == null || _postOrigMaterial == null)
        //    return;

        //PostMeshRenderer.material = _postOrigMaterial;
    }

    private void PlayInstallSfx()
    {
        if (!InstallSource.isPlaying)
        {
            int select = Random.Range(0, PostInstallCollection.Sounds.Length);
            InstallSource.clip = PostInstallCollection.Sounds[select];
            InstallSource.Play();
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 direction = transform.TransformDirection(Vector3.up) * 10;
        Gizmos.color = Color.red;
        //Gizmos.DrawRay(transform.position, direction);

        Gizmos.color = Color.blue;
        direction = transform.TransformDirection(Vector3.down) * 10;
        //Gizmos.DrawRay(transform.position, direction);

        Gizmos.color = Color.magenta;

        Gizmos.DrawLine(transform.TransformPoint(0, 0, 0), transform.TransformPoint(0, PostLength, 0));
        Gizmos.DrawWireSphere(transform.TransformPoint(0, 0, 0), PostRadius);
        Gizmos.DrawWireSphere(transform.TransformPoint(0, PostLength, 0), PostRadius);
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

    public void OnPickedUp(Transform interactor)
    {
        _canDetect = true;

        if (_installed)
        {
            UninstallPost();
            LogPostEvent(VRNLogEventType.PostRemoved);
        }
    }

    public void OnDropped(Transform interactor)
    {

        if (_customXRInteractable != null && _customXRInteractable.ActiveInteractors != null && _customXRInteractable.ActiveInteractors.Count > 0)
        {
            //still held by another interactor
            return;
        }

        RestorePostColor();

        _canDetect = false;

        //ClientInstallPost();

    }

    private bool ClientInstallPost()
    {
        if (!_allowPostActivate)
            return false;

        if (PlayerManager == null || PlayerManager.CurrentPlayer == null)
        {
            Debug.LogError($"PostBehavior: No current player found");
            return false;
        }

        if (_customXRInteractable != null)
        {
            _grabEnableTime = Time.time + 2.0f;
            _customXRInteractable.IsGrabbable = false;
        }

        if (_networkedObject != null && _networkedObject.HasAuthority)
            InstallPost(PlayerManager.CurrentPlayer.PlayerID);
        else
        {
            Debug.Log($"Post install called at {Time.frameCount}");

            var txtMessage = new VRNTextMessage();
            txtMessage.Message = PlayerManager.CurrentPlayer.PlayerID.ToString();
            _networkedObject.SendMessage("INSTALLPOST", txtMessage);
        }

        //LogPostEvent(VRNLogEventType.PostInstall);
        return true;

    }

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {
        if (!ClientInstallPost())
            return;

        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        if (NetworkManager.IsServer)
        {
            //cache position to prevent change in OnDrop
            pos = transform.position;
            rot = transform.rotation;
        }

        if (_customXRInteractable != null && _customXRInteractable.ActiveInteractors != null)
        {
            //foreach (var controller in _customXRInteractable.ActiveInteractors)
            for (int i = _customXRInteractable.ActiveInteractors.Count - 1; i >= 0; i--)
            {
                var controller = _customXRInteractable.ActiveInteractors[i];
                var interact = controller as CustomXRInteractor;
                if (interact != null)
                    interact.DropInteractable();
            }
        }

        if (NetworkManager.IsServer)
        {
            transform.position = pos;
            transform.rotation = rot;
        }
    }

    public void OnDeactivated(Transform interactor)
    {

    }
}
