using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generalized power toggle behavior compatible with loadable asset system
/// </summary>
public class PowerToggleBehavior : MonoBehaviour, IInteractableObject
{
    public NetworkedObject NetObj;
    //public List<GameObject> Lights;
    public List<ComponentInfo_Light> Lights;
    public List<AudioSource> AudioSources;
    //public List<MeshRenderer> Renderer;

    private bool _isOn = true;

    // Start is called before the first frame update
    void Start()
    {
        if (NetObj == null)
        {
            NetObj = GetComponentInParent<NetworkedObject>();
        }
        if (NetObj != null)
        {
            NetObj.RegisterMessageHandler(OnNetObjMessage);
        }
        if(Lights == null)
        {
            ComponentInfo_Light[] lightComps = GetComponentsInChildren<ComponentInfo_Light>();
            Lights = new List<ComponentInfo_Light>();
            foreach (var light in lightComps)
            {
                Lights.Add(light);
            }
        }
        if(AudioSources == null)
        {
            ComponentInfo_AudioSource[] audios = GetComponentsInChildren<ComponentInfo_AudioSource>();
            AudioSources = new List<AudioSource>();
            foreach (var audio in audios)
            {
                if(audio.IsAmbientSound)
                    AudioSources.Add(audio.m_component);
            }
        }
    }

    private void OnNetObjMessage(string messageType, CodedInputStream reader)
    {
        if(messageType == "POWER_ON")
        {
            NullCheck();
            TurnOn();
        }
        if(messageType == "POWER_OFF")
        {
            NullCheck();
            TurnOff();
        }
    }

    void NullCheck()
    {
        if (Lights == null)
        {
            ComponentInfo_Light[] lightComps = GetComponentsInChildren<ComponentInfo_Light>();
            Lights = new List<ComponentInfo_Light>();
            foreach (var light in lightComps)
            {
                Lights.Add(light);
            }
        }
        if (AudioSources == null)
        {
            ComponentInfo_AudioSource[] audios = GetComponentsInChildren<ComponentInfo_AudioSource>();
            AudioSources = new List<AudioSource>();
            foreach (var audio in audios)
            {
                if (audio.IsAmbientSound)
                    AudioSources.Add(audio.m_component);
            }
        }
    }

    private void TurnOn()
    {
        foreach (var lightComInf in Lights)
        {
            if (lightComInf.isOn)
            {
                foreach (var light in lightComInf.m_Lights)
                {
                    light.enabled = true;
                }
                foreach (var emr in lightComInf.emissiveMeshRenderers)
                {
                    emr.enabled = true;
                }
            }
        }
        foreach (var source in AudioSources)
        {
            source.Play();
        }
    }
    private void TurnOff()
    {
        foreach (var lightComInf in Lights)
        {
            if (lightComInf.isOn)
            {
                foreach (var light in lightComInf.m_Lights)
                {
                    light.enabled = false;
                }
                foreach (var emr in lightComInf.emissiveMeshRenderers)
                {
                    emr.enabled = false;
                }
            }
        }
        foreach (var source in AudioSources)
        {
            source.Stop();
        }
    }

    private void Toggle()
    {
        NullCheck();
        if (_isOn)
        {
            TurnOff();
            NetObj.SendMessage("POWER_OFF", new VRNTextMessage());
            _isOn = false;
        }
        else
        {
            TurnOn();
            NetObj.SendMessage("POWER_ON", new VRNTextMessage());
            _isOn = true;
        }
    }

    public ActivationState CanActivate => ActivationState.Ready;

    public void OnActivated(Transform interactor)
    {
        Toggle();
    }

    public void OnDeactivated(Transform interactor)
    {
    }

    public void OnDropped(Transform interactor)
    {
    }

    public void OnJoystickPressed(Transform interactor, bool pressed)
    {
    }

    public void OnPickedUp(Transform interactor)
    {
    }

    public void OnPrimaryButtonPressed(Transform interactor, bool pressed)
    {
    }

    public void OnSecondaryButtonPressed(Transform interactor, bool pressed)
    {
    }    
}
