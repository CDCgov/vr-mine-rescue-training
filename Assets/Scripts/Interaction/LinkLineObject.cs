using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;
using Google.Protobuf;

public class LinkLineObject : MonoBehaviour, IInteractableObject, ISocketableObject
{
    public SocketManager SocketManager;

    //private TeleportController _tc;
    private Vector3 _initLocalPosition;
    private Quaternion _initLocalRotation;

    public bool IsDemonstration = false;
    public float DropLinkDistance = 0.75f;
    public Transform LinkLineBasePoint;


    public CustomXRInteractor xrInteractor;

    public AudioSource ClipAudioPlayer;

    public SoundingStickSounds ClickSounds;
    public SoundingStickSounds UnclickSounds;
    public SoundingStickSounds DropSounds;

    public bool Clipped = false;
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;

    private Rigidbody _linkLineRB;
    private CustomXRInteractable _linkLineInteractable;

    private List<CustomXRSocket> _availableSockets;
    private BoxCollider _boxCollider;
    private List<CustomXRSocket> _priorHighlightedSockets;
    private NetworkedObject _netObj;

    private CustomXRSocket _closestSocket = null;
    private float _dropTime;

    // Start is called before the first frame update
    void Start()
    {
        if (SocketManager == null)
            SocketManager = SocketManager.GetDefault(gameObject);
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        //_tc = GameObject.FindObjectOfType<TeleportController>();
        //LinkPlayerHandle = GameObject.FindObjectOfType<LinkLinePlayerHandler>();
        _initLocalPosition = transform.localPosition;
        _initLocalRotation = transform.localRotation;

        _linkLineRB = gameObject.GetComponent<Rigidbody>();
        _linkLineRB.centerOfMass = new Vector3(0.001f, 0.05f, 0);

        _linkLineInteractable = gameObject.GetComponent<CustomXRInteractable>();

        //_socketTarget = gameObject.GetComponent<NetSyncSocketTarget>();
        _netObj = gameObject.GetComponent<NetworkedObject>();

        _availableSockets = new List<CustomXRSocket>();
        _boxCollider = GetComponent<BoxCollider>();
        _priorHighlightedSockets = new List<CustomXRSocket>();

    }


    void ClearHighlightedSockets()
    {

        foreach (var socket in _availableSockets)
        {
            socket.DestroyIndicator();
        }
        _availableSockets.Clear();
        _closestSocket = null;
    }

    private void HighlightAvailableSockets()
    {
        _availableSockets.Clear();
        //CustomXRSocket[] _allSockets = GameObject.FindObjectsOfType<CustomXRSocket>();


        //int linkLineLayer = LayerMask.NameToLayer("LinkLine");
        //foreach (CustomXRSocket socket in _allSockets)
        //{

        //    if (socket.SocketedInteractable == this._linkLineInteractable)
        //    {
        //        SocketManager.RequestSocketVacate(socket.SocketID);
        //        //SocketManager.UnsocketObject(this._netObj.uniqueID);
        //    }
        //    //if (sock.gameObject.layer == linkLineLayer && sock.tag == gameObject.tag && sock.HeldObject == null)
        //    //{
        //    //    if (SocketManager.IsSocketOccupied(sock.SocketID))
        //    //        continue;

        //    //    Debug.Log($"Spawning indicator on {sock.gameObject.name}");
        //    //    sock.SpawnIndicator();
        //    //    _availableSockets.Add(sock);
        //    //}
        //}
        //StartCoroutine(DelayedHighlight(_allSockets));

        int linkLineLayer = LayerMask.NameToLayer("LinkLine");

        foreach (var data in SocketManager.GetAllSocketData())
        {

            if (data.Socket == null || 
                data.Socket.gameObject.layer != linkLineLayer || 
                data.Socket.tag != gameObject.tag)
                continue;

            data.Socket.SpawnIndicator();
            _availableSockets.Add(data.Socket);
        }
    }

