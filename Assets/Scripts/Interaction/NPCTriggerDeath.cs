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

        if(_netObj == null)
        {
            TryGetComponent<NetworkedObject>(out _netObj);
        }

        if(_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        //do not immediately check - static gas zones may not have loaded until Start() is complete
        InvokeRepeating(nameof(CheckForDeath), 1.0f, 0.7f);
    }

    private void TeleportManager_AfterTeleport(Transform obj)
    {
        _deathEventLogged = false;
        _unconEventLogged = false;
        _timeout = Time.time;
    }

    private void CheckForDeath()
    {
        if ((NPCBehaviors != null && NPCBehaviors.HasBG4) || !IsActive)
            return;

        //only check on the client with network authority
        if (_netObj != null && !_netObj.HasAuthority)
            return;

        if (HasBG4 || Invulernable || VentilationManager == null)
            return;

        if (!VentilationManager.GetMineAtmosphere(transform.position + new Vector3(0, 0.2f, 0), out var atmo))
            return;

        int co = (int)(Mathf.Round(atmo.CarbonMonoxide * 1000000.0f));
        float oxygen = atmo.Oxygen * 100.0f;
       
        //CarbonMonoxideDisplay.text = _Atmosphere.CarbonMonoxide.ToString ("F1");

        if ((co > FatalCOLevel || oxygen < FatalO2Level))
        {
          
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

}
