using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CollisionSoundEffect : MonoBehaviour
{
    //public NetworkedObjectManager NetworkedObjectManager;

    public AudioMaterial SoundMaterial;
    //This should be on for most collision sound effects, but is in play for things like the ground or other large items
    public bool CanPlay = true;
    public Rigidbody CollisionRigidbody;
    public AudioCollisionManager AudioCollisionManager;
    public bool FilterSpawnCollision = true;
    public float DefaultPitch = 1;
    public int PriorityOffset = 0;

    //[HideInInspector]
    public Bounds ObjectBounds;

    private bool _lock = false;
    private int _triggerLayerMask;
    private int _excludedLayerMask;
    private int _teleportLayerMask;
    private int _guardianLayerMask;
    private int _playerLayerMask;
    private Vector3 _cachedPosition;
    private Vector3 _cachedVelocity;
    private NetSyncGrabState _netSyncGrabState;
    private float _spawnTime = 0;
    private ObjectInfo _objInfo;
    private CustomXRInteractable _xrInteractable;
    private LoadableAssetManager LoadableAssetManager;
    


    private void Start()
    {
        if (AudioCollisionManager == null)
            AudioCollisionManager = AudioCollisionManager.GetDefault(gameObject);
        //if (NetworkedObjectManager == null)
        //    NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);

        _triggerLayerMask = LayerMask.GetMask("BeltItem");
        _excludedLayerMask = LayerMask.GetMask("SoundingStick");
        _teleportLayerMask = LayerMask.GetMask("ValidTeleport");
        _guardianLayerMask = LayerMask.GetMask("Guardian");
        _playerLayerMask = LayerMask.GetMask("Player");
        _netSyncGrabState = GetComponent<NetSyncGrabState>();
        _cachedPosition = transform.position;
        _cachedVelocity = Vector3.zero;
        _spawnTime = Time.time + 0.5f;
        _objInfo = GetComponentInParent<ObjectInfo>();//Should this just be a regular GetComponent?
        LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);

        Collider[] cols = GetComponentsInChildren<Collider>();
        if(CollisionRigidbody == null)
        {
            CollisionRigidbody = GetComponentInChildren<Rigidbody>();
        }
        LoadableAsset lA = null;
        if (_objInfo != null && LoadableAssetManager != null)
        {
            lA = LoadableAssetManager.FindAsset(_objInfo.AssetID);
        }

        if (lA != null)
        {
            if (lA.AudioProperties == null)
            {
                ObjectBounds = new Bounds(transform.position, Vector3.zero);
                foreach (var col in cols)
                {
                    ObjectBounds.Encapsulate(col.bounds);
                }
            }
            else
            {
                if (!lA.AudioProperties.UseSizeOverride)
                {
                    ObjectBounds = new Bounds(transform.position, Vector3.zero);
                    foreach (var col in cols)
                    {
                        ObjectBounds.Encapsulate(col.bounds);
                    }
                }
                else
                {
                    ObjectBounds = LoadableAssetManager.FindAsset(_objInfo.AssetID).AudioProperties.SizeOverride;
                }
            }
        }
        else
        {
            ObjectBounds = new Bounds(transform.position, Vector3.zero);
            foreach (var col in cols)
            {
                ObjectBounds.Encapsulate(col.bounds);
            }
        }
        

        //if (_objInfo != null)
        //{
        //    LoadableAssetManager loadableAssetManager = LoadableAssetManager.GetDefault(gameObject);
        //    if(_objInfo.AssetID != null)
        //        Addressables.LoadAssetAsync<AudioMaterial>($"AudioMaterial/{loadableAssetManager.FindAsset(_objInfo.AssetID).AudioProperties.AudioMaterial}").Completed += OnLoadDone;
        //}

        _xrInteractable = GetComponent<CustomXRInteractable>();
        if(_xrInteractable != null)
        {
            _xrInteractable.OwnerChanged += _xrInteractable_OwnerChanged; ;
        }
    }

    private void _xrInteractable_OwnerChanged(XRObjectController obj)
    {
        //Debug.Log("Collision Sfx: Caught Ownership change event!");
        if (obj != null)
        {
            FilterSpawnCollision = true;
            _spawnTime = Time.time + 0.5f;
        }
    }

    private void OnLoadDone(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<AudioMaterial> obj)
    {
        if(obj.Result != null)
            SoundMaterial = obj.Result;
    }

    public void Lock()
    {
        _lock = true;
    }

    public float GetVelocityMagnitude()
    {
        float vel = 0;
        if(CollisionRigidbody != null)
        {
            if (CollisionRigidbody.isKinematic)
            {
                return _cachedVelocity.magnitude;
            }
            else
            {
                return CollisionRigidbody.velocity.magnitude;
            }
        }
        return vel;
    }

    private void FixedUpdate()
    {
        _cachedVelocity = (transform.position - _cachedPosition) / Time.fixedDeltaTime;
        _cachedPosition = transform.position;
        _lock = false;
    }

    //Method to process a local collision
    private void OnCollisionEnter(Collision collision)
    {
        
        if (AudioCollisionManager == null)
            return;

        if (_lock)
            return;

        if (!CanPlay)
        {            
            return;
        }
        if(FilterSpawnCollision)
        {
            if(Time.time < _spawnTime)
            {
                return;
            }
            else
            {
                FilterSpawnCollision = false;
            }
        }
        //if(_xrInteractable != null)
        //{
        //    if(_xrInteractable.CurrentOwner.is)
        //}

        if (((1 << collision.gameObject.layer) & _excludedLayerMask) != 0)
        {
            return;
        }
        //if (((1 << collision.gameObject.layer) & _triggerLayerMask) != 0)
        //{
        //    return;
        //}
        if (((1 << collision.gameObject.layer) & _teleportLayerMask) != 0)
        {
            return;
        }

        if (((1 << collision.gameObject.layer) & _guardianLayerMask) != 0)
        {
            return;
        }

        //Added this to filter out player related collisions that sometimes happened (i.e. metal sounding collision if hitting the RA, distinct from Knock)
        if (((1 << collision.gameObject.layer) & _playerLayerMask) != 0)
        {
            return;
        }

        ContactPoint point = collision.GetContact(0);
        //Debug.Log($"On collision triggered, is kinematic? {CollisionRigidbody.isKinematic}");
        Vector3 velocity = Vector3.zero;
        if (CollisionRigidbody != null)
        {
            if (CollisionRigidbody.isKinematic)
            {
                //Debug.Log($"On Collision Enter: What is the collider position? {transform.position} and cached? {_cachedPosition}");
                velocity = _cachedVelocity;
            }
            else
            {
                velocity = collision.relativeVelocity;
            }
        }
        else
        {
            velocity = collision.relativeVelocity;
        }

        CollisionSoundEffect cseOther = collision.gameObject.GetComponent<CollisionSoundEffect>();
        if (cseOther != null && SoundMaterial != null && cseOther.SoundMaterial != null)
        {
            var priority = PriorityOffset + SoundMaterial.Priority;
            var otherPriority = cseOther.PriorityOffset + cseOther.SoundMaterial.Priority;

            //Priority comparator
            if (priority == otherPriority)
            {
                //Velocity comparator
                if (GetVelocityMagnitude() < cseOther.GetVelocityMagnitude())
                {
                    _lock = true;
                    return;
                }
                else
                {
                    cseOther.Lock();
                }
            }
            else if (priority < otherPriority)
            {
                _lock = true;
                return;
            }
            else
            {
                cseOther.Lock();
            }                
        }

        AudioCollisionManager.ProcessCollision(gameObject, collision.gameObject, point.point, point.normal, velocity);
        //Debug.Log($"Playing collision audio! {gameObject.name}");
        _lock = true;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    //Note: current assumption is that OnTriggerEnter is always received by the collider that is not a trigger
    //    //OnTriggerEnter is not called if both colliders are trigger colliders

    //    //if (CollisionRigidbody == null || !CollisionRigidbody.isKinematic || SoundMaterial == null || !CanPlay || _excludeFirstCollision)
    //    //{
    //    //    _excludeFirstCollision = false;
    //    //    return;
    //    //}

    //    if (((1 << other.gameObject.layer) & _triggerLayerMask) > 0)
    //        return;

    //    if (((1 << other.gameObject.layer) & _excludedLayerMask) > 0)
    //        return;
        
    //    if (((1 << other.gameObject.layer) & _teleportLayerMask) > 0)
    //        return;

    //    if (!other.TryGetComponent<CustomXRInteractable>(out var xrInteractable))
    //        return;

    //    if (xrInteractable.CurrentOwner == null)
    //        return;

    //    if (FilterSpawnCollision)
    //    {
    //        if (Time.time < _spawnTime)
    //        {
    //            return;
    //        }
    //        else
    //        {
    //            FilterSpawnCollision = false;
    //        }
    //    }

    //    //if (GetComponent<SoundingStick>())
    //    //{
    //    //    return;
    //    //}

    //    //if (other.TryGetComponent<NetworkedObject>(out var netObj))
    //    //{
    //    //    if (!netObj.HasAuthority)
    //    //        return;
    //    //}

    //    Vector3 pointClosestToObject = other.ClosestPoint(transform.position);
    //    Vector3 kinematicVelocity = _cachedVelocity;

    //    //AudioCollisionManager.ProcessCollision(gameObject, other.gameObject, pointClosestToObject, Vector3.Normalize(pointClosestToObject + transform.position), SoundMaterial.RelativeVelocityThreshold.Max * Vector3.Normalize(pointClosestToObject + transform.position));
    //    AudioCollisionManager.ProcessCollision(gameObject, other.gameObject, pointClosestToObject, Vector3.Normalize(pointClosestToObject + transform.position), kinematicVelocity);
    //}

    
    //Method to request a particular sound collision from the Audio Collision Manager (network receive a play request) i.e. "Cinder Block, Play this clip at this location"

    private void OnDrawGizmosSelected()
    {
        if(SoundMaterial == null)
        {
            return;
        }
        Gizmos.color = Color.magenta;
        Gizmos.DrawCube(SoundMaterial.ReferenceSizeBounds.center, (SoundMaterial.ReferenceSizeBounds.size));
        Gizmos.color = Color.white;
        Gizmos.DrawCube(ObjectBounds.center, ObjectBounds.size);
    }
}
