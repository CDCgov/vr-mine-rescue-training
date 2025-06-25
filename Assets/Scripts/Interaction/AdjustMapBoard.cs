using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CustomXRInteractable))]
public class AdjustMapBoard : MonoBehaviour, IInteractableObject
{
    public CustomXRInteractable Interactable;

    private Transform _interactorTransform;
    private Vector3 _offset;
    private Quaternion _rotationOffset;
    private bool _allowAdjust = false;

    private Vector3 _interactorStartPos;
    private Quaternion _interactorStartRot;

    private Vector3 _mapStartPos;
    private Quaternion _mapStartRot;

    // Start is called before the first frame update
    void Start()
    {
        if (Interactable == null)
            Interactable = GetComponent<CustomXRInteractable>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_allowAdjust)
            return;

        var deltaRot = _interactorTransform.rotation * (Quaternion.Inverse(_interactorStartRot));
        //Debug.Log(deltaRot.eulerAngles);
        //transform.rotation = _mapStartRot * deltaRot;
        transform.rotation = deltaRot * _mapStartRot;

        var deltaPos = _interactorTransform.position - _interactorStartPos;
        transform.position = _mapStartPos + deltaPos;

        //Vector3 originalLocalPos = transform.localPosition;
        //Vector3 targetPos = transform.parent.InverseTransformPoint(_interactorTransform.position);
        //Vector3 finalPos = new Vector3();

        //transform.localPosition = targetPos + _offset;
        //transform.rotation = _interactorTransform.rotation;
        //transform.Rotate(new Vector3(-90, 0, 0), Space.Self);
    }

    //public void OnGrab()
    //{
    //    //_interactorTransform = Interactable.InteractorTransform;
    //    //_offset = transform.localPosition - transform.parent.InverseTransformPoint(_interactorTransform.position);
    //    //_rotationOffset = Quaternion.FromToRotation(_interactorTransform.forward, transform.forward);
    //    _allowAdjust = true;
    //    Debug.Log("~~~~~Mapboard grab triggered!~~~~~");
    //}

    //public void OnRelease()
    //{
    //    _allowAdjust = false;
    //    Debug.Log("~~~~~Mapboard grab triggered!~~~~~");
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
        _interactorTransform = interactor;
        _allowAdjust = true;

        _interactorStartPos = interactor.position;
        _interactorStartRot = interactor.rotation;

        _mapStartPos = transform.position;
        _mapStartRot = transform.rotation;
    }

    public void OnDropped(Transform interactor)
    {
        _allowAdjust = false;
    }

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {

    }

    public void OnDeactivated(Transform interactor)
    {

    }
}
