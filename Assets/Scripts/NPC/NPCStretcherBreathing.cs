using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStretcherBreathing : MonoBehaviour, IInteractableObject
{
    public bool PerformBreathing = true;
    public bool IsAlive = true;
    public float MaxBlendWeight = 50;
    public float BreathsPerMinute = 12;
    public SkinnedMeshRenderer SkinnedMesh;
    public VentilationManager VentilationManager;
    public StretcherController StretcherController;
    public NetworkManager NetworkManager;
    public NetworkedObject NetObj;
    public NPCTriggerDeath NPCTriggerDeath;
    public float MaxSyncInterval = 0.1f;
    public PlayerManager PlayerManager;
    

    float _blendShapeWeight = 0;
    float _halfBreathTime = 0;
    float t = 0;
    bool _breatheIn = true;
    private MineAtmosphere _atmosphere;
    private bool _hasBeenAssessed = false;
    private VRNBoolData _vRNBoolData;
    private float _lastSyncTime = 0;
    private bool _airCheckLogged = false;
    public Animator StretcherAnimator;
    private PlayerRepresentation _cachedPlayer;

    public ActivationState CanActivate
    {
        get
        {           
            if (!_hasBeenAssessed && !StretcherAnimator.GetBool("Breathing"))
            {
                NetworkManager.LogSessionEvent(VRNLogEventType.NpcstatusUpdate, $"Condition: Dead",
                    transform.position, transform.rotation, gameObject.name);

                _hasBeenAssessed = true;
            }
            else if (!_hasBeenAssessed && StretcherAnimator.GetBool("Breathing")) 
            {
                NetworkManager.LogSessionEvent(VRNLogEventType.NpcstatusUpdate, $"Condition: Alive",
                    transform.position, transform.rotation, gameObject.name);

                _hasBeenAssessed = true;
            }

            if (IsAlive)
            {
                return ActivationState.Unavailable;
            }
            else
            {
                return ActivationState.Error;
            }
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        //TryGetComponent<SkinnedMeshRenderer>(out SkinnedMesh);
        _halfBreathTime = (60f / BreathsPerMinute) / 2f;

        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        if(StretcherController == null)
            StretcherController = gameObject.GetComponentInParent<StretcherController>();

        if(NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);

        _atmosphere = new MineAtmosphere();
        if(StretcherAnimator == null)
            TryGetComponent<Animator>(out StretcherAnimator);

        if(NetObj == null)
        {
            NetObj = gameObject.GetComponentInParent<NetworkedObject>();
        }
        _vRNBoolData = new VRNBoolData();
        NetObj.RegisterMessageHandler(OnNetObjMessage);
        if(NPCTriggerDeath != null)
        {
            NPCTriggerDeath.OnDeath += NPCTriggerDeath_OnDeath;
            //NPCTriggerDeath.OnUnconscious += NPCTriggerDeath_OnUnconscious;
        }
    }

    //private void NPCTriggerDeath_OnUnconscious()
    //{
    //    throw new System.NotImplementedException();
    //}

    private void NPCTriggerDeath_OnDeath()
    {
        IsAlive = false;
        _hasBeenAssessed = false;
    }

    private void OnNetObjMessage(string messageType, CodedInputStream reader)
    {
        _vRNBoolData.BoolData = false;
        reader.ReadMessage(_vRNBoolData);
        switch (messageType)
        {
            case "BREATHING":
                IsAlive = _vRNBoolData.BoolData;
                PerformBreathing = _vRNBoolData.BoolData;
                StretcherController.RequestSetBreathing(_vRNBoolData.BoolData);
                break;
        }
    }

        // Update is called once per frame
    //    void Update()
    //{
    //    if (!IsAlive)
    //    {
    //        return;
    //    }
    //    if (PerformBreathing && SkinnedMesh != null)
    //    {
    //        if (_breatheIn)
    //        {
    //            t += Time.deltaTime;
    //            _blendShapeWeight = Mathf.Lerp(0, MaxBlendWeight, t / _halfBreathTime);
                
    //            SkinnedMesh.SetBlendShapeWeight(0, _blendShapeWeight);
    //            if(t >= _halfBreathTime)
    //            {
    //                _breatheIn = false;
    //                t = 0;
    //            }
    //            //Do a Death check here; logic being that a lethal breath happens on the breathing in phase
    //            //PerformBreathing = IsGoodAir();
    //            //GoodAirCheck();
    //        }
    //        else
    //        {
    //            t += Time.deltaTime;
    //            _blendShapeWeight = Mathf.Lerp(MaxBlendWeight, 0, t / _halfBreathTime);
    //            SkinnedMesh.SetBlendShapeWeight(0, _blendShapeWeight);
    //            if (t >= _halfBreathTime)
    //            {
    //                _breatheIn = true;
    //                t = 0;
    //            }
    //        }
    //    }
    //}

    //void GoodAirCheck()
    //{
    //    if (StretcherController == null || !NetworkManager.IsServer)
    //    {
    //        return;
    //    }
    //    if (StretcherController.GetNPCBG4State())
    //    {
    //        return;
    //    }
    //    if (VentilationManager != null)
    //    {
    //        if (VentilationManager.GetMineAtmosphere(transform.position, out _atmosphere))
    //        {
    //            //int co = (int)(Mathf.Round(_atmosphere.CarbonMonoxide * 1000000.0f));
    //            //if (co >= 50)
    //            //{
    //            //    //Run kill script on stretcher conroller
    //            //    //StretcherController.RequestSetBreathing(false);
    //            //    //_vRNBoolData.BoolData = false;
    //            //    //NetObj.SendMessage("BREATHING", _vRNBoolData);
    //            //    //SetAliveState(false);
    //            //    //Debug.Log($"Requesting set breathing to false at time {Time.time}");
    //            //    StretcherController.RequestSetBreathing(false);
    //            //}
    //            string desc;
    //            if(NPCTriggerDeath.CheckForDeath(transform.position, _atmosphere, out desc))
    //            {
    //                StretcherController.RequestSetBreathing(false);
    //                if (!_airCheckLogged)
    //                {
    //                    NetworkManager.LogSessionEvent(new VRNLogEvent
    //                    {
    //                        EventType = VRNLogEventType.Npcdeath,
    //                        Message = "",
    //                        Position = transform.position.ToVRNVector3(),
    //                        Rotation = transform.rotation.ToVRNQuaternion(),
    //                        ObjectName = gameObject.name,
    //                        PositionMetadata = desc,
    //                    });
    //                    //NetworkManager.LogSessionEvent(VRNLogEventType.Npcdeath, desc,
    //                    //    transform.position, transform.rotation, gameObject.name);
    //                    _airCheckLogged = true;
    //                }
    //            }
    //            else if(NPCTriggerDeath.CheckForUnconscious(transform.position, _atmosphere, out desc))
    //            {
    //                if (!_airCheckLogged)
    //                {
    //                    NetworkManager.LogSessionEvent(new VRNLogEvent
    //                    {
    //                        EventType = VRNLogEventType.Npcunconscious,
    //                        Message = "",
    //                        Position = transform.position.ToVRNVector3(),
    //                        Rotation = transform.rotation.ToVRNQuaternion(),
    //                        ObjectName = gameObject.name,
    //                        PositionMetadata = desc,
    //                    });
    //                    _airCheckLogged = true;
    //                }
    //            }
    //            else
    //            {
    //                _airCheckLogged = false;
    //            }
    //        }
    //    }
    //}

    public void SetAliveState(bool isAlive)
    {
        IsAlive = isAlive;
        PerformBreathing = isAlive;
        _hasBeenAssessed = false;

        StretcherAnimator.SetBool("Breathing", isAlive);
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

    public void OnActivated(Transform interactor)
    {
    }

    public void OnDeactivated(Transform interactor)
    {
    }

    
}
