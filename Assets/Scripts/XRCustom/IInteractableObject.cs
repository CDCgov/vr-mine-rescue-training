using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActivationState
{
    Unknown,
    Ready,
    Error,
    Unavailable,
}

public interface IInteractableObject
{
    void OnJoystickPressed(Transform interactor, bool pressed);
    //void OnTriggerPressed(bool pressed);
    //void OnGripPressed(bool pressed);
    void OnPrimaryButtonPressed(Transform interactor, bool pressed);
    void OnSecondaryButtonPressed(Transform interactor, bool pressed);
    void OnPickedUp(Transform interactor);
    void OnDropped(Transform interactor);

    public ActivationState CanActivate { get; }
    void OnActivated(Transform interactor);
    void OnDeactivated(Transform interactor);
}
