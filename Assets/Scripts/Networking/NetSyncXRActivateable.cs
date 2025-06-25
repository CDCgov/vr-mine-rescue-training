using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Google.Protobuf;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(CustomXRInteractable))]
public class NetSyncXRActivateable : MonoBehaviour, INetSync, IInteractableObject
{
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;
    private CustomXRInteractable _interact;
    private bool _receivingData = false;
    private bool _localActivation = false;
    private VRNActivationState _state = null;
    //private VRNObjectData _objectData;

    private Vector3 _activationStartPos;
    private float _activationStartTime;
    private float _activationEndTime;

    private List<IInteractableObject> _interactInterfaces;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        _interactInterfaces = new List<IInteractableObject>();
        _interact = GetComponent<CustomXRInteractable>();
        _interact.GetComponentsInChildren<IInteractableObject>(_interactInterfaces);
        //_objectData = GetComponent<VRNObjectData>();

        //_interact.onActivate.AddListener((interactor) => 
        //{
        //	//Debug.Log("Got Activate");
        //	_localActivation = true;
        //});

        //_interact.onDeactivate.AddListener((interactor) =>
        //{
        //	//Debug.Log("Got Deactivate");
        //	_localActivation = false;
        //});
        //_interact.onActivate.AddListener(Activate);
        //_interact.onDeactivate.AddListener(Deactivate);
    }

    public bool NeedsUpdate()
    {
        return true;
    }

    public void SyncObjState(CodedInputStream reader)
    {
        _receivingData = true;

        //_state = VRNActivationState.Parser.ParseDelimitedFrom(reader);

        if (_state == null)
            _state = new VRNActivationState();

        _state.ActivatationLevel = 0;
        _state.Activated = false;
        reader.ReadMessage(_state);

        //Debug.Log($"Received Sync Activation {_state.ActivatationLevel.ToString()} for {gameObject.name}");

        if (_state.Activated && !_localActivation)
            ActivateInteractable();
        else if (!_state.Activated && _localActivation)
            DeactivateInteractable();
    }

    private void ActivateInteractable()
    {
        foreach (var iface in _interactInterfaces)
        {
            if ((MonoBehaviour)iface == this)
                continue;

            iface?.OnActivated(transform);
        }
    }
    
    private void DeactivateInteractable()
    {
        foreach (var iface in _interactInterfaces)
        {
            if ((MonoBehaviour)iface == this)
                continue;

            iface?.OnDeactivated(transform);
        }
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        _receivingData = false;
        //VRNActivationState state = new VRNActivationState
        //{ 
        //    ActivatationLevel = _localActivation ? 1.0f : 0.0f,
        //    Activated = _localActivation,
        //};

        if (_state == null)
            _state = new VRNActivationState();

        _state.ActivatationLevel = _localActivation ? 1.0f : 0.0f;
        _state.Activated = _localActivation;

        writer.WriteMessage(_state);

        //state.Activated = _localActivation;
        //Debug.LogError($"Local Activation: {_localActivation}");

        //state.WriteDelimitedTo(writer);
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
        _localActivation = true;
    }

    public void OnDeactivated(Transform interactor)
    {
        _localActivation = false;
    }
}
