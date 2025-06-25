using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.VFX;

[RequireComponent(typeof(CustomXRInteractable))]
public class LaserPointerInteraction : MonoBehaviour, IInteractableObject, INetSync
{
    public NetworkManager NetworkManager;
    public NetworkedObjectManager NetworkedObjectManager;
    public TeleportManager TeleportManager;
    public LineRenderer LaserLineRenderer;
    public List<Color> LaserColors;
    public GameObject HitEffectPrefab;
    public AssetReference FlarePrefab;
    public AssetReference PhysicsOrbPrefab;

    public int StartingMode = 0;
    public bool AllowModeChange = true;

    public Vector3 LowGravity = Physics.gravity * 0.25f;
    public float LowGravityDuration = 1.0f;
    public bool OverrideLaserEnabled = false;

    public bool IsLaserEnabled
    {
        get
        {
            //if (LaserLineRenderer == null)
            //    return false;
            //else
            //    return LaserLineRenderer.enabled;
            return _laserEnabled;
        }
    }

    private CustomXRInteractable _xrInteractable;

    private int _laserMode = 0;
    private bool _laserEnabled = false;
    private float _laserActivationLevel = 0;

    private int _raycastMask;
    private CustomXRInteractor _interactor;
    private XRInputManager.InputDeviceState _xrInputState;
    private NetworkedObject _netObj;

    private GameObject _hitEffectObj;
    //private GameObject _flarePrefabObj;
    private VisualEffect _hitVFX;

    private float _maxSpawnRate = 100;
    private int _spawnRateID;
    private float _lastFlareLaunch = -100;
    private float _lastPhysicsOrbLaunch = -100;
    private MeshRenderer[] _renderers;
    private int _floorLayer;
    private Collider[] _colliders;

