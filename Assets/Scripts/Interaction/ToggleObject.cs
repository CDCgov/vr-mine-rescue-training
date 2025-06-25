using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ToggleObject : MonoBehaviour, IInteractableObject
{
    public AudioClip ToggleSfxClip;
    public AudioSource ToggleSource;
    public List<AudioSource> AudioToToggle;
    public List<GameObject> ObjectsToToggle;
    public List<Light> LightsToToggle;
    public bool IsOn = true;
    public UnityEvent<bool> onToggle;
    
    private NetworkedObject _netObj;

    public ActivationState CanActivate => ActivationState.Ready;

    public void OnActivated(Transform interactor)
    {
        if(ToggleSfxClip == null || ToggleSource == null || ObjectsToToggle == null || LightsToToggle == null)
        {
            ToggleHandler handler = GetComponentInChildren<ToggleHandler>();
            handler.PopulateBehavior(this);
        }
        DoToggle();
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
        if (ToggleSfxClip == null || ToggleSource == null || ObjectsToToggle == null || LightsToToggle == null)
        {
            ToggleHandler handler = GetComponentInChildren<ToggleHandler>();
            handler.PopulateBehavior(this);
        }
        DoToggle();
    }

    public void OnSecondaryButtonPressed(Transform interactor, bool pressed)
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        if(_netObj == null)
        {
            _netObj = GetComponent<NetworkedObject>();
        }
        if (_netObj != null)
        {
            _netObj.RegisterMessageHandler(HandleNetObjMessage);
        }
    }

    //public void Update()
    //{
    //    if (Input.GetKeyUp(KeyCode.RightShift))
    //    {
    //        DoToggle();
    //    }
    //}

    void OnDestroy()
    {
        if (_netObj != null)
        {
            _netObj.UnregisterMessageHandler(HandleNetObjMessage);
        }
    }
    private void HandleNetObjMessage(string messageType, CodedInputStream reader)
    {
        if(messageType == "MSG_TGL")
        {
            var msg = new VRNTextMessage();
            reader.ReadMessage(msg);
            bool toggle = false;
            if(bool.TryParse(msg.Message, out toggle))
            {
                ProcessToggle(toggle);
            }
            //ProcessToggle(msg);
        }
        if(messageType == "TG_ON")
        {
            ProcessToggle(true);
        }
        if(messageType == "TG_OFF")
        {
            ProcessToggle(false);
        }
    }

    void DoToggle()
    {
        ProcessToggle(!IsOn, true);
    }

    void ProcessToggle(bool isOn, bool sendMessageOut = false)
    {
        if(IsOn != isOn)
        {
            IsOn = isOn;
            if(ToggleSource != null && ToggleSfxClip != null)
            {
                ToggleSource.clip = ToggleSfxClip;
                ToggleSource.Play();
            }
            foreach (AudioSource audio in AudioToToggle)
            {
                if (IsOn)
                {
                    audio.Play();
                }
                else
                {
                    audio.Stop();
                }
            }
            foreach(GameObject obj in ObjectsToToggle)
            {
                obj.SetActive(IsOn);
            }
            foreach(Light light in LightsToToggle)
            {
                light.enabled = IsOn;
            }

            onToggle.Invoke(IsOn);
            if (sendMessageOut)
            {
                VRNTextMessage vRNTextMessage = new VRNTextMessage();
                
                if (IsOn)
                {
                    _netObj.SendMessage("TG_ON", vRNTextMessage);
                }
                else
                {
                    _netObj.SendMessage("TG_OFF", vRNTextMessage);
                }
                //vRNTextMessage.Message = IsOn.ToString();
                //_netObj.SendMessage("MSG_TG", vRNTextMessage);
            }
        }
    }
}