    //IEnumerator DelayedHighlight(CustomXRSocket[] allSockets)
    //{
    //    yield return new WaitForSeconds(0.2f);
    //    int linkLineLayer = LayerMask.NameToLayer("LinkLine");
    //    foreach (CustomXRSocket sock in allSockets)
    //    {
    //        if (sock.gameObject.layer == linkLineLayer && sock.tag == gameObject.tag && sock.SocketedInteractable == null)
    //        {
    //            //if (SocketManager.IsSocketOccupied(sock.SocketID))
    //            //    continue;

    //            Debug.Log($"Spawning indicator on {sock.gameObject.name}");
    //            sock.SpawnIndicator();
    //            _availableSockets.Add(sock);
    //        }
    //    }
    //}

    void HighlightClosestSocket()
    {
        _closestSocket = null;
        if (_availableSockets.Count <= 0)
            return;

        float minDist = float.MaxValue;

        foreach (var socket in _availableSockets)
        {
            //if (SocketManager.IsSocketOccupied(socket.SocketID))
            //{
            //    socket.DestroyIndicator();
            //    _availableSockets.Remove(socket);
            //    continue;
            //}

            socket.ChangeIndicatorColor(false);
            var dist = Vector3.Distance(socket.transform.position, transform.position);
            if (dist < minDist && dist < 0.11f)
            {
                minDist = dist;
                _closestSocket = socket;
            }
        }

        if (_closestSocket != null)
            _closestSocket.ChangeIndicatorColor(true);
    }

    private void SetPlayerLinkLineStatus(bool onLinkLine)
    {
        Clipped = onLinkLine;

        if (PlayerManager == null || PlayerManager.CurrentPlayer == null)
            return;

        PlayerManager.CurrentPlayer.OnLinkLine = onLinkLine;
    }

    private bool IsPlayerOnLinkLine
    {
        get
        {
            if (PlayerManager == null || PlayerManager.CurrentPlayer == null)
                return false;
            else
                return PlayerManager.CurrentPlayer.OnLinkLine;
        }
    }

    void Update()
    {
        if (!_netObj.HasAuthority)
            return;

        HighlightClosestSocket();

        if (Vector3.Distance(transform.position, LinkLineBasePoint.position) > (DropLinkDistance * 1.5f))
        {
            var socket = _linkLineInteractable.CurrentOwner as CustomXRSocket;
            if (socket != null)
            {
                SocketManager.RequestSocketVacate(socket.SocketID);
            }
        }

        if (_linkLineInteractable.CurrentOwner == null && Time.time - _dropTime > 3.0f)
        {
            if (Vector3.Distance(transform.localPosition, _initLocalPosition) > 0.5f)
            {
                transform.localPosition = _initLocalPosition;
                transform.localRotation = _initLocalRotation;
            }
        }
    }


    // Update is called once per frame
    //void LateUpdate()
    //{

    //    if (_netObj == null)
    //    {
    //        Debug.LogError("No net object on link line");
    //        return;
    //    }
    //    if (!_netObj.HasAuthority)
    //        return;


    //    //float dropElapsed = Time.unscaledTime - _linkLineDropTime;

    //    //if (Vector3.Distance(transform.position, LinkLineBasePoint.position) > (DropLinkDistance * 1.5f))
    //    //{
    //    //    var socket = _linkLineInteractable.CurrentOwner as CustomXRSocket;
    //    //    if (socket != null && dropElapsed > 2.0f)
    //    //    {
    //    //        _linkLineDropTime = Time.unscaledTime;
    //    //        SocketManager.RequestSocketVacate(socket.SocketID);
    //    //    }

    //    //    //ClearHighlightedSockets();

    //    //}