    private VRNLaserPointerState _vrnLaserState = new VRNLaserPointerState();

    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);
        if (TeleportManager == null)
            TeleportManager = TeleportManager.GetDefault(gameObject);

        _floorLayer = LayerMask.NameToLayer("Floor");

        _renderers = GetComponentsInChildren<MeshRenderer>();

        _raycastMask = LayerMask.GetMask("Default", "Floor", "Walls", "Roof", "StretcherItem");
        _xrInteractable = GetComponent<CustomXRInteractable>();
        _netObj = GetComponent<NetworkedObject>();

        if (HitEffectPrefab != null)
        {
            _hitEffectObj = Instantiate<GameObject>(HitEffectPrefab);
            if (_hitEffectObj.TryGetComponent<VisualEffect>(out _hitVFX))
            {
                _spawnRateID = Shader.PropertyToID("Spawn Rate");
                _maxSpawnRate = _hitVFX.GetFloat(_spawnRateID);
                _hitVFX.SetFloat(_spawnRateID, 0);
            }

            _hitEffectObj.SetActive(false);
        }

        //if (FlarePrefab != null)
        //{
        //    _flarePrefabObj = await Addressables.LoadAssetAsync<GameObject>(FlarePrefab).Task;
        //}


        if (LaserLineRenderer != null)
        {
            
        }

        ChangeLaserMode(StartingMode);

        //_xrInteractable.onActivate.AddListener(OnActivated);
        //_xrInteractable.onDeactivate.AddListener(OnDeactivated);
    }

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {
        //interactor.TryGetComponent<CustomXRInteractor>(out _interactor);
        //if (_interactor != null)
        //    _xrInputState = _interactor.XRInputState;

        //EnableLaser(true);
    }

    public void OnDeactivated(Transform interactor)
    {
        //_interactor = null;
        //_xrInputState = null;
        //EnableLaser(false);
        
    }

    private void EnableLaser(bool enabled)
    {
        if (LaserLineRenderer != null)
            LaserLineRenderer.enabled = enabled;

        _laserEnabled = enabled;
    }

    void Update()
    {
        UpdateLaserState();
        UpdateLaser();
    }

    void UpdateLaserState()
    {
        if (_netObj != null && !_netObj.HasAuthority)
            return;

        //if (_interactor == null || _interactor.XRInputState == null)
        //    return;

        if (OverrideLaserEnabled)
        {
            if (!_laserEnabled)
                EnableLaser(true);

            _laserActivationLevel = 1.0f;
            return;
        }

        //turn off laser if the trigger is released
        if (_interactor == null || _interactor.XRInputState == null || _xrInputState.Trigger < 0.05f)
        {
            if (_laserEnabled)
                EnableLaser(false);
            _laserActivationLevel = 0;
            return;
        }

        if (!_laserEnabled)
            EnableLaser(true);

        _laserActivationLevel = _xrInputState.Trigger;
    }

    void UpdateLaser()
    {
        if (LaserLineRenderer == null)
            return;

        var emitter = LaserLineRenderer.transform;

        if (Physics.Raycast(emitter.position, emitter.up, out var hit, 100, _raycastMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 localPos = emitter.InverseTransformPoint(hit.point);
            LaserLineRenderer.SetPosition(1, localPos);

            if (_laserActivationLevel > 0.5f)
            {
                float vfxActivation = (_laserActivationLevel - 0.5f) * 2.0f;
                UpdateVFX(vfxActivation, hit.point, hit.normal);

                if (_laserMode == 2)
                    TractorEffect(hit.rigidbody);
            }
            else
            {
                DisableVFX();
            }
        }
        else
        {
            LaserLineRenderer.SetPosition(1, new Vector3(0, 100, 0));
            DisableVFX();
        }

        if (_laserActivationLevel > 0.75f && _laserMode == 3)
            LaunchPrefab(FlarePrefab, ref _lastFlareLaunch, 1.0f);
        if (_laserActivationLevel > 0.75f && _laserMode == 4)
            LaunchPrefab(PhysicsOrbPrefab, ref _lastPhysicsOrbLaunch, 0.25f);
        if(_laserActivationLevel > 0.75f && _laserMode == 5)
        {
            //Collider[] cols = new Collider[100];
            if (_colliders == null)
                _colliders = new Collider[100];

            int count = Physics.OverlapSphereNonAlloc(transform.position, 5, _colliders);
            //foreach (Collider col in cols)

            var forward = transform.forward;

            for (int i = 0; i < count; i++)
            {
                var col = _colliders[i];

                if(col.attachedRigidbody != null /*&& col.attachedRigidbody.TryGetComponent<Renderer>(out var ren)*/)
                {
                    var dir = col.transform.position - transform.position;
                    dir.Normalize();

                    var angle = Vector3.Angle(forward, dir);
                    if (angle < 20.0f)
                    {
                        BoomEffect(col.attachedRigidbody);
                    }

                    //if (ren.isVisible)//figure this is easier for now than doing a bunch of cone math when what I really care about is if it's in view
                    //{
                    //    //Vector3 dir = Vector3.Normalize(transform.position + col.attachedRigidbody.position);
                    //    //col.attachedRigidbody.AddForce(dir * 5);
                    //    BoomEffect(col.attachedRigidbody);
                    //}
                }
            }
        }

        LaserLineRenderer.widthMultiplier = _laserActivationLevel;
    }

    void DisableVFX()
    {
        UpdateVFX(0, Vector3.zero, Vector3.zero);
    }

    void UpdateVFX(float activationLevel, Vector3 pos, Vector3 normal)
    {
        if (activationLevel <= 0 || _laserMode != 1)
        {
            _hitVFX.SetFloat(_spawnRateID, 0);
            return;
        }

        _hitEffectObj.SetActive(true);

        _hitVFX.SetFloat(_spawnRateID, activationLevel * _maxSpawnRate);

        if (TeleportManager != null && TeleportManager.ActiveTeleportTarget != null)
        {
            _hitVFX.SetVector3("GroundPlanePos", TeleportManager.ActiveTeleportTarget.position);
        }

        _hitEffectObj.transform.position = pos;
        _hitEffectObj.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);

    }

    void TractorEffect(Rigidbody target)
    {
        if (target == null)
            return;

        //if (_netObj != null && !_netObj.HasAuthority)
        //    return;
        if (!NetworkManager.IsServer)
            return;

        if (!target.TryGetComponent<CustomXRInteractable>(out var targetInteract))
            return;
        
        if (!targetInteract.IsGrabbable)
            return;
        

        if (target.TryGetComponent<NetworkedObject>(out var netObj))
        {
            if (!netObj.HasAuthority)
            {
                var objData = NetworkedObjectManager.GetObjectData(netObj.uniqueID);
                if (objData == null || objData.HeldState == null || objData.HeldState.ObjectHeld == false)
                    netObj.RequestOwnership();
            }
        }

        TemporaryGravityChange gravity = null;
        if (!target.TryGetComponent<TemporaryGravityChange>(out gravity))
        {
            gravity = target.gameObject.AddComponent<TemporaryGravityChange>();
        }

        gravity.SetTemporaryGravity(LowGravity, LowGravityDuration); 

        var dir = transform.position - target.position;
        dir += Vector3.up;
        dir.Normalize();

        target.isKinematic = false;
        target.velocity = dir * 2;        
    }

    void BoomEffect(Rigidbody target)
    {
        if (target == null)
            return;

        //if (_netObj != null && !_netObj.HasAuthority)
        //    return;
        if (!NetworkManager.IsServer)
            return;

        if (!target.TryGetComponent<CustomXRInteractable>(out var targetInteract))
            return;

        if (!targetInteract.IsGrabbable)
            return;

        if (target.gameObject == gameObject)
            return;


        if (target.TryGetComponent<NetworkedObject>(out var netObj))
        {
            if (!netObj.HasAuthority)
            {
                var objData = NetworkedObjectManager.GetObjectData(netObj.uniqueID);
                if (objData == null || objData.HeldState == null || objData.HeldState.ObjectHeld == false)
                    netObj.RequestOwnership();
            }
        }

        TemporaryGravityChange gravity = null;
        if (!target.TryGetComponent<TemporaryGravityChange>(out gravity))
        {
            gravity = target.gameObject.AddComponent<TemporaryGravityChange>();
        }

        gravity.SetTemporaryGravity(LowGravity, LowGravityDuration);

        var dir = target.position - transform.position;
        var distance = dir.magnitude;
        dir.Normalize();

        float t = Mathf.Clamp01(distance / 3.0f);
        var force = Mathf.Lerp(150, 5, t);

        target.isKinematic = false;
        target.AddForce(dir * force);
    }

    void LaunchPrefab(AssetReference prefab, ref float lastLaunch, float delay)
    {
        if (_netObj != null && !_netObj.HasAuthority)
            return;

        if (Time.time - lastLaunch < delay)
            return;

        lastLaunch = Time.time;

        if (NetworkedObjectManager == null || prefab == null)
            return;

        var dir = transform.forward;
        var pos = transform.position + dir * 0.5f;

        var rot = Quaternion.LookRotation(dir);

        _ = NetworkedObjectManager.SpawnObject(prefab.RuntimeKey.ToString(), System.Guid.NewGuid(), 
            pos, rot, spawnActivated:true);
        //var launchVelocity = dir * 20.0f;

        //await Task.Delay(1); //wait for object initialization

        //if (obj.TryGetComponent<IThrowableObject>(out var throwable))
        //{
        //    throwable.ThrowObject(transform, launchVelocity, Vector3.zero);
        //}
        //else
        //{
        //    var rb = obj.GetComponent<Rigidbody>();
        //    rb.useGravity = true;
        //    rb.isKinematic = false;
        //    rb.velocity = launchVelocity;
        //}
    }


    void ChangeLaserMode(int mode)
    {
        _laserMode = mode;
        if (LaserColors != null &&  mode < LaserColors.Count && LaserLineRenderer != null)
        {
            var color = LaserColors[mode];
            //LaserLineRenderer.startColor = color;
            //LaserLineRenderer.endColor = color;
            LaserLineRenderer.material.color = color;
            LaserLineRenderer.material.SetColor("_EmissiveColor", color);

            ChangeModelColor(color);
        }
    }

    void ChangeModelColor(Color color)
    {
        if (_renderers == null)
            return;

        foreach (var renderer in _renderers)
            renderer.material.color = color;
    }

    public void OnGripPressed(Transform interactor, bool pressed)
    {
        
    }

    public void OnJoystickPressed(Transform interactor, bool pressed)
    {
        if (!pressed || !AllowModeChange)
            return;

        if (_netObj != null && !_netObj.HasAuthority)
            return;

        Debug.Log($"LaserPointer: Joystick Pressed");

        if (LaserColors == null || LaserColors.Count <= 0)
            return;

        _laserMode++;
        if (_laserMode >= LaserColors.Count)
            _laserMode = 0;

        ChangeLaserMode(_laserMode);
    }

    public void OnPrimaryButtonPressed(Transform interactor, bool pressed)
    {
        
    }

    public void OnSecondaryButtonPressed(Transform interactor, bool pressed)
    {
        
    }

    public void OnTriggerPressed(Transform interactor, bool pressed)
    {
        
    }

    public bool NeedsUpdate()
    {
        if (_vrnLaserState.IsLaserEnabled != IsLaserEnabled ||
            _vrnLaserState.LaserMode != _laserMode ||
            _vrnLaserState.LaserActivationLevel != _laserActivationLevel)
            return true;

        return false;
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        _vrnLaserState.IsLaserEnabled = IsLaserEnabled;
        _vrnLaserState.LaserMode = _laserMode;
        _vrnLaserState.LaserActivationLevel = _laserActivationLevel;

        writer.WriteMessage(_vrnLaserState);
        //writer.WriteInt32(_laserMode);
        //writer.WriteBool(IsLaserEnabled);
        //writer.WriteFloat(_laserActivationLevel); 
    }

    public void SyncObjState(CodedInputStream reader)
    {
        try
        {
            _vrnLaserState.IsLaserEnabled = false;
            _vrnLaserState.LaserMode = 0;
            _vrnLaserState.LaserActivationLevel = 0;

            reader.ReadMessage(_vrnLaserState);
            //var mode = reader.ReadInt32();
            //var enabled = reader.ReadBool();
            //_laserActivationLevel = reader.ReadFloat();

            _laserActivationLevel = _vrnLaserState.LaserActivationLevel;

            if (_vrnLaserState.LaserMode != _laserMode)
                ChangeLaserMode(_vrnLaserState.LaserMode);

            EnableLaser(_vrnLaserState.IsLaserEnabled);
        }
        catch (Exception ex)
        {
            Debug.Log($"Error syncing LaserPointerInteraction {ex.Message}");
        }
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
        EnableLaser(false);
    }
}
