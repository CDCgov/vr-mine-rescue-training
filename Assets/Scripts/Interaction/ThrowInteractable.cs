using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowInteractable : MonoBehaviour, IInteractableObject
{
    public Vector3 ThrowVelocity = new Vector3(0, 15, 0);
    public bool ThrowOnActivate = false;
    public bool ThrowOnPrimaryPressed = false;
    public bool ThrowOnSecondaryPressed = false;

    public bool RetrieveOnRelease = false;

    private Vector3 _savedPos;
    private Quaternion _savedRot;
    private Transform _savedParent;

    private CustomXRInteractable _interactable;
    private Rigidbody _rb;
    private Transform _interactor = null;

    // Start is called before the first frame update
    void Start()
    {
        TryGetComponent<Rigidbody>(out _rb);
        TryGetComponent<CustomXRInteractable>(out _interactable);
    }

    public void OnJoystickPressed(Transform interactor, bool pressed)
    {

    }

    public void OnPrimaryButtonPressed(Transform interactor, bool pressed)
    {
        if (!ThrowOnPrimaryPressed)
            return;

        if (pressed)
            Throw();
        else if (RetrieveOnRelease)
            Retrieve();
    }

    public void OnSecondaryButtonPressed(Transform interactor, bool pressed)
    {
        if (!ThrowOnSecondaryPressed)
            return;

        if (pressed)
            Throw();
        else if (RetrieveOnRelease)
            Retrieve();
    }

    public void OnPickedUp(Transform interactor)
    {
        
    }

    public void OnDropped(Transform interactor)
    {
        
    }

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {
        _interactor = interactor;
        Debug.Log($"ThrowInteractable: Activated {gameObject.name}");
        if (ThrowOnActivate)
            Throw();
    }

    public void OnDeactivated(Transform interactor)
    {
        _interactor = interactor;
        if (ThrowOnActivate && RetrieveOnRelease)
            Retrieve();
    }

    private void Throw()
    {
        var throwVelocity = transform.TransformDirection(ThrowVelocity);
        _savedPos = transform.position;
        _savedRot = transform.rotation;

        IThrowableObject throwable = null;
        if (_interactable != null)
            _interactable.TryGetComponent<IThrowableObject>(out throwable);

        if (throwable != null)
            ThrowThrowableObject(throwVelocity, throwable);
        else
            ThrowRigidbody(throwVelocity);
    }

    private void ThrowThrowableObject(Vector3 throwVelocity, IThrowableObject throwable)
    {
        if (_interactable == null)
            return;

        if (_interactor != null && _interactor.TryGetComponent<CustomXRInteractor>(out var xrInteractor))
        {
            xrInteractor.DropInteractable(true, true, throwVelocity, Vector3.zero);
        }
        else
        {
            throwable.ThrowObject(transform, throwVelocity, Random.insideUnitSphere * 15);
        }
    }

    private void ThrowRigidbody(Vector3 throwVelocity)
    {
        if (_rb == null || _interactable == null)
            return;

        _interactable.ChangeOwnership(null);
        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.velocity = throwVelocity;
    }

    private void Retrieve()
    {
        if (_rb == null || _interactable == null)
            return;

        transform.position = _savedPos;
        transform.rotation = _savedRot;

        //if (!TryGetComponent<Rigidbody>(out var rb) || _interactable == null)
        //    return;

        //rb.isKinematic = true;
        //rb.useGravity = false;
        //rb.velocity = Vector3.zero;
        //_interactable.IsLargeItem = false;

        //transform.SetParent(_savedParent);
        //transform.localPosition = _savedPos;
        //transform.localRotation = _savedRot;
    }
}
