using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceMaskInteraction : MonoBehaviour, IInteractableObject
{
    public NetworkManager NetworkManager;

    //public Bounds CollisionCheckBounds = new Bounds(Vector3.zero, Vector3.one);

    private bool _activateScan = false;
    private bool _allowPlace = false;
    private NetworkedObject _netObj;
    private NPCPlaceBG4 _npcPlaceBG4;
    private ObjectHighlightController _gh;
    private Collider[] _overlapResults;
    private CustomXRInteractable _customXRInteractable;
    private int _layerMask = 0;
    private BoxCollider _boxCollider;
    private BodyBehavior _bodyBehavior;

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {
        AddBG4ToTarget();
    }

    public void OnDeactivated(Transform interactor)
    {
    }

    public void OnDropped(Transform interactor)
    {

        if (_customXRInteractable != null && _customXRInteractable.ActiveInteractors != null && _customXRInteractable.ActiveInteractors.Count > 0)
        {
            //still held by another interactor
            return;
        }

        _activateScan = false;
        Debug.Log("Deactivating scan");

        //AddBG4ToTarget();

        _gh.ClearHighlight(this);
    }

    private void AddBG4ToTarget()
    {
        if(_bodyBehavior != null)
        {
            if (!_bodyBehavior.BG4Active)
            {
                _activateScan = false;
                NetworkManager.DestroyObject(_netObj.uniqueID);

                NetworkManager.LogSessionEvent(VRNLogEventType.EquipmentAdded, null,
                    transform.position, transform.rotation,
                    "BG4", VRNLogObjectType.Bg4);

                if (_customXRInteractable != null)
                {
                    _customXRInteractable.DropFromAllInteractors();
                }
                _bodyBehavior.RequestEnableBG4();
            }
            return;
        }

        
        if (_npcPlaceBG4 == null || !_npcPlaceBG4.CanApplyBG4())
        {
            Debug.Log("FaceMaskInteraction: Couldn't apply bg4");
            return;
        }

        if (_customXRInteractable != null)
        {
            _customXRInteractable.DropFromAllInteractors();
        }

        Debug.Log($"Adding BG4 to {_npcPlaceBG4.name}");
        if (_npcPlaceBG4.RequestEnableBG4())
        {
            _activateScan = false;
            NetworkManager.DestroyObject(_netObj.uniqueID);

            NetworkManager.LogSessionEvent(VRNLogEventType.EquipmentAdded, null,
                transform.position, transform.rotation,
                "BG4", VRNLogObjectType.Bg4);
        }
        
    }

    public void OnJoystickPressed(Transform interactor, bool pressed)
    {
    }

    public void OnPickedUp(Transform interactor)
    {
        _activateScan = true;
        Debug.Log("Activating scan");
    }

    public void OnPrimaryButtonPressed(Transform interactor, bool pressed)
    {
    }

    public void OnSecondaryButtonPressed(Transform interactor, bool pressed)
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        _netObj = GetComponent<NetworkedObject>();
        NetworkManager = NetworkManager.GetDefault(gameObject);
        _gh = GetComponent<ObjectHighlightController>();
        _overlapResults = new Collider[100];
        _layerMask = LayerMask.GetMask("Default", "Player");

        TryGetComponent<BoxCollider>(out _boxCollider);
        TryGetComponent<CustomXRInteractable>(out _customXRInteractable);
    }

    // Update is called once per frame
    void Update()
    {
        if (_activateScan)
        {
            //Collider[] cols = Physics.OverlapSphere(transform.position, 1f);

            Vector3 center = Vector3.zero;
            Vector3 extents = Vector3.one;

            if (_boxCollider != null)
            {
                center = _boxCollider.center;
                extents = _boxCollider.size * 0.5f;
            }
            
            //int count = Physics.OverlapSphereNonAlloc(transform.position, 0.33f, _overlapResults, _layerMask, QueryTriggerInteraction.Collide);
            int count = Physics.OverlapBoxNonAlloc(transform.TransformPoint(center), 
                extents, _overlapResults, transform.rotation, _layerMask, QueryTriggerInteraction.Ignore);
            //_bg4 = null;
            bool found = false;

            //foreach (Collider col in _overlapResults)
            for (int i = 0; i < count; i++)
            {
                var col = _overlapResults[i];
                if(col == null)
                    continue;

                //NPCPlaceBG4 npcPlaceBG4 = col.GetComponent<NPCPlaceBG4>();
                //col.attachedRigidbody.TryGetComponent<NPCPlaceBG4>(out var npcPlaceBG4);
                var npcPlaceBG4 = col.GetComponentInParent<NPCPlaceBG4>();
                if(npcPlaceBG4 != null)
                {   
                    _npcPlaceBG4 = npcPlaceBG4;

                    if (_npcPlaceBG4.CanApplyBG4())
                        _gh.ShowHighlight(this, ObjectHighlightColor.ActivateHighlight);
                    else
                        _gh.ShowHighlight(this, ObjectHighlightColor.UnavailableHighlight);

                    //Debug.Log("Found Place BG4!");
                    found = true;
                    break;
                }

                var bodyBehavior = col.GetComponentInParent<BodyBehavior>();
                if(bodyBehavior != null)
                {
                    _bodyBehavior = bodyBehavior;
                    if (!_bodyBehavior.BG4Active)
                        _gh.ShowHighlight(this, ObjectHighlightColor.ActivateHighlight);
                    else
                        _gh.ShowHighlight(this, ObjectHighlightColor.UnavailableHighlight);
                    found = true;
                    break;
                }
            }

            //Clear cached NpcPlaceBG4 value if nothing is found in the overlap sphere, it means the player moved on without installing the BG4
            if (!found)
            {
                _npcPlaceBG4 = null;
                _bodyBehavior = null;
                _gh.ClearHighlight(this);
            }            
        }
    }

    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.matrix = transform.localToWorldMatrix;

    //    Gizmos.DrawCube(CollisionCheckBounds.center, CollisionCheckBounds.size);

    //    Gizmos.matrix = Matrix4x4.identity;
    //}

    //private void OnCollisionEnter(Collision collision)
    //{
    //    NPCPlaceBG4 bg4 = collision.gameObject.GetComponentInParent<NPCPlaceBG4>();
    //    if (bg4 != null)
    //    {
    //        _bg4 = bg4;
    //        _gh.GlowOn();
    //        Debug.Log("Collision place BG4!");
    //    }
    //}

    //private void OnCollisionExit(Collision collision)
    //{
    //    if(_bg4 == null)
    //    {
    //        return;
    //    }
    //    if(_bg4.gameObject == collision.gameObject)
    //    {
    //        _bg4 = null;
    //        _gh.GlowOff();
    //    }
    //}
}
