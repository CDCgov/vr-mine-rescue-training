using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
public class PencilSnapBehavior : MonoBehaviour
{
    public bool UseProximity = false;
    public Transform SnapPointTransform
    {
        set 
        {
            _snapPointTransform = value;

            if (_snapPointTransform != null)
                _snapPointTransform.TryGetComponent<BoxCollider>(out _snapPointCollider);
            if (_snapPointCollider == null && _snapPointTransform != null && _snapPointTransform.parent != null)
                _snapPointTransform.parent.TryGetComponent<BoxCollider>(out _snapPointCollider);
        }
    }
    public Transform PencilTransform;
    public Transform HandTransform;

    public CustomXRInteractor XRInteractor;
    public ControllerBehaviors ControllerBehavior;
    
    private Transform _startingParent;
    private bool _isInHand = true;
    private float _distanceScale = 1.0f;

    private Transform _snapPointTransform = null;
    private BoxCollider _snapPointCollider = null;
    private float _mapPencilActivationDist = 0.25f;

    private void Start()
    {
        var systemManager = SystemManager.GetDefault();
        if (systemManager != null && systemManager.SystemConfig != null)
        {
            _distanceScale = systemManager.SystemConfig.MapBoardScale.x;
            _mapPencilActivationDist = systemManager.SystemConfig.MapPencilActivationDist;
        }

        _startingParent = PencilTransform.parent;

        if (XRInteractor != null)
        {
            XRInteractor.ObjectPickedUp += OnObjectPickedUp;
            XRInteractor.ObjectDropped += OnObjectDropped;
        }
    }

    private void OnDestroy()
    {
        if (XRInteractor != null)
        {
            XRInteractor.ObjectPickedUp -= OnObjectPickedUp;
            XRInteractor.ObjectDropped -= OnObjectDropped;
        }

        ReturnPencil();
    }


    private void OnObjectPickedUp(CustomXRInteractable obj)
    {
        DropPencil();
    }

    private void OnObjectDropped(CustomXRInteractable obj)
    {
        ReturnPencil();
    }

    private bool IsNearMapBoard()
    {
        if (_snapPointCollider != null)
        {
            var closestPt = _snapPointCollider.ClosestPoint(HandTransform.position);

            if (Vector3.Distance(HandTransform.position, closestPt) <= _mapPencilActivationDist)
            {
                return true;
            }
        }

        else if (_snapPointTransform != null)
        {
            if (Vector3.Distance(HandTransform.position, _snapPointTransform.position) <= 0.65f * _distanceScale)
            {
                return true;
            }
        }

        return false;
    }

    private void Update()
    {
        if (_snapPointTransform == null)
            return;

        if (UseProximity && XRInteractor.IsHoldingInteractable == false)
        {
            if (IsNearMapBoard())
            {                
                ReturnPencil();
                ControllerBehavior.PencilHideController();   
            }
            else
            {
                
                DropPencil();
                ControllerBehavior.PencilShowController();
            }
        }
        else if(XRInteractor.IsHoldingInteractable)
        {
            DropPencil();
            ControllerBehavior.PencilActive = false;
        }
       
    }
    //Overrides the tracking changes from the secondary XR controller on the pencil (used for UI interaction for mapman)
    private void LateUpdate()
    {
        if (_isInHand)
        {
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
        }
        else
        {
            transform.position = _snapPointTransform.position;
            transform.rotation = _snapPointTransform.rotation;//POTENTIALLY BUGGED
        }
    }
    public void DropPencil()
    {
        if (!gameObject.activeSelf)
            return;
        if (PencilTransform.parent != _snapPointTransform)
            PencilTransform.parent = _snapPointTransform;
        PencilTransform.localPosition = Vector3.zero;
        PencilTransform.localEulerAngles = Vector3.zero;
        PencilTransform.parent = _startingParent;//Might need to revert

        XRInteractor.ActiveGraphicsRaycaster = false;
        //ControllerBehavior.PencilHideController();

    }

    public void ReturnPencil()
    {
        if (!gameObject.activeSelf)
            return;
        if (PencilTransform.parent != _startingParent)
            PencilTransform.parent = _startingParent;
        PencilTransform.localPosition = Vector3.zero;
        PencilTransform.localEulerAngles = Vector3.zero;

        XRInteractor.ActiveGraphicsRaycaster = true;
        
    }

    public void SetSnapPoint(Transform snapTransform)
    {
        SnapPointTransform = snapTransform;
    }

    private void OnEnable()
    {
        XRInteractor.ActiveGraphicsRaycaster = true;
    }

    private void OnDisable()
    {
        XRInteractor.ActiveGraphicsRaycaster = false;
    }

    public void PencilShake()
    {
        XRInteractor.HapticShake();
    }
}
