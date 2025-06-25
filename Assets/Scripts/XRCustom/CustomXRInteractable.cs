using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Flags]
public enum InteractableType : uint
{
    None = 0,
    FireExtinguisher    = 0b1,
    CareVent            = 0b10,
    Blanket             = 0b100,
    FirstAidBag         = 0b1000,
    BG4                 = 0b10000,
    Curtain             = 0b100000,

    All                 = 0xFFFFFFFF
}

public class CustomXRInteractable : MonoBehaviour
{
    //public UnityEvent onActivate;
    //public UnityEvent onDeactivate;
    //public UnityEvent onSelectEnter;
    //public UnityEvent onSelectExit;

    public XRObjectController CurrentOwner;
    public Rigidbody InteractableRigidbody;
    public List<XRObjectController> ActiveInteractors;

    //public Transform SnapPoint;
    //public Transform InteractorTransform;

    public InteractableType InteractableType;
    public bool IsGrabbable = false;
    public bool GrabWithoutOwnerChange = false;
    public bool KeepOriginalParent = true;
    //public bool ReturnToOriginalPosition = false;
    //public bool IsKinematic = false;
    public bool ThrowOnDetach = true;
    //public bool IgnoreSnapPosition = false;//used for large items
    
    //public bool SpawnedWithPlayer = false;
    //public bool SelectReady = false;

    //public bool ActivatableItem = false;
    [UnityEngine.Serialization.FormerlySerializedAs("IsLargeItem")]
    public bool SnapToController = false;
    public bool RotateToPalm = false;//used for Radio and Gas Meter at the moment
    public bool IsMapBoard = false;
    public bool RequireGrabToActivate = false;
    //public bool GrabAlternateActivate = false;
    //public bool Lock = false;
    public Vector3 PalmRotationEulers;

    public float GrabSelectionWeight = 1;
    public float ScaleHeldObject = 1;

    //public System.Guid Address;
    public SocketManager SocketManager;

    public bool HasEmissiveTexture = false;
    public bool IgnoreSelectHighlight = false;
    public bool SelectHighlightOverride = false;
    public Material[] glowMaterials;
    public Renderer ModelRenderer;
    public int MaterialIndexToGlow = 0;
    public GameObject GlowSurrogate;

    public event Action<XRObjectController> OwnerChanged;

    private Transform _originalParent;
    private bool _destroyed = false;
    private NetworkedObject _netObj;
    //private bool _priorSelectReady = false;


    private int _selectHighlightRequests = 0; //number of requests for selection highlight this frame
    private bool _highlightActive = false;
    private ObjectHighlightController _gh;

    protected List<IInteractableObject> _interactInterfaces;

    private void Awake()
    {
        _interactInterfaces = new List<IInteractableObject>();
        _netObj = GetComponent<NetworkedObject>();
        ActiveInteractors = new List<XRObjectController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _originalParent = transform.parent;
        //_startingPosition = transform.localPosition;
        //_startingRotation = transform.localRotation;
        if(InteractableRigidbody == null)
        {
            InteractableRigidbody = gameObject.GetComponentInChildren<Rigidbody>();
        }
        //NetworkedObject netObj = GetComponent<NetworkedObject>();
        //if(netObj != null)
        //{
        //    //Address = netObj.uniqueID;
        //}

        if(SocketManager == null)
            SocketManager = SocketManager.GetDefault(gameObject);

        _gh = GetComponent<ObjectHighlightController>();

        GetComponentsInChildren<IInteractableObject>(_interactInterfaces);

        ShowSelectHighlight(false);
    }

    private void OnDestroy()
    {
        _destroyed = true;

        ChangeOwnership(null);
        //if (CurrentOwner != null)
        //{
        //    CustomXRSocket customXRSocket = CurrentOwner.GetComponent<CustomXRSocket>();
        //    //NetworkedObject networkedObject = GetComponent<NetworkedObject>();
        //    if (customXRSocket != null && _netObj != null)
        //    {
        //        if (_netObj.HasAuthority)
        //            customXRSocket.RequestRemoveSocketedItem();
        //    }
        //}
    }

    private void OnDisable()
    {
        ShowSelectHighlight(false);
    }

    public void ShowSelectHighlight()
    {
        _selectHighlightRequests++;
    }

