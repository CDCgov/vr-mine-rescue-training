using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SoundingStick : MonoBehaviour, IInteractableObject
{
    /// <summary>
    /// Minimum distance away from the last sounding stick log event for a new event to be logged
    /// </summary>
    public const float SoundingStickMinLogDistance = 0.5f;
    private const float SoundPlaybackDelay = 0.08f;

    [FormerlySerializedAs("asSoundStickPlayer")]
    public AudioSource SoundingStickAudioSource;
    //public SoundingStickSounds DefaultSounds;

    public Transform RaycastTransform;
    public Bounds CollisionCheckZone;

    //public AudioClip StandardSoundStick;
    //public AudioClip WoodSoundStick;
    //public AudioClip HollowSoundStick;
    //public AudioClip MetalSoundStick;
    //public AudioClip RockSoundStick;
    //public AudioClip PlasticSoundStick;
    //public AudioClip CrumblingSoundStick;
    //public AudioClip[] GoodRoof;
    //public AudioClip[] BadRoof;
    public Collider PickedUpCollider;

    public AudioCollisionManager AudioCollisionManager;

    //public NetSyncGrabState _grabState;

    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;
    public NetworkedObjectManager NetworkedObjectManager;

    private CustomXRInteractable _interactable;

    private NetworkedObject _netObj = null;
    private NetworkedObjectManager.NetObjData _netObjData = null;
    private Vector3 _lastLogPosition = Vector3.zero;
    private float _nextSoundPlaybackMin; //time at which next collision sound is allowed
    private RaycastHit[] _raycastHits;
    private Collider[] _colliderHits;
    private int _numHits = 0;
    
    private int _layerMask;
    private int _excludedLayerMask;

    private CollisionSoundEffect _cse;
    private float _cooldownTime = 0;
    private float _knockCooldownTime = 0;
    private bool _localHeld = false;
    private bool _lock = false;
    private Vector3 _cachedVelocity;
    private Vector3 _cachedPosition;

    private void Awake()
    {
        _raycastHits = new RaycastHit[10];
        _colliderHits = new Collider[20];
    }

    private void Start()
    {
        _layerMask = LayerMask.GetMask("Default", "Floor", "RoofBolts");
        _interactable = GetComponentInParent<CustomXRInteractable>();
        //TryGetComponent<Collider>(out PickedUpCollider);
        if (ScenarioSaveLoad.IsScenarioEditor)
        {
            return;
        }
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);
        if (AudioCollisionManager == null)
            AudioCollisionManager = AudioCollisionManager.GetDefault(gameObject);

        //NetworkedObjectManager.ObjectHeldStateChanged += OnHeldStateChanged;

        _netObj = GetComponentInParent<NetworkedObject>();

        //yield return new WaitForEndOfFrame();

        if (_netObj != null)
        {
            _netObjData = NetworkedObjectManager.GetObjectData(_netObj.uniqueID);

            if (_netObjData == null)
            {
                Debug.LogError($"Couldn't get NetworkedObjectData for {name} {_netObj.uniqueID}");
            }
        }

        _excludedLayerMask = LayerMask.GetMask("SoundingStick", "BeltItem", "ValidTeleport", "Guardian");

        _cse = GetComponent<CollisionSoundEffect>();
        if (_cse == null)
            Debug.LogError($"Sounding stick {gameObject.name} missing CollisionSoundEffect component");
    }

    private void OnDestroy()
    {
        //if (NetworkedObjectManager != null)
        //    NetworkedObjectManager.ObjectHeldStateChanged -= OnHeldStateChanged;
    }

    private void OnHeldStateChanged(System.Guid obj)
    {
        if (_netObj == null || _netObj.uniqueID != obj)
            return;

    }

    private void FixedUpdate()
    {
        _cachedVelocity = (transform.position - _cachedPosition) / Time.fixedDeltaTime;
        _cachedPosition = transform.position;
        _lock = false;
    }

    //private void Update()
    //{
    //    if (_netObj == null || _netObjData == null)
    //        return;

    //    if (!_netObj.HasAuthority && PickedUpCollider != null && _netObjData.HeldState != null)
    //    {
    //        if (PickedUpCollider.enabled != _netObjData.HeldState.ObjectHeld)
    //            PickedUpCollider.enabled = _netObjData.HeldState.ObjectHeld;
    //    }
    //}


    private void PlayCollisionSound(SoundingType sType, Vector3 pt)
    {
        //asSoundStickPlayer.clip = sType.MaterialSounds.Sounds[Random.Range(0, sType.MaterialSounds.Sounds.Length - 1)];
        //asSoundStickPlayer.Play();
        if (sType == null || sType.MaterialSounds == null)
            return;

        if (Time.time < _nextSoundPlaybackMin)
            return;

        var clip = sType.MaterialSounds.Sounds[Random.Range(0, sType.MaterialSounds.Sounds.Length - 1)];
        SoundingStickAudioSource.PlayOneShot(clip);
        Debug.Log($"Sounding successful in Play Collision Sound!");
        _nextSoundPlaybackMin = Time.time + SoundPlaybackDelay;
        if (_interactable != null)
        {
            if (_interactable.CurrentOwner != null)
            {
                var interactor = _interactable.CurrentOwner as CustomXRInteractor;
                if (interactor != null && interactor.IsTrackedController)
                {
                    interactor.HapticShake(1);
                }
            }
        }
    }

    private void LogRoofCheck(Vector3 collisionPt, SoundingType soundingType)
    {
        if (_netObj == null || !_netObj.HasAuthority)
            return;

        string roofType = "";
        switch (soundingType.SoundMaterial)
        {
            case SoundType.GoodRoof:
                roofType = "Good Roof";
                break;
            case SoundType.BadRoof:
                roofType = "Bad Roof";
                break;
            default:
                //don't log non-roof checks
                return;

            //case SoundType.Metal:
            //    roofType = "Metal";
            //    break;
            //case SoundType.Wood:
            //    roofType = "Wood";
            //    break;
        }

        if (Vector3.Distance(_lastLogPosition, transform.position) > SoundingStickMinLogDistance)
        {
            NetworkManager.LogSessionEvent(new VRNLogEvent
            {
                EventType = VRNLogEventType.RoofCheck,
                ObjectType = VRNLogObjectType.SoundingStick,
                ObjectName = "Sounding Stick",
                Position = collisionPt.ToVRNVector3(),
                Rotation = transform.rotation.ToVRNQuaternion(),
                PositionMetadata = roofType,
                SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID,
            });

            _lastLogPosition = transform.position;
        }
    }

    private void PerformKnock(Knock knock, Vector3 collisionPt)
    {
        if (_netObj == null || !_netObj.HasAuthority)
            return;

        var knockResponse = knock.SoundingStickKnock();

        if (Time.time > _knockCooldownTime || knockResponse != null)
        {
            knock.LogKnockEvent(knock.KnockName, knockResponse, collisionPt);
            _knockCooldownTime = Time.time + 10.0f;

            //if (knockResponse == null)
            //    knockResponse = "No response";
            //else
            //    knockResponse = $"Knock Response: {knockResponse}";

            //NetworkManager.LogSessionEvent(new VRNLogEvent
            //{
            //    EventType = VRNLogEventType.DoorKnock,
            //    ObjectName = knock.KnockName,
            //    Message = knockResponse,
            //    Position = collisionPt.ToVRNVector3(),
            //    Rotation = transform.rotation.ToVRNQuaternion(),
            //    SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID
            //}); 
        }
    }

    //private void OnTriggerEnter(Collider collision)
    //{
    //    if (collision.TryGetComponent<SoundingType>(out var soundingType))
    //    {
    //        SoundRoof(collision);
    //    }
    //    else
    //    {
    //        if (_netSyncGrabState != null)
    //        {
    //            if (!_netSyncGrabState.Grabbed)
    //            {
    //                return;
    //            }
    //            Vector3 collisionPt = collision.ClosestPoint(transform.position);
    //            AudioCollisionManager.ProcessCollision(gameObject, collision.gameObject, collisionPt, Vector3.Normalize(collisionPt + transform.position), Vector3.one * 10);
    //        }
    //        else
    //        {
    //            Vector3 collisionPt = collision.ClosestPoint(transform.position);
    //            AudioCollisionManager.ProcessCollision(gameObject, collision.gameObject, collisionPt, Vector3.Normalize(collisionPt + transform.position), Vector3.one * 10);
    //        }
    //    }
    //}

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Sounding stick on collision enter!");
        //SoundRoof(collision.collider);
        if (collision == null) return;
        if (Time.time < _cooldownTime)
            return;

        if (SoundRoof(collision))
            return;

        if (((1 << collision.gameObject.layer) & _excludedLayerMask) != 0)
            return;

        if (_cse == null)
            return;

        Vector3 collisionPt = collision.contacts[0].point;
        if (collisionPt == null) return;
        CollisionSoundEffect otherCSE = collision.gameObject.GetComponent<CollisionSoundEffect>();

        //always play other's sound effect for sounding stick if possible
        AudioCollisionManager.ProcessCollision(collision.gameObject, gameObject, collisionPt, Vector3.Normalize(collisionPt + transform.position), _cachedVelocity);
        _cooldownTime = Time.time + 0.1f;
        _lock = true;
        if (otherCSE != null)
            otherCSE.Lock();

        //if (otherCSE != null)
        //{
        //    AudioCollisionManager.ProcessCollision(collision.gameObject, gameObject, collisionPt, Vector3.Normalize(collisionPt + transform.position), _cachedVelocity);
        //    _cooldownTime = Time.time + 0.1f;
        //    _lock = true;
        //}
        //else
        //{
        //    AudioCollisionManager.ProcessCollision(gameObject, collision.gameObject, collisionPt, Vector3.Normalize(collisionPt + transform.position), _cachedVelocity);
        //    _cooldownTime = Time.time + 0.1f;
        //    otherCSE.Lock();
        //    //Debug.Log($"Should have played sound against {collision.gameObject.name}");
        //}


        //else
        //{
        //    AudioCollisionManager.ProcessCollision(gameObject, collision.gameObject, collisionPt, Vector3.Normalize(collisionPt + transform.position), _cachedVelocity);
        //    _cooldownTime = Time.time + 0.1f;
        //}

    }

    private bool SoundRoof(Collision collision)
    {
        SoundingType soundingType = null;
        int priority = int.MinValue;
        Knock knock = null;

        var collisionPt = collision.contacts[0].point;

        if (collision.gameObject.TryGetComponent<Knock>(out knock))
        {
            PerformKnock(knock, collisionPt);
            return false; //play default collision audio
        }

        if (!collision.gameObject.TryGetComponent<SoundingType>(out var _))
        {
            //if the initial object doesn't have a sounding type, use default collision audio
            return false;
        }

        _numHits = Physics.OverlapBoxNonAlloc(transform.TransformPoint(CollisionCheckZone.center), CollisionCheckZone.extents, _colliderHits, transform.rotation, _layerMask);

        for (int i = 0; i < _numHits; i++)
        {
            if (_colliderHits[i].TryGetComponent<SoundingType>(out var colSoundingType))
            {
                var newPriority = colSoundingType.Priority;
                if (colSoundingType.SoundMaterial == SoundType.Metal)
                {
                    //boost roof bolt priority as a workaround for now
                    newPriority += 20;
                }

                if (soundingType == null || newPriority > priority)
                {
                    soundingType = colSoundingType;
                    priority = newPriority;
                }
            }

            //if (knock == null)
            //{
            //    _colliderHits[i].TryGetComponent<Knock>(out knock);
            //}

        }

        //if (knock != null)
        //{
        //    PerformKnock(knock, collisionPt);
        //    return false; //play default collision audio
        //}

        if (soundingType != null)
        {
            PlayCollisionSound(soundingType, collisionPt);
            LogRoofCheck(collisionPt, soundingType);
            Debug.Log($"Sounding successful in collision!");
            return true;
        }

        return false; //no knock or roof check

    }

    //private void SoundRoof(Collision collision)
    //{
    //    var objState = NetworkedObjectManager.GetObjectData(_netObj.uniqueID);
    //    if (objState != null)
    //    {
    //        //Debug.Log("grab not null");
    //        if (objState.HeldState == null || !objState.HeldState.ObjectHeld)
    //        {
    //            //Debug.Log(!_grabState.Grabbed);
    //            return;
    //        }
    //    }


    //    SoundingType sType = null;
    //    Knock knock = null;
    //    Vector3 collisionPt = Vector3.zero;
    //    bool raycastHit = false;

    //    if (RaycastTransform != null)
    //    {
    //        Debug.DrawLine(RaycastTransform.position, RaycastTransform.position + RaycastTransform.up * 0.33f, Color.magenta, 1.0f);
    //        //RaycastHit hit;
    //        float minDist = float.MaxValue;

    //        int numHits = Physics.RaycastNonAlloc(RaycastTransform.position, RaycastTransform.up, _raycastHits, 0.33f, _layerMask);
    //        if (numHits > 0)
    //        {
    //            for (int i = 0; i < numHits; i++)
    //            {
    //                //var hit = _raycastHits[i];
    //                //Debug.Log($"Sounding Stick: Raycast hit {hit.collider.gameObject.name}");

    //                var hitSoundType = _raycastHits[i].collider.gameObject.GetComponent<SoundingType>();
    //                if (hitSoundType == null)
    //                    continue;

    //                var dist = Vector3.Distance(_raycastHits[i].point, transform.position);
    //                if (dist > minDist)
    //                    continue;

    //                minDist = dist;

    //                //sType = _raycastHits[i].collider.gameObject.GetComponent<SoundingType>();
    //                sType = hitSoundType;
    //                knock = _raycastHits[i].collider.gameObject.GetComponent<Knock>();

    //                raycastHit = true;
    //                collisionPt = _raycastHits[i].point;

    //                Debug.Log($"SoundingStick ray hit {_raycastHits[i].collider.gameObject.name}");
    //            }
    //        }
    //    }

    //    if (!raycastHit)
    //    {
    //        sType = collision.gameObject.GetComponent<SoundingType>();
    //        knock = collision.gameObject.GetComponent<Knock>();
    //        //collisionPt = collision.ClosestPoint(transform.position);
    //        collisionPt = collision.contacts[0].point;
    //        Debug.Log($"SoundingStick hit {collision.gameObject.name}");
    //    }

    //    if (sType == null)
    //    {   
    //        AudioCollisionManager.ProcessCollision(gameObject, collision.gameObject, collisionPt, Vector3.Normalize(collisionPt + transform.position), Vector3.one * 10);
    //        _cooldownTime = Time.time + 0.1f;
    //        return;
    //    }

    //    PlayCollisionSound(sType, collisionPt);
    //    _cooldownTime = Time.time + 0.1f;

    //    if (knock != null)
    //        PerformKnock(knock, collisionPt);
    //    else
    //        PerformRoofCheck(collisionPt, sType);

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
        if (!interactor.TryGetComponent<CustomXRInteractor>(out var xrInteractor))
            return;

        if (!xrInteractor.IsTrackedController)
            return;

        Debug.Log($"Sounding stick picked up by {interactor.name}");

        _localHeld = true;

        //if (PickedUpCollider != null)
        //    PickedUpCollider.enabled = true;

    }

    public void OnDropped(Transform interactor)
    {
        Debug.Log($"Sounding stick dropped: interactor count: {_interactable.ActiveInteractors.Count}");

        //if (PickedUpCollider != null)
        //    PickedUpCollider.enabled = false;

        _localHeld = false;
    }

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {

    }

    public void OnDeactivated(Transform interactor)
    {

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        _numHits = Physics.OverlapBoxNonAlloc(transform.TransformPoint(CollisionCheckZone.center), CollisionCheckZone.extents, _colliderHits, transform.rotation, _layerMask);
        for (int i = 0; i < _numHits; i++)
        {
            if (_colliderHits[i].TryGetComponent<SoundingType>(out var _))
            {
                Gizmos.color = Color.red;
                break;
            }    
        }


        var oldMat = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(CollisionCheckZone.center, CollisionCheckZone.size);

        Gizmos.matrix = oldMat;
    }
}
