using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.VFX;
using Google.Protobuf;
using System.Text;

[RequireComponent(typeof(NetworkedObject))]
public class FireInteraction : MonoBehaviour, INetSync, ISelectableObjectAction, ISelectableObjectInfo
{
    //public const float MinFireStrength = 0.1f;

    public VisualEffect FireEffect;
    public Light FireLight;
    public GameObject ExtinguishedPrefab;
    public PlayerManager PlayerManager;

    public AudioSource FireAudioSource;
    public AudioClip ExtinguishedClip;
    public AnimationCurve AnimationCurve;
    public bool FireExtinguishing = false;

    public float MaxHealth = 10.0f;

    [System.NonSerialized]
    public bool FireExtinguished = false;


    private float _flameRate;
    private float _sparkRate;
    private float _smokeRate;
    private float _lightIntensity;
    private float _progress = 1;
    private float _extinguishTimer = 0;
    private NetworkedObject _netObj;
    private NetworkManager NetworkManager;
    //private NetSyncFloatValue _netFloat;
    private float _fireHealth = 10.0f;

    private float _extinguishAmount = 0;
    //private float _extinguishedAt = -1;
    private float _startingVolume = 1;

    private MineFireHost _fireHost;
    private float _lastFireUpdate = -1;
    private float _lastNetSyncHealth = -1;
    private VRNFireStatus _fireStatus;
    private GameObject _extinguishedPrefabInstance;
    private int _lastDamagedByPlayerID = -1;

    public string SelectableActionName
    {
        get
        {
            if (FireExtinguished)
                return "Restart Fire";
            else
                return "Extinguish Fire";
        }
    }

    void Awake()
    {
        _fireStatus = new VRNFireStatus();
        _netObj = GetComponent<NetworkedObject>();
        _fireHost = GetComponent<MineFireHost>();
    }

    private void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        
        //_netFloat = GetComponent<NetSyncFloatValue>();
        _flameRate = FireEffect.GetFloat("Flames Rate");
        _sparkRate = FireEffect.GetFloat("Sparks Rate");
        _smokeRate = FireEffect.GetFloat("Smoke Rate");
        _lightIntensity = FireLight.intensity;

        //_maxHealth = _netFloat.ValueToSync;
        _fireHealth = MaxHealth;

