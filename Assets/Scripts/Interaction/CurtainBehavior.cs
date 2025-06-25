using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CurtainBehavior : MonoBehaviour, IInteractableObject
{
    public float RoofboltDetectionRadius = 10f;
    public GameObject indicatorSphere;
    public GameObject Root;
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;
    public AudioSource CurtainTearDown;
    public string HalfHungCurtainAddress = "Curtain_HalfHung_MP";
    public bool PlayTearDownAudioOnStart = true;
    public Transform RaycastOrigin;

    private bool _canAttach = false;
    private bool _destroyed = false;

    CurtainReceiver _currentReceiver;

    CustomXRInteractable _xrInteractable;
    BoxCollider _boxCollider;
    List<CurtainReceiver> _validReceiversInRange;
    NetworkedObject _netObj;
    bool _canDetect = false;
    //bool _triggerEnterFlag = false;

    private int _layerMask;

    System.Guid guid;
    // Start is called before the first frame update
    void Start()
    {
        _layerMask = LayerMask.GetMask("RoofBolts");
        _boxCollider = GetComponentInChildren<BoxCollider>();

        _validReceiversInRange = new List<CurtainReceiver>();

        if (NetworkManager == null)
        {
            NetworkManager = NetworkManager.GetDefault(gameObject);
        }

        if (PlayerManager == null)
        {
            PlayerManager = PlayerManager.GetDefault(gameObject);
        }

        _xrInteractable = gameObject.GetComponent<CustomXRInteractable>();

        if (CurtainTearDown == null)
        {
            CurtainTearDown = GetComponent<AudioSource>();
        }
        bool isEditorMode = false;
        if (ScenarioSaveLoad.IsScenarioEditor)
        {
            isEditorMode = true;
        }         
        if (PlayTearDownAudioOnStart && Time.timeSinceLevelLoad > 2 && !isEditorMode)
        {
            CurtainTearDown.Play();
        }
        if (_netObj == null)
        {
            _netObj = GetComponent<NetworkedObject>();
        }        
    }

    private void HighlightValidReceivers()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, RoofboltDetectionRadius);
        List<CurtainReceiver> recievers = new List<CurtainReceiver>();
        foreach (Collider col in colliders)
        {
            CurtainReceiver cr = col.GetComponent<CurtainReceiver>();
            if (cr != null)
            {
                recievers.Add(cr);
                cr.ActivateIndicator();
            }
            else
            {
                continue;
            }
        }
        foreach (CurtainReceiver oldReceiver in _validReceiversInRange)
        {
            if (!recievers.Contains(oldReceiver))
            {
                oldReceiver.RestoreStartingColor();
                oldReceiver.DeactivateIndicator();
            }
        }
        _validReceiversInRange.Clear();

        _validReceiversInRange = recievers;
    }

    private void Update()
    {
        if (!_canDetect)
            return;

        HighlightValidReceivers();

        CurtainReceiver receiver = null;

        if (_boxCollider != null)
        {
            var z_extent = _boxCollider.size.z / 2.0f;
            var p1 = _boxCollider.center + new Vector3(0, 0, z_extent);
            var p2 = _boxCollider.center + new Vector3(0, 0, -z_extent);

            p1 = _boxCollider.transform.TransformPoint(p1);
            p2 = _boxCollider.transform.TransformPoint(p2);

            Debug.DrawLine(p1, p1 + Vector3.up, Color.cyan);
            Debug.DrawLine(p2, p2 + Vector3.up, Color.magenta);

            receiver = SearchForRoofbolt(p1, Vector3.up);
            if (receiver == null)
                receiver = SearchForRoofbolt(p2, Vector3.up);
        }
        else
        {
            receiver = SearchForRoofbolt(RaycastOrigin.position, Vector3.up);
            Debug.DrawLine(RaycastOrigin.position, RaycastOrigin.position + Vector3.up, Color.green);
        }

        if (receiver == null && _xrInteractable != null && _xrInteractable.CurrentOwner != null)
        {
            var rayOrigin = _xrInteractable.CurrentOwner.transform.position;
            receiver = SearchForRoofbolt(rayOrigin, Vector3.up);
            Debug.DrawLine(rayOrigin, rayOrigin + Vector3.up, Color.red);
        }

        //if (receiver == null)
        //{
        //    Vector3 direction = RaycastOrigin.TransformDirection(Vector3.forward);
        //    Vector3 direction2 = RaycastOrigin.TransformDirection(Vector3.back);
        //    receiver = SearchForRoofbolt(RaycastOrigin.position, direction);
        //    if (receiver == null)
        //        receiver = SearchForRoofbolt(RaycastOrigin.position, direction2);
        //}

        if (receiver != null)
            HighlightReceiver(receiver);
        else
            ClearReceiverHighlight();

    }

    CurtainReceiver SearchForRoofbolt(Vector3 rayOrigin, Vector3 dir)
    {
        //if (Physics.Raycast(rayOrigin, dir, out RaycastHit hit, 50, _layerMask))
        if (Physics.SphereCast(rayOrigin, 0.15f, dir, out RaycastHit hit, 50, _layerMask))
        {
            CurtainReceiver receiver = hit.collider.GetComponent<CurtainReceiver>();
            if (receiver != null)
            {
                return receiver;
            }
        }
        return null;
    }

    void HighlightReceiver(CurtainReceiver receiver)
    {
        ClearReceiverHighlight();

        _currentReceiver = receiver;
        receiver.SetValidColor();
        _canAttach = true;

    }

    void ClearReceiverHighlight()
    {
        _canAttach = false;

        if (_currentReceiver != null)
        {
            _currentReceiver.RestoreStartingColor();
            _currentReceiver = null;
        }
    }

    void ClearReceiverOrbs()
    {
        if (_validReceiversInRange == null || _validReceiversInRange.Count <= 0)
            return;

        foreach (CurtainReceiver cr in _validReceiversInRange)
        {
            cr.RestoreStartingColor();
            cr.DeactivateIndicator();
        }
        _validReceiversInRange.Clear();
    }


    public void HangCurtain()
    {
        ClearReceiverOrbs();

        if (_destroyed || !_canAttach || _netObj == null)
            return;

        Vector3 eul = new Vector3(0, _currentReceiver.transform.eulerAngles.y, 0);
        Quaternion rot = Quaternion.Euler(eul);
        guid = System.Guid.NewGuid();
        NetworkManager.SpawnObject(HalfHungCurtainAddress, guid, _currentReceiver.transform.position, rot, true);

        Debug.Log($"Curtain: Spawning half hung curtain {guid.ToString()} at {_currentReceiver.transform.position.ToString()}");
        NetworkManager.LogSessionEvent(new VRNLogEvent
        {
            EventType = VRNLogEventType.CurtainHalfHang,
            ObjectType = VRNLogObjectType.Curtain,
            ObjectName = "Curtain Half Hang",
            Position = transform.position.ToVRNVector3(),
            Rotation = transform.rotation.ToVRNQuaternion(),
            SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID,
        });

        NetworkManager.DestroyObject(_netObj.uniqueID);
        _destroyed = true;
    }

    void Drop()
    {
        if (_xrInteractable != null && _xrInteractable.CurrentOwner != null)
        {
            _xrInteractable.ChangeOwnership(null);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 direction = RaycastOrigin.TransformDirection(Vector3.forward) * 3;
        Gizmos.DrawRay(RaycastOrigin.position, direction);
        Gizmos.color = Color.blue;
        direction = RaycastOrigin.TransformDirection(Vector3.back) * 3;
        Gizmos.DrawRay(RaycastOrigin.position, direction);
    }

    private void OnDestroy()
    {
        ClearReceiverOrbs();
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
        if (_xrInteractable != null && _xrInteractable.CurrentOwner != null)
        {
            if (_xrInteractable.CurrentOwner.GetComponent<CustomXRSocket>() != null)
                return;
        }

        _canDetect = true;
    }

    public void OnDropped(Transform interactor)
    {
        ClearReceiverOrbs();
        _canDetect = false;

        if (_canAttach)
        {
            HangCurtain();
            return;
        }
    }

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {
        
    }

    public void OnDeactivated(Transform interactor)
    {

    }
}
