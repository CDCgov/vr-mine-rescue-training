using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMoveToStretcher : MonoBehaviour, IInteractableObject
{
    public NetworkManager NetworkManager;


    private Animator _animator;
    private StretcherController _stretcher;
    private NPCController _npc;
    private NetworkedObject _netObj;

    private const string UnconsciousStateName = "Unconscious";

    public ActivationState CanActivate
    {
        get
        {
            if (_animator == null)
                return ActivationState.Unknown;

            if (!_animator.GetCurrentAnimatorStateInfo(0).IsName(UnconsciousStateName))
                return ActivationState.Unknown;

            if (_stretcher == null)
            {

                Debug.LogWarning($"NPC {gameObject.name} didn't find StretcherController on start");
                _stretcher = GameObject.FindObjectOfType<StretcherController>();

                if (_stretcher == null)
                    return ActivationState.Unknown;
            }

            if (_stretcher.IsNPCActive)
                return ActivationState.Unavailable;
            else
                return ActivationState.Ready;
        }
    }

    public void OnActivated(Transform interactor)
    {
        if (_animator == null || !_animator.GetCurrentAnimatorStateInfo(0).IsName(UnconsciousStateName))
            return;

        if (_stretcher == null)
        {
            _stretcher = GameObject.FindObjectOfType<StretcherController>();
            if (_stretcher == null)
            {
                return;
            }
        }

        if (_stretcher.IsNPCActive)
        {
            return;
        }
        _stretcher.RequestSetNPCActive(true);
        _stretcher.RequestSetNPCName(_npc.NPCName);
        if (_npc != null)
        {
            if (_npc.HasBG4)
            {
                _stretcher.RequestSetBG4Active(true);
            }

            NetworkManager.LogSessionEvent(VRNLogEventType.NpcplacedOnStretcher, null,
                transform.position, transform.rotation, _npc.NPCName);

        }
        if (_netObj != null)
        {
            NetworkManager.DestroyObject(_netObj.uniqueID);
        }

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

    // Start is called before the first frame update
    void Start()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        _stretcher = GameObject.FindObjectOfType<StretcherController>();
        if (_stretcher == null)
        {
            Debug.LogWarning($"NPC {gameObject.name} couldn't find StretcherController in scene");
        }

        TryGetComponent<NetworkedObject>(out _netObj);
        TryGetComponent<NPCController>(out _npc);

        NetworkManager = NetworkManager.GetDefault(gameObject);
    }


    //private void Update()
    //{
    //    if (Input.GetKeyUp(KeyCode.Backslash))
    //    {
    //        if (!NetworkManager.IsServer)
    //        {
    //            return;
    //        }
    //        if (_animator == null || !_animator.GetCurrentAnimatorStateInfo(0).IsName(UnconsciousStateName))
    //            return;

    //        if (_stretcher == null)
    //        {
    //            _stretcher = GameObject.FindObjectOfType<StretcherController>();
    //            if (_stretcher == null)
    //            {
    //                return;
    //            }
    //        }

    //        if (_stretcher.IsNPCActive)
    //        {
    //            return;
    //        }
    //        _stretcher.RequestSetNPCActive(true);
    //        if (_npc != null)
    //        {
    //            if (_npc.HasBG4)
    //            {
    //                _stretcher.RequestSetBG4Active(true);
    //            }

    //            _stretcher.RequestSetNPCName(_npc.NPCName);

    //            NetworkManager.LogSessionEvent(VRNLogEventType.NpcplacedOnStretcher, null,
    //                transform.position, transform.rotation, _npc.NPCName);

    //        }
    //        if (_netObj != null)
    //        {
    //            NetworkManager.DestroyObject(_netObj.uniqueID);
    //        }
    //    }
    //}
}
