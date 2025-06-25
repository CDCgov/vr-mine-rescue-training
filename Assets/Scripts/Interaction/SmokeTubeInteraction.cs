using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class SmokeTubeInteraction : MonoBehaviour, IInteractableObject
{
    public SkinnedMeshRenderer SkinMeshRenderer;
    public string VFXEventName = "EmitBurst";
    public VisualEffect _vfx;
    public NetworkedObject _netObj;
    public PlayerManager PlayerManager;
    public NetworkManager NetworkManager;
    private bool _pressed = false;
    private float _lerpVal = 0;
    private float _deformation = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (SkinMeshRenderer == null)
            SkinMeshRenderer = GetComponent<SkinnedMeshRenderer>();

        if(_vfx == null)
        {
            _vfx = GetComponentInChildren<VisualEffect>();
        }

        if(_netObj == null)
        {
            _netObj = GetComponentInParent<NetworkedObject>();
        }

        if (_netObj != null)
            _netObj.RegisterMessageHandler(OnNetObjMessage);

        if(PlayerManager == null)
        {
            PlayerManager = PlayerManager.GetDefault(gameObject);
        }

        if(NetworkManager == null)
        {
            NetworkManager = NetworkManager.GetDefault(gameObject);
        }
    }

    private void OnNetObjMessage(string messageType, CodedInputStream reader)
    {
        if(messageType == "EMIT")
        {
            _vfx.SendEvent(VFXEventName);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_pressed)
        {
            if (_deformation < 100)
            {
                _deformation += 200 * Time.deltaTime;
                if(_deformation > 100)
                {
                    _deformation = 100;
                }
                SkinMeshRenderer.SetBlendShapeWeight(0, _deformation);
            }
            else
            {
                _deformation = 100;
            }

        }
        else
        {
            if (_deformation > 0)
            {
                _deformation -= 200 * Time.deltaTime;
                if(_deformation < 0)
                {
                    _deformation = 0;
                }
                SkinMeshRenderer.SetBlendShapeWeight(0, _deformation);
            }
            else
            {
                _deformation = 0;
            }
        }
    }

    //public void OnActivate()
    //{
        
    //}

    //public void OnDeactivate()
    //{
       
    //}

    //public void OnSelectExit()
    //{
    //    _pressed = false;
    //    _deformation = 0;
    //    SkinMeshRenderer.SetBlendShapeWeight(0, _deformation);
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

    }

    public void OnDropped(Transform interactor)
    {

    }

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {
        _pressed = true;

        if (_vfx == null)
            return;

        _vfx.SendEvent(VFXEventName);
        if (_netObj != null)
        {
            _netObj.SendMessage("EMIT", new VRNTextMessage());
        }
        NetworkManager.LogSessionEvent(new VRNLogEvent
        {
            EventType = VRNLogEventType.SmokeTube,
            ObjectType = VRNLogObjectType.SmokeTube,
            ObjectName = "Smoke Tube",
            Position = transform.position.ToVRNVector3(),
            Rotation = transform.rotation.ToVRNQuaternion(),
            SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID
        });
    }

    public void OnDeactivated(Transform interactor)
    {
        _pressed = false;
    }
}