        _netObj.RegisterMessageHandler(HandleNetObjMessage);
        _startingVolume = FireAudioSource.volume;
        if(PlayerManager == null)
        {
            PlayerManager = PlayerManager.GetDefault(gameObject);
        }
    }

    private void OnDestroy()
    {
        _netObj.UnregisterMessageHandler(HandleNetObjMessage);
    }

    private void HandleNetObjMessage(string messageType, CodedInputStream reader)
    {
        //Debug.Log($"Received NetObjMessage {messageType}");
        if (messageType == "DAMAGE_FIRE")
        {
            if (_netObj.HasAuthority)
            {
                //var msg = VRNMsgDamageFire.Parser.ParseDelimitedFrom(recvStream);
                var msg = new VRNMsgDamageFire();
                reader.ReadMessage(msg);

                //Debug.Log($"Damaging fire by {msg.DmgAmount}");
                //if (_fireHealth > 0)
                //    _fireHealth -= msg.DmgAmount;

                ApplyFireExtinguish(msg.DmgAmount, msg.PlayerID);

                //TO DO: Add a new type of log message that saves the fire's health?
            }
        }
    }

    private void ExtinguishFire()
    {
        FireExtinguished = true;
        _fireHealth = 0;
        FireLight.enabled = false;
        //_extinguishedAt = Time.time;

        //FireEffect.SetFloat("Flames Rate", 0);
        //FireEffect.SetFloat("Sparks Rate", 0);
        //FireEffect.SetFloat("Smoke Rate", 0);
        FireEffect.Stop();
        FireAudioSource.Stop();
        //FireAudioSource.clip = ExtinguishedClip;
        //FireAudioSource.loop = false;
        //FireAudioSource.Play();
        FireAudioSource.volume = 0.7f;
        FireAudioSource.PlayOneShot(ExtinguishedClip);

        if (_netObj.HasAuthority)
        {
            var player = PlayerManager.GetPlayer(_lastDamagedByPlayerID);

            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;
            int playerID = PlayerManager.CurrentPlayer.PlayerID;

            if (player != null)
            {
                if (player.HeadTransform != null)
                {
                    pos = player.HeadTransform.position;
                    rot = player.HeadTransform.rotation;
                }

                playerID = player.PlayerID;
            }

            Debug.Log($"FireInteraction: Logging fire extinguished by {playerID}");

            NetworkManager.LogSessionEvent(new VRNLogEvent
            {
                EventType = VRNLogEventType.FireExtinguished,
                ObjectName = "Fire",
                Position = pos.ToVRNVector3(),
                Rotation = rot.ToVRNQuaternion(),
                SourcePlayerID = playerID,
                PositionMetadata = "Extinguished"
            });
        }

        //for now disable extinguished audio/effect
        //SpawnExtinguishedPrefab();
        UpdateVentSimulation();

        //this.enabled = false;
    }

    private void RestartFire(float health)
    {
        //this.enabled = true;
        _fireHealth = health;
        FireLight.enabled = true;
        if (FireExtinguished)
        {
            FireEffect.Play();
            FireAudioSource.Play();
            FireExtinguished = false;
        }
        DestroyExtinguishedPrefab();

        UpdateVentSimulation();
    }

    private void SpawnExtinguishedPrefab()
    {
        if (ExtinguishedPrefab == null)
            return;

        if (_extinguishedPrefabInstance != null)
            DestroyExtinguishedPrefab();

        _extinguishedPrefabInstance = Instantiate<GameObject>(ExtinguishedPrefab, transform);
        _extinguishedPrefabInstance.transform.localPosition = Vector3.zero;
        _extinguishedPrefabInstance.transform.localRotation = Quaternion.identity;
    }

    private void DestroyExtinguishedPrefab()
    {
        if (_extinguishedPrefabInstance == null)
            return;

        Destroy(_extinguishedPrefabInstance);
        _extinguishedPrefabInstance = null;
    }

    private void UpdateVentSimulation()
    {
        var health = Mathf.Clamp(_fireHealth / MaxHealth, 0, 1.0f);
        if (_fireHost != null && _fireHost.VentFire != null)
        {
            var delta = Mathf.Abs(_lastFireUpdate - health);
            if (delta >= 0.1f)
            {
                _lastFireUpdate = health;

                var fireStrength = health;
                //if (fireStrength < MinFireStrength)
                //    fireStrength = MinFireStrength;

                _fireHost.VentFire.UpdateFireStrength(fireStrength);
                Debug.Log($"FireInteraction: Updating fire strength to {health:F2}");
            }
        }
    }

    private void Update()
    {
        if (FireExtinguished)
            return;

        var health = Mathf.Clamp(_fireHealth / MaxHealth, 0, 1.0f);
        _extinguishTimer = health;
        _progress = AnimationCurve.Evaluate(health);
        FireAudioSource.volume = Mathf.Clamp(_startingVolume * (_fireHealth / MaxHealth), 0, _startingVolume);
        //Debug.Log($"Fire Value:{_netFloat.ValueToSync:F1} health: {health:F1} prog:{_progress:F1}");

        UpdateVentSimulation();


        FireEffect.SetFloat("Flames Rate", _flameRate * (_progress));
        FireEffect.SetFloat("Sparks Rate", _sparkRate * (_progress));
        //FireEffect.SetFloat("Smoke Rate", _smokeRate * (_progress));
        FireLight.intensity = _lightIntensity * (_progress);

        if (_netObj.HasAuthority)
        {
            if (_fireHealth <= 0 && !FireExtinguished)
            {
                ExtinguishFire();
            }
        }

        //if (_netObj.HasAuthority)
        //{
        //    if (FireExtinguished && !FireAudioSource.isPlaying)
        //    {
        //        FireExtinguishing = false;

        //        if (Time.time - _extinguishedAt > 10.0f)
        //        {
        //            System.Guid guid = System.Guid.NewGuid();
        //            NetworkManager.SpawnObject("MineRescue/BonfireExtinguished", guid, transform.position, transform.rotation, true);
        //            NetworkManager.DestroyObject(_netObj.uniqueID);                    
        //        }
        //    }
        //}

        //if (FireExtinguishing)
        //{
        //    _extinguishAmount += Time.deltaTime;

        //    if (_netObj.HasAuthority)
        //    {
        //        //_netFloat.ValueToSync -= Time.deltaTime;
        //        _fireHealth -= Time.deltaTime;
        //        _playerID = PlayerManager.CurrentPlayer.PlayerID;
        //    }
        //    else
        //    {
        //        if (_extinguishAmount > 0.1f)
        //        {
        //            _extinguishAmount -= 0.1f;

        //            VRNMsgDamageFire msg = new VRNMsgDamageFire();
        //            msg.DmgAmount = 0.1f;
        //            //msg.PlayerID = PlayerManager.CurrentPlayer;
        //            _netObj.SendMessage("DAMAGE_FIRE", msg);
        //        }
        //    }
        //}

    }

    public void ApplyFireExtinguish(float extinguishAmount, int player = -1)
    {
        if (_netObj.HasAuthority)
        {
            //_netFloat.ValueToSync -= Time.deltaTime;
            _fireHealth -= extinguishAmount;
            _lastDamagedByPlayerID = player;
        }
        else
        {
            _extinguishAmount += extinguishAmount;

            if (_extinguishAmount > 0.1f)
            {
                _extinguishAmount -= 0.1f;

                VRNMsgDamageFire msg = new VRNMsgDamageFire();
                msg.DmgAmount = 0.1f;
                msg.PlayerID = PlayerManager.CurrentPlayer.PlayerID;

                _netObj.SendMessage("DAMAGE_FIRE", msg);
            }
        }

    }


    public void OnExtinguish()
    {

        FireExtinguishing = true;
    }

    public void StopExtinguish()
    {
        FireExtinguishing = false;
    }

    public bool NeedsUpdate()
    {
        if (_lastNetSyncHealth != _fireHealth)
            return true;
        else
            return false;
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        _fireStatus.FireHealth = _fireHealth;
        _fireStatus.PlayerID = _lastDamagedByPlayerID;
        //_fireStatus.WriteDelimitedTo(writer);
        writer.WriteMessage(_fireStatus);

        _lastNetSyncHealth = _fireHealth;
    }

    public void SyncObjState(CodedInputStream reader)
    {
        _fireStatus.FireHealth = 0;
        _lastDamagedByPlayerID = -1;
        //_fireStatus.MergeDelimitedFrom(reader);
        reader.ReadMessage(_fireStatus);

        _fireHealth = _fireStatus.FireHealth;
        _lastDamagedByPlayerID = _fireStatus.PlayerID;

        if (FireExtinguished && _fireHealth > 0)
            RestartFire(_fireHealth);

        if (_fireHealth <= 0 && !FireExtinguished)
        {
            ExtinguishFire();
        }
    }

    public void PerformSelectableObjectAction()
    {
        if (_netObj == null)
            return;

        if (!_netObj.HasAuthority)
            _netObj.RequestOwnership();

        if (FireExtinguished)
            RestartFire(MaxHealth);
        else
            ExtinguishFire();
    }

    public void GetObjectInfo(StringBuilder sb)
    {
        sb.AppendLine($"Fire Health: {_fireHealth}");
    }

    public float GetFireHealth()
    {
        return _fireHealth;
    }

    public float GetFirePercentage()
    {
        return _fireHealth / MaxHealth;
    }
}
