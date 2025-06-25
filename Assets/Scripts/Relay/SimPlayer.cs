using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimPlayer : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;
    public bool SendVRData = true;
    public float UpdateRate = 20.0f;

    public Bounds RigBounds;
    public Bounds ControllerBounds;
    public Bounds HMDBounds;

    public float TimeDivisor = 20.0f;

    private float _lastVRUpdate;
    private VRNVRPlayerInfo _vrPlayerInfo;
    private VRNCalibrationOffsetData _calOffset;

    private List<VRNPlayerRole> _possibleRoles;

    private float _randomSeed = 0;
    

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        NetworkManager.ClientIDAssigned += OnClientIDAssigned;
        PlayerManager.PlayerIDAssigned += OnPlayerIDAssigned;

        _randomSeed = UnityEngine.Random.Range(0.0f, 100.0f);
        //SceneLoadManager.EnteredSimulationScene += OnEnteredSimulationScene;

        _possibleRoles = new List<VRNPlayerRole>();
        _possibleRoles.Add(VRNPlayerRole.Alt1);
        _possibleRoles.Add(VRNPlayerRole.Alt2);
        _possibleRoles.Add(VRNPlayerRole.Alt3);
        _possibleRoles.Add(VRNPlayerRole.Alt4);
        _possibleRoles.Add(VRNPlayerRole.GasMan);
        _possibleRoles.Add(VRNPlayerRole.SecondGasMan);
        _possibleRoles.Add(VRNPlayerRole.TailCaptain);

        
        _vrPlayerInfo = new VRNVRPlayerInfo();
        _vrPlayerInfo.Head = new VRNTransformData
        {
            Position = new Vector3(1, 2, 3).ToVRNVector3(),
            Rotation = UnityEngine.Random.rotation.ToVRNQuaternion(),
        };

        _vrPlayerInfo.RigOffset = new VRNTransformData
        {
            Position = new Vector3(1, 2, 3).ToVRNVector3(),
            Rotation = UnityEngine.Random.rotation.ToVRNQuaternion(),
        };


        _vrPlayerInfo.LeftController = new VRNTransformData
        {
            Position = new Vector3(-3, 1, 7).ToVRNVector3(),
            Rotation = UnityEngine.Random.rotation.ToVRNQuaternion(),
        };

        _vrPlayerInfo.RightController = new VRNTransformData
        {
            Position = new Vector3(-3, 1, 7).ToVRNVector3(),
            Rotation = UnityEngine.Random.rotation.ToVRNQuaternion(),
        };

        var role = UnityEngine.Random.Range(0, _possibleRoles.Count - 1);
        _vrPlayerInfo.RightControllerTracked = true;
        _vrPlayerInfo.LeftControllerTracked = true;
        //_vrPlayerInfo.Role = _possibleRoles[role];
        _vrPlayerInfo.Role = PlayerManager.CurrentPlayer.PlayerRole;
        _vrPlayerInfo.Name = NetworkManager.GetMultiplayerName();

        if (NetworkManager.ClientConnected)
        {
            //we already have a client ID
            OnClientIDAssigned(NetworkManager.ClientID);
        }


    }

    void RandomizePositions()
    {
        var range = new Vector3(360, 360, 360);
        _vrPlayerInfo.Head.Position = RandomPosition(1.0f, HMDBounds).ToVRNVector3();
        
        _vrPlayerInfo.Head.Rotation = RandomRotation(1.0f, new Vector3(0,360,0)).ToVRNQuaternion();

        _vrPlayerInfo.LeftController.Position = RandomPosition(2.0f, ControllerBounds).ToVRNVector3();
        _vrPlayerInfo.LeftController.Rotation = RandomRotation(2.0f, range).ToVRNQuaternion();

        _vrPlayerInfo.RightController.Position = RandomPosition(3.0f, ControllerBounds).ToVRNVector3();
        _vrPlayerInfo.RightController.Rotation = RandomRotation(3.0f, range).ToVRNQuaternion();

        _vrPlayerInfo.RigOffset.Position = RandomPosition(4.0f, RigBounds).ToVRNVector3();
        _vrPlayerInfo.RigOffset.Rotation = Quaternion.identity.ToVRNQuaternion();

    }

    Vector3 RandomPerlinVector3(float x, float y, Vector3 scale)
    {
        x = x + _randomSeed;
        float vx = Mathf.PerlinNoise(x, y);
        float vy = Mathf.PerlinNoise(x, y + 7.1f);
        float vz = Mathf.PerlinNoise(x, y + 8.2f);

        Vector3 v = new Vector3(vx, vy, vz);
        v.Scale(scale);

        return v;
    }

    Vector3 RandomPosition(float y, Bounds b)
    {
        float x = Time.time / TimeDivisor;
        var v = RandomPerlinVector3(x, y, b.size);

        v = v + b.min;
        return v;
    }

    Quaternion RandomRotation(float y, Vector3 range)
    {
        float x = Time.time / TimeDivisor;
        var v = RandomPerlinVector3(x, y, range);

        return Quaternion.Euler(v);
    }

    private void OnClientIDAssigned(int clientID)
    {
        Debug.Log($"SimPlayer: Client ID Assigned: {PlayerManager.CurrentPlayer.Name}:{clientID}");
        if (PlayerManager.CurrentPlayer.PlayerID < 0)
        {
            PlayerManager.RequestPlayerID();
        }
    }

    private void OnPlayerIDAssigned(int playerID)
    {
        Debug.Log($"SimPlayer: Player ID Assigned: {PlayerManager.CurrentPlayer.Name}:{playerID}");
    }

    private void Update()
    {
        float elapsed = Time.time - _lastVRUpdate;
        float delay = 1.0f / UpdateRate;

        if (elapsed > delay && NetworkManager.IsInGame && PlayerManager.CurrentPlayer.PlayerID >= 0)
        {
            _lastVRUpdate = Time.time;

            _vrPlayerInfo.PlayerID = PlayerManager.CurrentPlayer.PlayerID;
            _vrPlayerInfo.ClientID = NetworkManager.ClientID;
            _vrPlayerInfo.PlayerHeight = 1.6f;
            _vrPlayerInfo.Role = PlayerManager.CurrentPlayer.PlayerRole;

            RandomizePositions();

            NetworkManager.SendNetMessage(VRNPacketType.VrplayerInfo, _vrPlayerInfo, reliable: false);
            PlayerManager.UpdateLocalVRClientData(_vrPlayerInfo);
        }
    }

}