    private void ShowSelectHighlight(bool show)
    {
        if (IgnoreSelectHighlight)
            return;

        //if (HasEmissiveTexture)
        //{
        //    //ModelRenderer.materials[MaterialIndexToGlow].SetTexture("_EmissionMap", null);
        //    //ModelRenderer.materials[MaterialIndexToGlow].SetColor("_EmissiveColor", new Color(0, 0.1f, 0, (1 / 255)));
        //    GlowSurrogate.SetActive(show);
        //    _highlightActive = show;
        //    return;
        //}
        //else if (ModelRenderer != null)
        //{
        //    Color selectColor;
        //    if (show)
        //        selectColor = new Color(0, 0.1f, 0, (1 / 255));
        //    else
        //        selectColor = new Color(0, 0, 0, 0);
        //    _highlightActive = show;
        //    ModelRenderer.materials[MaterialIndexToGlow].SetColor("_EmissiveColor", selectColor);
        //    return;
        //}

        //if (_gh == null || SelectHighlightOverride)
        //{
        //    return;
        //}
        //if (show)
        //{
        //    _gh.GlowOn();
        //}
        //else
        //{
        //    _gh.GlowOff();
        //}


        if (show)
        {
            var highlightColor = DetermineHighlightColor();
            ObjectHighlightController.ShowHighlight(gameObject, this, highlightColor);
        }
        else
        {
            ObjectHighlightController.ClearHighlight(gameObject, this);
        }

        _highlightActive = show;
    }

    private ObjectHighlightColor DetermineHighlightColor()
    {
        if (IsGrabbable)
            return ObjectHighlightColor.SelectHighlight;

        if (_interactInterfaces == null || _interactInterfaces.Count <= 0)
            return ObjectHighlightColor.HighlightOff;

        int highlightEnumIndex = (int)ObjectHighlightColor.HighlightOff;
        foreach (var interact in _interactInterfaces)
        {
            var canActivate = interact.CanActivate;
            int highlight = (int)ObjectHighlightColor.HighlightOff;
            switch (canActivate)
            {
                case ActivationState.Ready:
                    highlight = (int)ObjectHighlightColor.ActivateHighlight;
                    break;

                case ActivationState.Unavailable:
                    highlight = (int)ObjectHighlightColor.UnavailableHighlight;
                    break;

                case ActivationState.Error:
                    highlight = (int)ObjectHighlightColor.ErrorHighlight;
                    break;
            }
            
            if (highlight > highlightEnumIndex)
                highlightEnumIndex = highlight;
        }

        return (ObjectHighlightColor)highlightEnumIndex;
        //return ObjectHighlightColor.SelectHighlight;
    }

    private void LateUpdate()
    {
        if (_selectHighlightRequests > 0 && !_highlightActive)
            ShowSelectHighlight(true);
        else if (_selectHighlightRequests <= 0 && _highlightActive)
            ShowSelectHighlight(false);

        
        //check if we lose network authority & are controlled locally
        if (CurrentOwner != null && _netObj != null && !_netObj.HasAuthority)
        {
            ChangeOwnership(null);
        }

        _selectHighlightRequests = 0;
    }

    public void ChangeOwnership(XRObjectController newOwner)
    {
        if (CurrentOwner == newOwner)
            return;

        if (CurrentOwner != null)
        {
            CurrentOwner.LostOwnership(this);
            CurrentOwner = null;
        }


        var netObj = GetComponent<NetworkedObject>();
        if (!_destroyed && netObj != null && newOwner != null)
        {
            if (netObj.HasAuthority)
                transform.SetParent(newOwner.transform);
            else
                ResetParent();
        }
        
        //string curOwnName = "null";
        //string newOwnName = "null";
        //if(CurrentOwner != null)
        //{
        //    curOwnName = CurrentOwner.name;
        //}
        //if(newOwner != null)
        //{
        //    newOwnName = newOwner.name;
        //}
        
        CurrentOwner = newOwner;
        if (CurrentOwner != null)
            CurrentOwner.GainedOwnership(this);
        //if (newOwner != null && triggerEvent)
        //{
        //    onSelectEnter.Invoke();
        //}

        if (newOwner == null)
            ResetParent();

        OwnerChanged?.Invoke(newOwner);
    }

    public void ResetParent()
    {
        if (_destroyed)
            return;

        if (ScaleHeldObject != 1)
        {
            transform.localScale = Vector3.one;
        }

        if (KeepOriginalParent)
            transform.SetParent(_originalParent);
        else
            transform.SetParent(null);
    }

    public void DropFromAllInteractors()
    {
        if (ActiveInteractors == null || ActiveInteractors.Count <= 0)
            return;
        
        for (int i = ActiveInteractors.Count - 1; i >= 0; i--)
        {
            var controller = ActiveInteractors[i];
            var interact = controller as CustomXRInteractor;
            if (interact != null)
                interact.DropInteractable();
        }
        
    }
   
}
