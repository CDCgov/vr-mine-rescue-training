using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InjuredNPCState { Start, Following, Stopped }
public class InjuredNPCIdleBehavior : MonoBehaviour, IInteractableObject
{
    public AudioSource VoiceSource;
    public SoundCollection InjuredVoiceCollection;
    public SoundCollection InjuredFollowingCollection;
    public AudioClip Greet;
    public float DialogLoopDelay = 40;
    
    bool _LoopDialog = true;
    bool _initialMeet = true;
    float _triggerTime = 0;
    private InjuredNPCState InjuredNPCState = InjuredNPCState.Start;
    private NetworkedObject _netObj;

    private void Start()
    {
        _netObj = GetComponent<NetworkedObject>();

        if(_netObj != null)
        {
            _netObj.RegisterMessageHandler(OnNetObjMessage);
        }
        //_triggerTime = Time.time + 45;
    }
    // Update is called once per frame
    void Update()
    {
        if (!_netObj.HasAuthority)
        {
            return;
        }

        if (_initialMeet)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 1.5f);
            foreach(Collider col in colliders)
            {
                MinePlayerInfo mpInfo = col.GetComponent<MinePlayerInfo>();
                if(mpInfo != null && !VoiceSource.isPlaying)
                {
                    VoiceSource.clip = Greet;
                    VoiceSource.Play();
                    VRNFloatData vRNFloatData = new VRNFloatData();
                    vRNFloatData.FloatData = 0;
                    _netObj.SendMessage("Init", vRNFloatData);
                    _initialMeet = false;
                    _triggerTime = Time.time + DialogLoopDelay;
                }
            }
            return;
        }

        if (_LoopDialog)
        {
            if (Time.time > _triggerTime)
            {
                if (!VoiceSource.isPlaying)
                {
                    int selection = 0;
                    string messageType = "Idle";
                    switch (InjuredNPCState)
                    {
                        case InjuredNPCState.Start:
                            selection = Random.Range(0, InjuredVoiceCollection.Sounds.Length);
                            VoiceSource.clip = InjuredVoiceCollection.Sounds[selection];
                            messageType = "Idle";
                            break;
                        case InjuredNPCState.Following:
                            selection = Random.Range(0, InjuredFollowingCollection.Sounds.Length);
                            VoiceSource.clip = InjuredFollowingCollection.Sounds[selection];
                            messageType = "Follow";
                            break;
                        case InjuredNPCState.Stopped:
                            selection = Random.Range(0, InjuredFollowingCollection.Sounds.Length);
                            VoiceSource.clip = InjuredFollowingCollection.Sounds[selection];
                            messageType = "Follow";
                            break;
                        default:
                            break;
                    }                    
                    VoiceSource.Play();
                    VRNFloatData vRNFloatData = new VRNFloatData();
                    vRNFloatData.FloatData = (float)selection;
                    _netObj.SendMessage(messageType, vRNFloatData);
                    _triggerTime = Time.time + DialogLoopDelay;
                }
            }
        }
    }

    private void OnNetObjMessage(string messageType, CodedInputStream reader)
    {
        var msg = new VRNFloatData();
        reader.ReadMessage(msg);
        int selection = (int)msg.FloatData;
        switch (messageType)
        {
            case "Init":
                VoiceSource.clip = Greet;
                _initialMeet = false;
                VoiceSource.Play();
                break;
            case "Idle":
                VoiceSource.clip = InjuredVoiceCollection.Sounds[selection];
                VoiceSource.Play();
                break;
            case "Following":
                VoiceSource.clip = InjuredFollowingCollection.Sounds[selection];
                VoiceSource.Play();
                break;
            default:
                break;
        }
    }

    public void OnGrab()
    {
        int selection = Random.Range(0, InjuredFollowingCollection.Sounds.Length);
        VoiceSource.clip = InjuredFollowingCollection.Sounds[selection];
        string messageType = "Follow";
        VoiceSource.Play();
        VRNFloatData vRNFloatData = new VRNFloatData();
        vRNFloatData.FloatData = (float)selection;
        _netObj.SendMessage(messageType, vRNFloatData);
    }

    private void OnDestroy()
    {
        _netObj.UnregisterMessageHandler(OnNetObjMessage);
    }

    public void EndIdleLoop()
    {
        _LoopDialog = false;

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
        OnGrab();
    }

    public void OnDropped(Transform interactor)
    {

    }

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {
        EndIdleLoop();
    }

    public void OnDeactivated(Transform interactor)
    {
        
    }
}
