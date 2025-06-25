using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ChalkBehavior : MonoBehaviour, IInteractableObject
{
    //public TextTexture TTexture;
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;
    //public NetworkedObject NetObj;
    public LayerMask RaycastLayerMask;
    public float RaycastDistance = 0.4f;

    private int _sourcePlayer = 0;

    private float _updateTime = 0;
    private int _floorLayer;

    private RaycastHit[] _raycastHits;
    private LaserLineRenderer _laserRenderer;
    private XRInputManager.InputDeviceState _xrInputState;
    private CustomXRInteractor _interactor;
    private LaserLineRenderer.LaserRaycastCheck _laserCheckDelegate;

    private void Awake()
    {
        RaycastLayerMask = new LayerMask();
        RaycastLayerMask.value = LayerMask.GetMask("Floor", "Default");

        _raycastHits = new RaycastHit[10];

        _laserCheckDelegate = new LaserLineRenderer.LaserRaycastCheck(IsDNIAllowed);
    }

    private void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        //if (NetObj == null)
        //{
        //    NetObj = GetComponent<NetworkedObject>();
        //}

        if (TryGetComponent<LaserLineRenderer>(out _laserRenderer))
        {
            _laserRenderer.RaycastDistance = RaycastDistance;
            _laserRenderer.RaycastCheckFunction = _laserCheckDelegate;
        }

        _floorLayer = LayerMask.NameToLayer("Floor");
    }

    private void Update()
    {
        if (_xrInputState == null || _laserRenderer == null)
            return;

        if (_laserRenderer != null)
        {
            var activationLevel = _xrInputState.Trigger;
            _laserRenderer.ActivationLevel = activationLevel;
        }
     
    }

    private bool IsDNIAllowed(RaycastHit hit)
    {
        if (hit.collider == null || hit.collider.gameObject == null)
            return false;

        if (hit.collider.gameObject.layer != _floorLayer &&
            hit.collider.GetComponent<AllowDNI>() == null)
            return false;

        return true;
    }


    public void ActivateChalk()
    {
        var numHits = Physics.RaycastNonAlloc(transform.position, transform.forward, _raycastHits, RaycastDistance, RaycastLayerMask.value);
        float minDist = float.MaxValue;
        RaycastHit hit = new RaycastHit();

        if (numHits > 0)
        {
            Debug.Log("Chalk hit");

            for (int i = 0; i < numHits; i++)
            {
                if (!IsDNIAllowed(_raycastHits[i]))
                    continue;

                var dist = Vector3.Distance(transform.position, _raycastHits[i].point);
                if (dist > minDist)
                    continue;

                minDist = dist;
                hit = _raycastHits[i];
            }

            Vector3 dniPos = hit.point;
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, hit.normal);
            rot = rot * new Quaternion(0, 1, 0, 0);
            Vector3 localZCorrection = rot.eulerAngles;
            localZCorrection.z = 0;
            dniPos = hit.point + (hit.normal.normalized * 0.5f);
            //NetworkManager.SendSpawnObject("Prefabs/MineRescue/DNI_Obj_MP", System.Guid.NewGuid(), startPos, rot);
            var dniRot = Quaternion.Euler(localZCorrection);

            NetworkManager.SpawnObject("Prefabs/MineRescue/DNI_Obj_MP", System.Guid.NewGuid(), dniPos, dniRot, true);
            //NetworkManager.SendSpawnObject("Prefabs/MineRescue/DNI_Obj_MP", System.Guid.Parse("1e7080da-9abf-4f9d-87df-87cb89ed4ae3"), startPos, rot);
            NetworkManager.LogSessionEvent(new VRNLogEvent
            {
                EventType = VRNLogEventType.DateAndInitial,
                ObjectType = VRNLogObjectType.Chalk,
                ObjectName = "DNI",
                Position = dniPos.ToVRNVector3(),
                Rotation = dniRot.ToVRNQuaternion(),
                SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID
            });
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

    public void OnPickedUp(Transform interactor)
    {
        interactor.TryGetComponent<CustomXRInteractor>(out _interactor);
        if (_interactor != null)
            _xrInputState = _interactor.XRInputState;
    }

    public void OnDropped(Transform interactor)
    {
        _interactor = null;
        _xrInputState = null;
        
        if (_laserRenderer != null)
        {
            _laserRenderer.EnableLaser(false);
        }
    }

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {
        ActivateChalk();
    }

    public void OnDeactivated(Transform interactor)
    {

    }
}