    //    if (_linkLineInteractable.CurrentOwner == null && _notDropped)
    //    {
    //        //if (_priorHighlightedSockets.Count > 0 && dropElapsed > 2.0f)
    //        //{
    //        //    var netObj = LinkLineInteract.GetComponent<NetworkedObject>();
    //        //    if (netObj != null)
    //        //        SocketManager.RequestSocketAssignment(_priorHighlightedSockets[0].SocketID, netObj.uniqueID);
    //        //}
    //        if (Time.time > _dropTimer)
    //        {
    //            if (_netObj.HasAuthority)
    //            {
    //                NetworkManager.LogSessionEvent(new VRNLogEvent
    //                {
    //                    EventType = VRNLogEventType.LinkLineDetach,
    //                    ObjectType = VRNLogObjectType.LinkLineCarabiner,
    //                    ObjectName = $"{gameObject.name}",
    //                    Position = transform.position.ToVRNVector3(),
    //                    Rotation = transform.rotation.ToVRNQuaternion(),
    //                    SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID,
    //                });
    //            }

    //            //LinkLineInteract.ChangeOwnership(null);

    //            transform.localPosition = _initLocalPosition;
    //            transform.localRotation = _initLocalRotation;
    //            _linkLineInteractable.InteractableRigidbody.isKinematic = true;
    //            //LinkLineInteract.onSelectExit.Invoke();

    //            //ClipAudioPlayer.clip = DropSfx;
    //            PlayDropSound();

    //            SetPlayerLinkLineStatus(false);

    //            //foreach (CustomXRSocket sock in _availableSockets)
    //            //{
    //            //    sock.DestroyIndicator();
    //            //}
    //            //_availableSockets.Clear();
    //            _notDropped = false;
    //            //_socketTarget.TargetToSync = "";
    //            _socket = null;


    //        }
    //    }

    //    HighlightClosestSocket();


    //}

    public void ReturnToStart()
    {
        transform.localPosition = _initLocalPosition;
        transform.localRotation = _initLocalRotation;
    }

    private void OnCarabinerPickedUp()
    {
        if (_netObj == null || !_netObj.HasAuthority)
            return;

        if (IsPlayerOnLinkLine)
            return;

        //SetPlayerLinkLineStatus(false);
        HighlightAvailableSockets();
    }

    private void OnCarabinerDropped()
    {
        //refresh closest socket
        HighlightClosestSocket();

        if (!IsPlayerOnLinkLine && _netObj != null && _netObj.HasAuthority && _closestSocket != null)
        {
            Debug.Log($"LinkLineObj: Requesting socket {_closestSocket.name}");
            SocketManager.RequestSocketAssignment(_closestSocket.SocketID, _netObj.uniqueID);
        }
        else
        {
            Debug.Log($"LinkLineObj: Carabiner dropped but no nearby socket");
        }

        _dropTime = Time.time;

        ClearHighlightedSockets();
    }

    private void OnConnectedToBelt()
    {
        Debug.Log($"LinkLineObject: Connected to belt");

        PlayClickSound();
        SetPlayerLinkLineStatus(true);


        if (_netObj.HasAuthority)
        {
            NetworkManager.LogSessionEvent(new VRNLogEvent
            {
                EventType = VRNLogEventType.LinkLineAttach,
                ObjectType = VRNLogObjectType.LinkLineCarabiner,
                ObjectName = $"{gameObject.name}",
                Position = transform.position.ToVRNVector3(),
                Rotation = transform.rotation.ToVRNQuaternion(),
                SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID,
            });
        }

    }

    private void OnDisconnectedFromBelt()
    {
        Debug.Log($"LinkLineObject: Disconnected from belt");
        PlayUnclickSound();

        //if (!_netObj.HasAuthority)
        //    return;

        NetworkManager.LogSessionEvent(new VRNLogEvent
        {
            EventType = VRNLogEventType.LinkLineDetach,
            ObjectType = VRNLogObjectType.LinkLineCarabiner,
            ObjectName = $"{gameObject.name}",
            Position = transform.position.ToVRNVector3(),
            Rotation = transform.rotation.ToVRNQuaternion(),
            SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID,
        });

        transform.localPosition = _initLocalPosition;
        transform.localRotation = _initLocalRotation;
        _linkLineInteractable.InteractableRigidbody.isKinematic = true;


        SetPlayerLinkLineStatus(false);
    }


