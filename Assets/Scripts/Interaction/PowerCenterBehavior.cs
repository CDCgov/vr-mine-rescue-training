using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerCenterBehavior : MonoBehaviour, IInteractableObject
{
    public NetworkedObject NetObj;
    public GameObject PCLight;
    public AudioSource PCAudio;
    public Material EmissiveOff;
    public Material EmissiveOn;
    public MeshRenderer Renderer;

    public bool PCOn = true;
    // Start is called before the first frame update
    void Start()
    {
        if(NetObj == null)
        {
            NetObj = GetComponentInParent<NetworkedObject>();
        }


        if(NetObj != null)
        {
            NetObj.RegisterMessageHandler(OnNetObjMessage);
        }

        if(Renderer == null)
        {
            Renderer = GetComponent<MeshRenderer>();
        }
    }
    public void TogglePowerCenter()
    {
        if (PCOn)
        {
            TurnOffPowerCenter();
            NetObj.SendMessage("PC_OFF", new VRNTextMessage());
        }
        else
        {
            TurnOnPowerCenter();
            NetObj.SendMessage("PC_ON", new VRNTextMessage());
        }
    }

    private void OnNetObjMessage(string messageType, CodedInputStream reader)
    {
        if(messageType == "PC_OFF")
        {
            TurnOffPowerCenter();
        }

        if(messageType == "PC_ON")
        {
            TurnOnPowerCenter();
        }
    }

    private void TurnOnPowerCenter()
    {
        PCAudio.enabled = true;
        PCLight.SetActive(true);
        Renderer.material = EmissiveOn;
        PCOn = true;
    }
    private void TurnOffPowerCenter()
    {
        PCAudio.enabled = false;
        PCLight.SetActive(false);
        Renderer.material = EmissiveOff;
        PCOn = false;
    }


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

    }

    public void OnDropped(Transform interactor)
    {

    }

    public ActivationState CanActivate => ActivationState.Ready;

    public void OnActivated(Transform interactor)
    {
        TogglePowerCenter();
    }

    public void OnDeactivated(Transform interactor)
    {

    }
}
