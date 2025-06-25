using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerCenterInteract : Interactable, IInteractableObject {

    public AudioSource PcAudioSource;
    public Light PcLight;
    public GameObject PcLightEmissive;
    private bool IsOn = true;
    private NetworkedObject _netObj;

    private void Start()
    {
        _netObj = GetComponentInParent<NetworkedObject>();
        if(_netObj != null)
        {
            _netObj.RegisterMessageHandler(OnNetObjMessage);
        }
    }
    private void OnNetObjMessage(string messageType, CodedInputStream reader)
    {
        if(messageType == "PC_ON")
        {
            IsOn = true;
        }
        if(messageType == "PC_OFF")
        {
            IsOn = false;
        }
        if (IsOn)
        {
            if (!PcAudioSource.isPlaying)
            {
                PcAudioSource.Play();
                PcLightEmissive.SetActive(IsOn);
            }
        }
        else
        {
            if (PcAudioSource.isPlaying)
            {
                PcAudioSource.Stop();
                PcLightEmissive.SetActive(IsOn);
            }
        }
        PcLight.enabled = IsOn;
    }

    public void Update()
    {
        //if (IsOn)
        //{
        //    if (!PcAudioSource.isPlaying)
        //    {
        //        PcAudioSource.Play();
        //        PcLightEmissive.SetActive(IsOn);
        //    }            
        //}
        //else
        //{
        //    if (PcAudioSource.isPlaying)
        //    {
        //        PcAudioSource.Stop();
        //        PcLightEmissive.SetActive(IsOn);
        //    }
        //}
        //PcLight.enabled = IsOn;

        if(_netObj == null)
        {
            Debug.Log("Net Object check performed?");
            _netObj = GetComponent<NetworkedObject>();
            if (_netObj != null)
            {
                _netObj.RegisterMessageHandler(OnNetObjMessage);
            }
        }
        else
        {
            Debug.Log($"Power center net object not null: {_netObj.gameObject.name}");
        }
    }

    public override void Interact()
    {
        IsOn = !IsOn;

        if (IsOn)
        {
            if (!PcAudioSource.isPlaying)
            {
                PcAudioSource.Play();
                PcLightEmissive.SetActive(IsOn);
            }
        }
        else
        {
            if (PcAudioSource.isPlaying)
            {
                PcAudioSource.Stop();
                PcLightEmissive.SetActive(IsOn);
            }
        }
        PcLight.enabled = IsOn;
        if (_netObj != null)
        {
            if (IsOn)
            {
                _netObj.SendMessage("PC_ON", new VRNTextMessage());
            }
            else
            {
                _netObj.SendMessage("PC_OFF", new VRNTextMessage());
            }
        }
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

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {
        Interact();
    }

    public void OnDeactivated(Transform interactor)
    {

    }
}