    private void PlayClickSound()
    {
        int index = Random.Range(0, ClickSounds.Sounds.Length);
        ClipAudioPlayer.clip = ClickSounds.Sounds[index];
        ClipAudioPlayer.Play();
    }

    private void PlayUnclickSound()
    {
        int index = Random.Range(0, UnclickSounds.Sounds.Length);
        ClipAudioPlayer.clip = UnclickSounds.Sounds[index];
        ClipAudioPlayer.Play();
    }

    private void PlayDropSound()
    {
        int index = Random.Range(0, DropSounds.Sounds.Length);
        ClipAudioPlayer.clip = DropSounds.Sounds[index];
        ClipAudioPlayer.Play();
    }


    //public void OnLinkLineActivated(CustomXRInteractor xrInteractor)
    //{
    //    if (!_netObj.HasAuthority || _linkLineInteractable == null || _linkLineInteractable.CurrentOwner == null)
    //        return;

    //    Debug.Log($"LinkLine activated by {_linkLineInteractable.CurrentOwner.name}");

    //    _notDropped = true;
    //    //LinkLineRB.isKinematic = true;
    //    //foreach (CustomXRSocket availSock in _availableSockets)
    //    //{
    //    //    availSock.DestroyIndicator();
    //    //}
    //    //_availableSockets.Clear();

    //    if (_linkLineInteractable.CurrentOwner.IsTrackedController)
    //    {
    //        //activated by controller
    //        OnCarabinerPickedUp();
    //    }
    //    else
    //    {
    //        //activated by belt socket
    //        OnConnectedToBelt();
    //    }

    //    _lastOwner = _linkLineInteractable.CurrentOwner;

    //}

    //public void OnLinkLineDeactivated(CustomXRInteractor xrInteractor)
    //{
    //    Debug.Log($"LinkLine Deactivated");
    //    //Debug.Log("Link line deactivated called");
    //    _dropTimer = Time.time + _dropDelay;

    //    //LinkLineRB.isKinematic = true;
    //    if (!_netObj.HasAuthority)
    //    {
    //        return;
    //    }


    //    if (_lastOwner != null && _closestSocket != null)
    //    {
    //        var netObj = _linkLineInteractable.GetComponent<NetworkedObject>();
    //        SocketManager.RequestSocketAssignment(_closestSocket.SocketID, netObj.uniqueID);
    //        _closestSocket = null;
    //    }
    //    else
    //    {
    //        SetPlayerLinkLineStatus(false);
    //    }
    //    ClearHighlightedSockets();


    //    if (_lastOwner == null || (_lastOwner as CustomXRSocket) == null)
    //        return;


    //    PlayUnclickSound();

    //}

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
        Debug.Log($"LinkLineObject: Picked up by {interactor.name}");
        var xrInteractor = interactor.GetComponent<CustomXRInteractor>();
        if (xrInteractor == null)
            return;
        //OnLinkLineActivated(xrInteractor);

        OnCarabinerPickedUp();
    }

    public void OnDropped(Transform interactor)
    {
        Debug.Log($"LinkLineObject: Dropped by {interactor.name}");
        var xrInteractor = interactor.GetComponent<CustomXRInteractor>();
        if (xrInteractor == null)
            return;
        //OnLinkLineDeactivated(xrInteractor);

        OnCarabinerDropped();
    }

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {

    }

    public void OnDeactivated(Transform interactor)
    {

    }

    public void OnSocketed(CustomXRSocket socket)
    {
        Debug.Log($"Link line {gameObject.name} socketed");
        //OnLinkLineActivated(socket);

        if (_netObj == null || _netObj.HasAuthority)
            OnConnectedToBelt();
    }

    public void OnRemovedFromSocket(CustomXRSocket socket)
    {
        Debug.Log($"Link line {gameObject.name} removed from socket");
        //OnLinkLineDeactivated(socket);

        if (_netObj == null || _netObj.HasAuthority)
            OnDisconnectedFromBelt();
    }
}
