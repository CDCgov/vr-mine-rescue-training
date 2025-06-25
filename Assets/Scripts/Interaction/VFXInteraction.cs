using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXInteraction : MonoBehaviour, IInteractableObject
{
    public TeleportManager TeleportManager;

    public string VFXEventActivate;
    public string VFXEventDeactivate;
    public string VFXEventPrimaryButton;
    public string VFXEventSecondaryButton;
    public string VFXEventJoystickPress;
    public VisualEffect VFX;

    public bool SetGroundPlanePosition = false;

    private VRNInteractionData _vrnEvent;
    

    private bool HasAuthority
    {
        get
        {
            if (_netObj == null || _netObj.HasAuthority)
                return true;
            else
                return false;
        }
    }

    private NetworkedObject _netObj;



    void Start()
    {
        _vrnEvent = new VRNInteractionData();
        
        _netObj = GetComponent<NetworkedObject>();

        if (SetGroundPlanePosition && TeleportManager == null)
            TeleportManager = TeleportManager.GetDefault(gameObject);
        

        if (_netObj != null)
            _netObj.RegisterMessageHandler(OnNetObjMessage);
    }

    private void OnNetObjMessage(string messageType, CodedInputStream reader)
    {
        if (messageType != "VFXInteract")
            return;

        _vrnEvent.Interaction = VRNVRInteraction.VriNone;
        reader.ReadMessage(_vrnEvent);

        switch (_vrnEvent.Interaction)
        {
            case VRNVRInteraction.VriActivated:
                TriggerVFXEvent(VFXEventActivate);
                break;

            case VRNVRInteraction.VriDeactivated:
                TriggerVFXEvent(VFXEventDeactivate);
                break;

            case VRNVRInteraction.VriJoystickPressed:
                TriggerVFXEvent(VFXEventJoystickPress);
                break;

            case VRNVRInteraction.VriPrimaryButtonPressed:
                TriggerVFXEvent(VFXEventPrimaryButton);
                break;

            case VRNVRInteraction.VriSecondaryButtonPressed:
                TriggerVFXEvent(VFXEventSecondaryButton);
                break;
        }
    }

    private void TriggerVFXEvent(string eventName)
    {
        if (VFX == null || eventName == null || eventName.Length <= 0)
            return;

        if (SetGroundPlanePosition && 
            TeleportManager != null && TeleportManager.ActiveTeleportTarget != null)
        {
            VFX.SetVector3("GroundPlanePos", TeleportManager.ActiveTeleportTarget.position);
        }

        VFX.SendEvent(eventName);
    }

    private void SendNetworkVFXEvent(VRNVRInteraction interaction)
    {
        if (_netObj == null)
            return;

        _vrnEvent.Interaction = interaction;

        _netObj.SendMessage("VFXInteract", _vrnEvent);

    }

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {
        if (VFXEventActivate == null || VFXEventActivate.Length <= 0)
            return;

        TriggerVFXEvent(VFXEventActivate);
        SendNetworkVFXEvent(VRNVRInteraction.VriActivated);
    }

    public void OnDeactivated(Transform interactor)
    {
        if (VFXEventDeactivate == null || VFXEventDeactivate.Length <= 0)
            return;

        TriggerVFXEvent(VFXEventDeactivate);
        SendNetworkVFXEvent(VRNVRInteraction.VriDeactivated);
    }

    public void OnDropped(Transform interactor)
    {
    }

    public void OnJoystickPressed(Transform interactor, bool pressed)
    {
        if (VFXEventJoystickPress == null || VFXEventJoystickPress.Length <= 0)
            return;

        TriggerVFXEvent(VFXEventJoystickPress);
        SendNetworkVFXEvent(VRNVRInteraction.VriJoystickPressed);
    }

    public void OnPickedUp(Transform interactor)
    {

    }

    public void OnPrimaryButtonPressed(Transform interactor, bool pressed)
    {
        if (VFXEventPrimaryButton == null || VFXEventPrimaryButton.Length <= 0)
            return;

        TriggerVFXEvent(VFXEventPrimaryButton);
        SendNetworkVFXEvent(VRNVRInteraction.VriPrimaryButtonPressed);
    }

    public void OnSecondaryButtonPressed(Transform interactor, bool pressed)
    {
        if (VFXEventSecondaryButton == null || VFXEventSecondaryButton.Length <= 0)
            return;

        TriggerVFXEvent(VFXEventSecondaryButton);
        SendNetworkVFXEvent(VRNVRInteraction.VriSecondaryButtonPressed);
    }

}
