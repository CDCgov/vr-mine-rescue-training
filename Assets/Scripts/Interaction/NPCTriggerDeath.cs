using BAHDOL;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCTriggerDeath : MonoBehaviour
{

    public const int FatalCOLevel = 50;
    public const float FatalO2Level = 12.0f;
    public const float UnconsciousO2Level = 19.5f;

    public NetworkManager NetworkManager;

    public BAHDOL.NPC_Animator NPCAnimator;
    public VentilationManager VentilationManager;
    public NPCController NPCBehaviors;

    public event Action OnDeath;
    public event Action OnUnconscious;
    public GameObject RagdollParent;
    public bool Invulernable = false;
    public bool HasBG4 = false;
    public string NPCName = "";
    public float EventTimeout = 10;
    public TeleportManager TeleportManager;
    public bool IsActive = true;
    private float _timeout = 0;

  
    //private MineAtmosphere _atmosphere;
    
    private CustomXRInteractable _xrInteractable;
    public Animator _animator;
    private NetworkedObject _netObj;
    private bool _deathEventLogged = false;
    private bool _unconEventLogged = false;
    

    
    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);


        if (TeleportManager == null)
        {
            TeleportManager = TeleportManager.GetDefault(gameObject);
        }
        TeleportManager.AfterTeleport += TeleportManager_AfterTeleport;
        if (NPCBehaviors == null)
            TryGetComponent<NPCController>(out NPCBehaviors);

        //if (VentilationManager != null)
        //    VentilationManager.ReceivedMineAtmosphere += OnReceivedMineAtmosphere;

        if(_netObj == null)
        {
            //_netObj = GetComponent<NetworkedObject>();
            TryGetComponent<NetworkedObject>(out _netObj);
        }
        if (NPCAnimator == null)
        {
            //NPCAnimator = GetComponent<BAHDOL.NPC_Animator>();
            TryGetComponent<BAHDOL.NPC_Animator>(out NPCAnimator);
        }

        if(_xrInteractable == null)
        {
            //_xrInteractable = GetComponent<CustomXRInteractable>();
            TryGetComponent<CustomXRInteractable>(out _xrInteractable);
        }

        if(_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }


        if(_xrInteractable != null && _animator != null)
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Dead") || _animator.GetCurrentAnimatorStateInfo(0).IsName("Dying"))
            {
                _xrInteractable.enabled = false;
            }
        }

        InvokeRepeating("GasUpdate", 0.0f, 0.7f);
    }

    private void TeleportManager_AfterTeleport(Transform obj)
    {
        _deathEventLogged = false;
        _unconEventLogged = false;
        _timeout = Time.time;
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.KeypadEnter))
    //    {
    //        Vector3 pos = Vector3.Normalize(transform.position + transform.forward + transform.up);
    //        Kill();
    //        Explode(pos, 100000, 10);            
    //    }
    //}

    //private void OnReceivedMineAtmosphere(Vector3 worldPos, MineAtmosphere atm)
    //{
    //    if (!gameObject)
    //        return;

    //    if (Vector3.Distance(worldPos, transform.position) < 2.0f)
    //    {
    //        _atmosphere = atm;
    //        CheckForDeath();
    //    }
    //}

    private void GasUpdate()
    {
        CheckForDeath();
        //if (VentilationManager != null)
        //{
        //    //VentilationManager.RequestMineAtmosphere(transform.position);

        //    MineAtmosphere atm;
        //    if (VentilationManager.GetMineAtmosphere(transform.position, out atm))
        //    {
        //        //_atmosphere = atm;
        //        CheckForDeath();
        //    }
        //}
    }

    private void OnDestroy()
    {
        //if (VentilationManager != null)
        //    VentilationManager.ReceivedMineAtmosphere -= OnReceivedMineAtmosphere;
    }


    //public void Explode(Vector3 position, float force, float radius)
    //{
    //    Ragdoll(true);
    //    foreach (var rb in _ragdollBodies)
    //    {
    //        rb.AddExplosionForce(force, position, radius);
    //    }
    //}

    private void CheckForDeath()
    {
        if ((NPCBehaviors != null && NPCBehaviors.HasBG4) || !IsActive)
            return;


        if (HasBG4)
        {
            return;
        }
        if (VentilationManager == null)
            return;

        if (!VentilationManager.GetMineAtmosphere(transform.position + new Vector3(0, 0.2f, 0), out var atmo))
            return;

        if (Invulernable)
        {
            return;
        }
        int co = (int)(Mathf.Round(atmo.CarbonMonoxide * 1000000.0f));
        float oxygen = atmo.Oxygen * 100.0f;
       
        //CarbonMonoxideDisplay.text = _Atmosphere.CarbonMonoxide.ToString ("F1");

        if ((co > FatalCOLevel || oxygen < FatalO2Level))
        {
            //NPCAnimator.TransitionToAnimation(BAHDOL.AnimationState.Dying);
            ////NPCAnimator.JumpToAnimationState(BAHDOL.AnimationState.Dying);

            //if (NPCBehaviors != null)
            //{
            //    if (_netObj != null && _netObj.HasAuthority)
            //    {
            //        OnDeath.Invoke();
            //        NPCBehaviors.NPCDeath();
            //    }                
            //}
            //else
            //{
            //    if (_netObj != null && _netObj.HasAuthority)
            //    {
            //        OnDeath.Invoke();

            //    }
            //}
            //if (_xrInteractable != null)
            //{
            //    _xrInteractable.enabled = false;
            //}
            if (_deathEventLogged || Time.time < _timeout)
            {
                return;
            }
            if (NPCBehaviors != null)
            {
                NPCBehaviors.Kill();
                NPCName = NPCBehaviors.NPCName;
            }
            else if(_animator != null)
            {
                _animator.SetBool("Breathing", false);
                Debug.Log($"Animator should have set breathing to false!");
            }

            if (string.IsNullOrEmpty(NPCName))
            {
                NPCName = gameObject.name;
            }
            string causeOfDeath = "";
            if(co > FatalCOLevel)
            {
                causeOfDeath += $"High CO: {co}ppm";
            }
            if (oxygen < FatalO2Level)
            {
                if (!string.IsNullOrEmpty(causeOfDeath))
                {
                    causeOfDeath += ", ";
                }
                causeOfDeath += $"Criticaly Low Oxygen: {oxygen.ToString("00")}";
            }
            if (NetworkManager.IsServer)
            {
                NetworkManager.LogSessionEvent(new VRNLogEvent
                {
                    EventType = VRNLogEventType.Npcdeath,
                    Position = transform.position.ToVRNVector3(),
                    Rotation = transform.rotation.ToVRNQuaternion(),
                    ObjectName = NPCName,
                    SourcePlayerID = -1,
                    PositionMetadata = causeOfDeath,
                });
            }
            //NetworkManager.LogSessionEvent(VRNLogEventType.Npcdeath, causeOfDeath,
            //        transform.position, transform.rotation, gameObject.name);
            _deathEventLogged = true;
            _timeout = Time.time + EventTimeout;
            OnDeath?.Invoke();
        }
        else if (oxygen < UnconsciousO2Level)
        {
            //if (!_endangermentFlag)
            //{
            //    return;
            //}
            if(Time.time < _timeout)
            {
                return;
            }
            if (NPCBehaviors != null)
            {
                NPCBehaviors.SetUnconscious();
                NPCName = NPCBehaviors.NPCName;
            }
            
            if(string.IsNullOrEmpty(NPCName)) 
            { 
                NPCName = gameObject.name; 
            }
            //if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Unconscious"))
            //TODO separate out unconscious endangerment flag and death endangerment flag. For case where you go from Unconscious zone straight to death zone
            if (!_unconEventLogged && NetworkManager.IsServer)
            {

                NetworkManager.LogSessionEvent(new VRNLogEvent
                {
                    EventType = VRNLogEventType.Npcunconscious,
                    Position = transform.position.ToVRNVector3(),
                    Rotation = transform.rotation.ToVRNQuaternion(),
                    ObjectName = NPCName,
                    SourcePlayerID = -1,
                    PositionMetadata = $"Low Oxygen: {oxygen.ToString("00")}",
                });
                //NetworkManager.LogSessionEvent(VRNLogEventType.Npcunconscious, $"Low Oxygen: {oxygen}",
                //        transform.position, transform.rotation, gameObject.name);
                _unconEventLogged = true;
                OnUnconscious?.Invoke();
                _timeout = Time.time + EventTimeout;
            }
        }
        else
        {
            _deathEventLogged = false;
            _unconEventLogged = false;
        }
    }

    //[Obsolete]
    //public static bool CheckForDeath(Vector3 pos, MineAtmosphere atmo, out string desc, bool IsInvulnerable = false)
    //{
    //    //if (NPCBehaviors != null && NPCBehaviors.HasBG4)
    //    //    return;

    //    //if (VentilationManager == null)
    //    //    return;

    //    //if (!VentilationManager.GetMineAtmosphere(transform.position, out var atmo))
    //    //    return;
    //    desc = string.Empty;
    //    if (IsInvulnerable)
    //    {
    //        desc = "Invulnerable";
    //        return false;
    //    }
    //    int co = (int)(Mathf.Round(atmo.CarbonMonoxide * 1000000.0f));
    //    float oxygen = atmo.Oxygen * 100.0f;

    //    //CarbonMonoxideDisplay.text = _Atmosphere.CarbonMonoxide.ToString ("F1");

    //    if ((co > FatalCOLevel || oxygen < FatalO2Level))
    //    {
    //        //Log it?
    //        if (co < FatalCOLevel)
    //        {
    //            desc = $"High CO: {co}ppm";
    //        }
    //        if (oxygen < FatalO2Level)
    //        {
    //            if (!string.IsNullOrEmpty(desc))
    //            {
    //                desc += ", ";
    //            }
    //            desc += $"Critically Low Oxygen: {oxygen}%";
    //        }
    //        return true;
    //    }

    //    return false;
    //}
    //[Obsolete]
    //public static bool CheckForUnconscious(Vector3 pos, MineAtmosphere atmo, out string desc, bool IsInvulnerable = false)
    //{
    //    desc = string.Empty;
    //    if (IsInvulnerable)
    //    {
    //        desc = "Invulnerable";
    //        return false;
    //    }
    //    float oxygen = atmo.Oxygen * 100.0f;

    //    if (oxygen < UnconsciousO2Level)
    //    {
    //        //Log it?
    //        desc = $"Low Oxygen: {oxygen}%";
    //        return true;
    //    }

    //    return false;
    //}
}
