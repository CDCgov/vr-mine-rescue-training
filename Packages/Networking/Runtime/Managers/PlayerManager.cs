//using LiteNetLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Google.Protobuf;

public struct ObjectPositionData
{
    public GameObject Object;
    public Vector3 Position;
    public Quaternion Rotation;
}


public enum PlayerDominantHand
{
    RightHanded,
    LeftHanded
}

public class PlayerRepresentation : ISessionTimeSeriesData<PlayerRepresentation>
{
    public int ClientID;
    public int PlayerID;

    public const float BasePlayerHeight = 1.58f; //was 1.58f; changed to 1.70f; should be height of eyes of player model

    public string Name
    {
        get { return _playerName; }
        set
        {
            if (_playerName != value)
            {
                _playerName = value;
                PlayerNameChanged?.Invoke(value);
            }
        }
    }

    public VRNPlayerRole PlayerRole
    {
        get { return _playerRole; }
        set
        {
            if (_playerRole != value)
            {
                _playerRole = value;
                PlayerRoleChanged?.Invoke(this, value);
            }
        }
    }

    public PlayerDominantHand PlayerDominantHand
    {
        get { return _playerHandedness; }
        set
        {
            if (_playerHandedness != value)
            {
                _playerHandedness = value;
                PlayerHandednessChanged?.Invoke(value);
            }
        }
    }

    public Color PlayerColor { 
        get { return _playerColor; }

        set
        {
            _playerColor = value;
            PlayerColorChanged?.Invoke(value);
        } 
    }

    public bool OnLinkLine
    {
        get { return _onLinkLine; }
        set
        {
            if (_onLinkLine != value)
            {
                _onLinkLine = value;
                OnLinkLineChanged?.Invoke(value);
            }
        }
    }

    public string AntilatencyStatusText
    {
        get { return _antilatencyStatusText; }
        set
        {
            if (_antilatencyStatusText != value)
            {
                _antilatencyStatusText = value;
                AntilatencyStatusTextChanged?.Invoke(value);
            }
        }
    }

    public GameObject PlayerObject
    {
        get { return _playerObject; }
        set
        {
            if (_playerObject != value)
            {
                _playerObject = value;
                PlayerObjectChanged?.Invoke(value);
            }
        }
    }

    public bool DebugInterfaceEnabled
    {
        get { return _debugInterfaceEnabled; }
        set
        {
            if (_debugInterfaceEnabled != value)
            {
                _debugInterfaceEnabled = value;
                DebugInterfaceEnabledChanged?.Invoke(value);
            }
        }
    }

    public bool MuteEnabled
    {
        get { return _muteEnabled; }
        set
        {
            if(_muteEnabled != value)
            {
                _muteEnabled = value;
                MuteEnabledChanged?.Invoke(value);
            }
        }
    }

    public bool DeafenEnabled
    {
        get { return _deafenEnabled; }
        set
        {
            if(_deafenEnabled != value)
            {
                _deafenEnabled = value;
                DeafenEnabledChanged?.Invoke(value);
            }
        }
    }

    public bool RotationEnabled
    {
        get { return _rotationEnabled; }
        set
        {
            if (_rotationEnabled != value)
            {
                _rotationEnabled = value;
                RotationEnabledChanged?.Invoke(value);
            }
        }
    }

    public bool TranslationEnabled
    {
        get { return _translationEnabled; }
        set
        {
            if (_translationEnabled != value)
            {
                _translationEnabled = value;
                TranslationEnabledChanged?.Invoke(value);
            }
        }
    }

    public bool UserTeleportEnabled
    {
        get { return _userTeleportEnabled; }
        set
        {
            if(_userTeleportEnabled != value)
            {
                _userTeleportEnabled = value;
                UserTeleportEnabledChanged?.Invoke(value);
            }
        }
    }

    public float PlayerHeight
    {
        get { return _PlayerHeight; }
        set
        {
            _PlayerHeight = value;
            PlayerHeightChanged?.Invoke(value);
        }
    }

    public int PlayerMode
    {
        get { return _PlayerMode; }
        set
        {
            _PlayerMode = value;
            PlayerModeChanged?.Invoke(value);
        }
    }

    public bool LeftGrip
    {
        get { return _lGrip; }
        set
        {
            _lGrip = value;
            PlayerLGripChanged?.Invoke(value);
        }
    }

    public bool RightGrip
    {
        get { return _rGrip; }
        set
        {
            _rGrip = value;
            PlayerRGripChanged?.Invoke(value);
        }
    }

    public bool LeftControllerTracked
    {
        get { return _leftControllerTracked; }
        set
        {
            if (_leftControllerTracked == value)
                return;

            _leftControllerTracked = value;
            PlayerControllerTrackedChanged?.Invoke();
        }
    }

    public bool RightControllerTracked
    {
        get { return _rightControllerTracked; }
        set
        {
            if (_rightControllerTracked == value)
                return;

            _rightControllerTracked = value;
            PlayerControllerTrackedChanged?.Invoke();
        }
    }

    public string BatteryStatus
    {
        get { return _batteryStatus; }
        set
        {
            if (_batteryStatus == value)
                return;

            _batteryStatus = value;
            BatteryStatusChanged?.Invoke();

        }
    }

    public float FrameRate
    {
        get { return _frameRate; }
        set
        {
            if (_frameRate == value)
                return;

            _frameRate = value;
            FrameRateChanged?.Invoke();
        }
            
    }

    public VRNClientState ClientState
    {
        get { return _clientState; }
        set
        {
            _clientState = value;
            ClientStateChanged?.Invoke(value);
        }
    }

    public Vector3 CalTestPoint
    {
        get { return _calTestPoint; }
        set
        {
            if (_calTestPoint != value)
            {
                _calTestPoint = value;
                CalTestPointChanged?.Invoke(value);
            }
        }
    }

    public bool AppFocused;
    public bool AppPaused;

    public float LastDataTimestamp;
    public ObjectPositionData Head;
    public ObjectPositionData LeftController;
    public ObjectPositionData RightController;
    public ObjectPositionData RigOffset;

    public Vector3 CalibrationPos;
    public Quaternion CalibrationRot = Quaternion.identity;
    public float CalReceiveTimestamp = 0;
    public string SpawnedPrefabID;

    private float _PlayerHeight;
    private int _PlayerMode;


    public Transform HeadTransform;
    public Transform CalibrationTransform;
    public Transform RigTransform;

    public Vector3 SmoothDampVelocity = Vector3.zero;

    public event Action<Color> PlayerColorChanged;
    public event Action<PlayerRepresentation, VRNPlayerRole> PlayerRoleChanged;
    public event Action<bool> OnLinkLineChanged;
    public event Action<string> PlayerNameChanged;
    public event Action<string> AntilatencyStatusTextChanged;
    public event Action<GameObject> PlayerObjectChanged;
    public event Action<PlayerDominantHand> PlayerHandednessChanged;
    public event Action<bool> DebugInterfaceEnabledChanged;
    public event Action<bool> RotationEnabledChanged;
    public event Action<bool> TranslationEnabledChanged;
    public event Action<bool> UserTeleportEnabledChanged;
    public event Action<bool> MuteEnabledChanged;
    public event Action<bool> DeafenEnabledChanged;
    public event Action<VRNClientState> ClientStateChanged;
    public event Action PlayerVisualUpdated;
    public event Action<float> PlayerHeightChanged;
    public event Action<int> PlayerModeChanged;
    public event Action<bool> PlayerLGripChanged;
    public event Action<bool> PlayerRGripChanged;
    public event Action PlayerControllerTrackedChanged;
    public event Action BatteryStatusChanged;
    public event Action FrameRateChanged;
    public event Action<Vector3> CalTestPointChanged;

    private Color _playerColor = Color.white;
    private VRNPlayerRole _playerRole = VRNPlayerRole.UnknownRole;
    private bool _onLinkLine = false;
    private string _playerName = null;
    private string _antilatencyStatusText;
    private GameObject _playerObject;
    private PlayerDominantHand _playerHandedness;
    private bool _debugInterfaceEnabled;
    private bool _rotationEnabled;
    private bool _translationEnabled;
    private bool _userTeleportEnabled;
    private bool _muteEnabled;
    private bool _deafenEnabled;
    private bool _lGrip = false;
    private bool _rGrip = false;
    private bool _leftControllerTracked = false;
    private bool _rightControllerTracked = false;
    private VRNClientState _clientState;
    private float _frameRate;
    private string _batteryStatus;
    private Vector3 _calTestPoint;
    private Vector3[] _controllerStability;
    private int _stabilityIndex = 0;

    public void FillVRNVRPlayerInfo(VRNVRPlayerInfo info)
    {
        if (info.Head == null)
            info.Head = new VRNTransformData();
        if (info.LeftController == null)
            info.LeftController = new VRNTransformData();
        if (info.RightController == null)
            info.RightController = new VRNTransformData();

        info.ClientID = ClientID;
        info.PlayerID = PlayerID;
        info.Name = Name;
        info.Role = PlayerRole;
        info.PlayerHeight = PlayerHeight;
        info.PlayerMode = PlayerMode;
        info.OnLinkLine = OnLinkLine;

        info.Head.Position = Head.Position.ToVRNVector3();
        info.Head.Rotation = Head.Rotation.ToVRNQuaternion();

        info.LeftController.Position = LeftController.Position.ToVRNVector3();
        info.LeftController.Rotation = LeftController.Rotation.ToVRNQuaternion();
        info.LeftGrip = LeftGrip;

        info.RightController.Position = RightController.Position.ToVRNVector3();
        info.RightController.Rotation = RightController.Rotation.ToVRNQuaternion();
        info.RightGrip = RightGrip;

        info.RigOffset.Position = RigOffset.Position.ToVRNVector3();
        info.RigOffset.Rotation = RigOffset.Rotation.ToVRNQuaternion();
    }

    public void ProcessPlayerMessage(VRNPlayerMessage msg)
    {
        if (msg.PlayerID != PlayerID)
            return;

        switch (msg.MessageType)
        {
            case VRNPlayerMessageType.PmToggleDebugMode:
                DebugInterfaceEnabled = msg.BoolData;
                break;

            case VRNPlayerMessageType.PmToggleTranslation:
                TranslationEnabled = msg.BoolData;
                break;

            case VRNPlayerMessageType.PmToggleRotation:
                RotationEnabled = msg.BoolData;
                break;
            case VRNPlayerMessageType.PmToggleUserTeleport:
                UserTeleportEnabled = msg.BoolData;
                break;
            case VRNPlayerMessageType.PmSetPlayerHeight:
                PlayerHeight = msg.FloatData;
                break;
            case VRNPlayerMessageType.PmSetPlayerMode:
                PlayerMode = msg.IntData;
                break;
            case VRNPlayerMessageType.PmLGripOn:
                LeftGrip = msg.BoolData;
                break;
            case VRNPlayerMessageType.PmRGripOn:
                RightGrip = msg.BoolData;
                break;
            case VRNPlayerMessageType.PmRespawnEquipment:
                break;
        }
    }

    public void InvokePlayerVisualUpdated()
    {
        PlayerVisualUpdated?.Invoke();
    }

    public float ComputeHeight()
    {
        //return RigTransform.InverseTransformPoint(HeadTransform.position).y;
        return Head.Position.y + CalibrationPos.y;
    }

    public bool IsBatteryLow()
    {
        if (_batteryStatus == null || _batteryStatus.Length <= 0)
            return false; //no battery information 

        var bats = _batteryStatus.Split(' ');
        float batLevel;

        foreach (var bat in bats)
        {
            if (float.TryParse(bat, out batLevel))
            {
                //Debug.Log($"LowBat Checking \"{bat}\"");
                if (batLevel < 40.0f)
                    return true;
            }
        }

        return false;
    }

    public bool IsBatteryCritical()
    {
        if (_batteryStatus == null || _batteryStatus.Length <= 0)
            return false; //no battery information 

        var bats = _batteryStatus.Split(' ');
        float batLevel;

        foreach (var bat in bats)
        {
            if (float.TryParse(bat, out batLevel))
            {
                //Debug.Log($"LowBat Checking \"{bat}\"");
                if (batLevel < 20.0f)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// check if the player has both controllers visible
    /// possibly add additional checks to determine if they are active and stationary
    /// </summary>
    /// <returns></returns>
    public bool CheckCalibrationReady(float angleThreshold = 45.0f)
    {
        float leftAngle, rightAngle;
        ComputeControllerAngles(out leftAngle, out rightAngle);

        if (leftAngle < angleThreshold && rightAngle < angleThreshold)
        {
            return true;
        }
        else
            return false;
    }

    public void ComputeControllerAngles(out float leftAngle, out float rightAngle)
    {
        var head = Head.Position;
        var leftController = LeftController.Position;
        var rightController = RightController.Position;

        //compute forward vector for head transform
        var forward = Head.Rotation * Vector3.forward;

        //compute vectors from head position to controller positions
        leftController = leftController - head;
        rightController = rightController - head;

        //compute angle between HMD look direction (forward vector) and each controller
        leftAngle = Vector3.Angle(forward, leftController);
        rightAngle = Vector3.Angle(forward, rightController);
    }

    public Vector3 GetRightControllerPOISpace()
    {
        Vector3 poiSpace = Vector3.zero;
        if (RigTransform != null && RigTransform.parent != null)
        {
            var xform = RigTransform.parent;

            poiSpace = xform.InverseTransformPoint(RightController.Object.transform.position);
        }
        return poiSpace;
    }

    public void GetRightControllerStability(out float maxDist)
    {
        maxDist = -1;
        if (_controllerStability == null || _controllerStability.Length <= 0)
            return;

        Vector3 mean = Vector3.zero;
        for (int i = 0; i < _controllerStability.Length; i++)
        {
            mean += _controllerStability[i];
        }

        mean /= _controllerStability.Length;

        maxDist = 0;
        for (int i = 0; i < _controllerStability.Length; i++)
        {
            float dist = Vector3.Distance(mean, _controllerStability[i]);
            if (dist > maxDist)
                maxDist = dist;
        }

        //convert to mm
        maxDist = maxDist * 1000.0f;
    }

    public void SampleControllerStability(Vector3 pos, int numSamplePoints = 40)
    {
        if (_controllerStability == null)
        {
            _controllerStability = new Vector3[numSamplePoints];
            _stabilityIndex = -1;
            for (int i = 0; i < _controllerStability.Length; i++)
            {
                _controllerStability[i] = Vector3.zero;
            }
        }

        _stabilityIndex++;
        if (_stabilityIndex >= _controllerStability.Length)
            _stabilityIndex = 0;

        _controllerStability[_stabilityIndex] = pos;
    }

    private void SampleControllerStability(ObjectPositionData data)
    {
        SampleControllerStability(data.Position);
    }

    public void SaveCalTestPosition(Vector3 pos)
    {
        CalTestPoint = pos;
    }

    public void Interpolate(PlayerRepresentation next, float interp, ref PlayerRepresentation result)
    {
        result.PlayerID = PlayerID;
        result.ClientID = ClientID;

        result.Head = InterpolateObjPosData(Head, next.Head, interp);
        result.LeftController = InterpolateObjPosData(LeftController, next.LeftController, interp);
        result.RightController = InterpolateObjPosData(RightController, next.RightController, interp);
        result.RigOffset = InterpolateObjPosData(RigOffset, next.RigOffset, interp);

        result.CalibrationPos = Vector3.Lerp(CalibrationPos, next.CalibrationPos, interp);
        result.CalibrationRot = Quaternion.Lerp(CalibrationRot, next.CalibrationRot, interp);
        result._PlayerHeight = Mathf.Lerp(_PlayerHeight, next._PlayerHeight, interp);
        result._PlayerMode = PlayerMode;

        result._playerColor = _playerColor;
        result._playerRole = _playerRole;
        result._onLinkLine = _onLinkLine;
        result._playerName = _playerName;
        result._antilatencyStatusText = _antilatencyStatusText;
        result._playerHandedness = _playerHandedness;
        result._debugInterfaceEnabled = _debugInterfaceEnabled;
        result._rotationEnabled = _rotationEnabled;
        result._translationEnabled = _translationEnabled;
        result._userTeleportEnabled = _userTeleportEnabled;
        result._muteEnabled = _muteEnabled;
        result._deafenEnabled = _deafenEnabled;
        result._lGrip = _lGrip;
        result._rGrip = _rGrip;
        result._clientState = _clientState;
    }

    private ObjectPositionData InterpolateObjPosData(ObjectPositionData data1, ObjectPositionData data2, float interp)
    {
        ObjectPositionData result = new ObjectPositionData();
        result.Object = data1.Object;

        result.Position = Vector3.Lerp(data1.Position, data2.Position, interp);
        result.Rotation = Quaternion.Lerp(data1.Rotation, data2.Rotation, interp);

        return result;
    }

    public void CopyTo(PlayerRepresentation dest)
    {
        //dest.Head = Head;
        //dest.LeftController = LeftController;
        //dest.RightController = RightController;
        //dest.RigOffset = RigOffset;

        dest.PlayerID = PlayerID;
        dest.ClientID = ClientID;

        CopyObjectPositionData(Head, ref dest.Head);
        CopyObjectPositionData(LeftController, ref dest.LeftController);
        CopyObjectPositionData(RightController, ref dest.RightController);
        CopyObjectPositionData(RigOffset, ref dest.RigOffset);

        dest.CalibrationPos = CalibrationPos;
        dest.CalibrationRot = CalibrationRot;
        dest._PlayerHeight = _PlayerHeight;
        dest._PlayerMode = _PlayerMode;

        dest.PlayerColor = _playerColor;
        dest.PlayerRole = _playerRole;
        dest.OnLinkLine = _onLinkLine;
        dest.Name = _playerName;
        dest._antilatencyStatusText = _antilatencyStatusText;
        dest._playerHandedness = _playerHandedness;
        dest._debugInterfaceEnabled = _debugInterfaceEnabled;
        dest._rotationEnabled = _rotationEnabled;
        dest._translationEnabled = _translationEnabled;
        dest._userTeleportEnabled = _userTeleportEnabled;
        dest._muteEnabled = _muteEnabled;
        dest._deafenEnabled = _deafenEnabled;
        dest._lGrip = _lGrip;
        dest._rGrip = _rGrip;
        dest._clientState = _clientState;
    }

    private void CopyObjectPositionData(ObjectPositionData src, ref ObjectPositionData dest)
    {
        dest.Position = src.Position;
        dest.Rotation = src.Rotation;
    }

    public void UpdateVRClientData(VRNVRPlayerInfo vrinfo, bool calcStability = false)
    {
        if (vrinfo.Head != null && vrinfo.Head.Position != null && vrinfo.Head.Rotation != null)
        {
            Head.Position = vrinfo.Head.Position.ToVector3();
            Head.Rotation = vrinfo.Head.Rotation.ToQuaternion();
        }

        if (vrinfo.LeftController != null && vrinfo.LeftController.Position != null && vrinfo.LeftController.Rotation != null)
        {
            LeftController.Position = vrinfo.LeftController.Position.ToVector3();
            LeftController.Rotation = vrinfo.LeftController.Rotation.ToQuaternion();
        }

        if (vrinfo.RightController != null && vrinfo.RightController.Position != null && vrinfo.RightController.Rotation != null)
        {
            RightController.Position = vrinfo.RightController.Position.ToVector3();
            RightController.Rotation = vrinfo.RightController.Rotation.ToQuaternion();

            if (calcStability)
                SampleControllerStability(RightController);
        }

        if (vrinfo.RigOffset != null && vrinfo.RigOffset.Position != null && vrinfo.RigOffset.Rotation != null)
        {
            RigOffset.Position = vrinfo.RigOffset.Position.ToVector3();
            RigOffset.Rotation = vrinfo.RigOffset.Rotation.ToQuaternion();
        }

        LeftControllerTracked = vrinfo.LeftControllerTracked;
        RightControllerTracked = vrinfo.RightControllerTracked;

        OnLinkLine = vrinfo.OnLinkLine;
        PlayerHeight = vrinfo.PlayerHeight;
        PlayerMode = vrinfo.PlayerMode;
        LeftGrip = vrinfo.LeftGrip;
        RightGrip = vrinfo.RightGrip;
    }

    public void ApplyCalibrationOffset(VRNCalibrationOffsetData calData)
    {
        var newCalibrationPos = calData.OffsetPos.ToVector3();
        var newCalibrationRot = calData.OffsetRot.ToQuaternion();

        if (newCalibrationPos != CalibrationPos ||
            newCalibrationRot != CalibrationRot)
        {
            CalibrationPos = newCalibrationPos;
            CalibrationRot = newCalibrationRot;

            //reset test point
            CalTestPoint = Vector3.zero;
        }

        Debug.Log($"CalPos: {CalibrationPos.ToString()}\nCalRot: {CalibrationRot.ToString()}");
        CalReceiveTimestamp = Time.unscaledTime;

        if (CalibrationTransform != null)
        {
            CalibrationTransform.localPosition = CalibrationPos;
            CalibrationTransform.localRotation = CalibrationRot;
        }
    }


    public void ShowPlayer(bool show)
    {
        if (RigTransform == null)
            return;

        RigTransform.gameObject.SetActive(show);
    }


}


//[CreateAssetMenu(fileName = "PlayerManager", menuName = "VRMine/Managers/PlayerManager", order = 0)]
public class PlayerManager : MonoBehaviour
{
    //public const string DefaultResourcePath = "Managers/PlayerManager";
    //public static PlayerManager GetDefault() { return Resources.Load<PlayerManager>(DefaultResourcePath); }

    public static PlayerManager GetDefault(GameObject self)
    {
        var manager = self.GetDefaultManager<PlayerManager>("PlayerManager");
        manager.tag = "Manager";
        return manager;


        //GameObject obj = null;
        //var manager = FindObjectOfType<PlayerManager>();
        //if (manager != null)
        //    obj = manager.gameObject;

        //if (obj == null)
        //{
        //    obj = new GameObject("PlayerManager");
        //    obj.tag = "Manager";
        //}
        //manager = obj.GetComponent<PlayerManager>();
        //if (manager == null)
        //    manager = obj.AddComponent<PlayerManager>();

        //return manager;
    }

    public PlayerColorManager PlayerColorManager;
    public NetworkManager NetworkManager;
    public SystemManager SystemManager;

    //public PlayerRepresentation CurrentPlayer;
    //[System.NonSerialized]
    //public PlayerInfo CurrentPlayer;

    [System.NonSerialized]
    public PlayerRepresentation CurrentPlayer;

    //[System.NonSerialized]
    //public int PlayerID = -1;

    public delegate void PlayerMessageHandler(VRNPlayerMessageType messageType, VRNPlayerMessage msg);

    public event Action<PlayerRepresentation> PlayerJoined;
    public event Action<PlayerRepresentation> PlayerLeft;
    public event Action<VRNAssignPlayerRole> PlayerRoleAssigned;
    public event Action<int> PlayerIDAssigned;
    public event Action TestPointCentroidChanged;

    public Dictionary<int, PlayerRepresentation> PlayerList = new Dictionary<int, PlayerRepresentation>();


    private List<PlayerMessageHandler> _messageHandlers;
    private int _lastPlayerIDAssigned = 99;
    private bool _playerIDRequested = false;

    private Dictionary<int, VRNCalibrationOffsetData> _calDataCache;
    private List<Vector3> _testPointCache;

    public void Awake()
    {
        _messageHandlers = new List<PlayerMessageHandler>();
        CurrentPlayer = new PlayerRepresentation();
        CurrentPlayer.PlayerID = -1;
        CurrentPlayer.ClientID = -1;

        _calDataCache = new Dictionary<int, VRNCalibrationOffsetData>();
    }

    public void Start()
    {
        if (PlayerColorManager == null)
            PlayerColorManager = PlayerColorManager.GetDefault();
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        if (NetworkManager == null)
            return;


        switch (SystemManager.SystemConfig.PlatformType)
        {
            case PlatformType.Collocation:
                CurrentPlayer.PlayerMode = 0;
                CurrentPlayer.PlayerHeight = PlayerRepresentation.BasePlayerHeight;//The head bone height of the game model
                break;
            case PlatformType.Standing:
                CurrentPlayer.PlayerMode = 1;
                CurrentPlayer.PlayerHeight = PlayerRepresentation.BasePlayerHeight;//The head bone height of the game model
                CurrentPlayer.RotationEnabled = true;
                CurrentPlayer.TranslationEnabled = true;
                break;
            case PlatformType.Seated:
                CurrentPlayer.PlayerMode = 2;
                CurrentPlayer.PlayerHeight = 0;//The head bone height of the game model
                CurrentPlayer.RotationEnabled = true;
                CurrentPlayer.TranslationEnabled = true;
                break;
            case PlatformType.Desktop:
                CurrentPlayer.PlayerMode = 3;
                CurrentPlayer.PlayerHeight = 0;//The head bone height of the game model
                CurrentPlayer.RotationEnabled = true;
                CurrentPlayer.TranslationEnabled = true;
                break;
            default:
                break;
        }

        SceneManager.activeSceneChanged += OnSceneChanged;

        NetworkManager.RegisterHandler(VRNPacketType.AssignPlayerRole, AssignPlayerRoleHandler);
        NetworkManager.RegisterHandler(VRNPacketType.AssignPlayerId, AssignPlayerIDHandler);
        NetworkManager.RegisterHandler(VRNPacketType.RequestNewPlayerId, RequestPlayerIDHandler);
        NetworkManager.RegisterHandler(VRNPacketType.PlayerMessage, VRNPlayerMessageHandler);
        NetworkManager.RegisterHandler(VRNPacketType.SendAltStatus, VRNAltStatusHandler);
        NetworkManager.RegisterHandler(VRNPacketType.SendCalibrationData, VRNCalibrationDataHandler);
        NetworkManager.RegisterHandler(VRNPacketType.SendBatteryStatus, VRNBatteryStatusHandler);
        NetworkManager.RegisterHandler(VRNPacketType.SendSystemStatus, VRNSystemStatusHandler);
        NetworkManager.RegisterHandler(VRNPacketType.VrplayerInfo, VRNVRPlayerInfoHandler);
        NetworkManager.RegisterHandler(VRNPacketType.CalVerificationTest, VRNCalVerificationTestHandler);

        //NetworkManager.RegisterHandler(VRNPacketType.SendClientState, VRNSendClientStateHandler);

        NetworkManager.ClientJoinedServer += OnClientJoinedServer;
        NetworkManager.ClientDisconnected += OnClientDisconnected;
        NetworkManager.DisconnectedFromServer += OnDisconnectedFromServer;
        NetworkManager.ServerDisconnectedFromRelay += OnServerDisconnectedFromRelay;
        NetworkManager.ClientStateChanged += OnClientStateChanged;

        NetworkManager.ClientIDAssigned += OnClientIDAssigned;

        Util.DontDestroyOnLoad(gameObject);
    }

    public void RaiseTestPointCentroidChanged()
    {
        TestPointCentroidChanged?.Invoke();
    }

    private void OnClientStateChanged(VRNClientState cls)
    {
        PlayerRepresentation player = null;
        if (PlayerList.TryGetValue(cls.ClientID, out player))
        {
            player.ClientState = cls;
        }
    }

    //private void VRNSendClientStateHandler(VRNHeader header, Stream recvStream, NetPeer fromPeer)
    //{
    //    var clstate = VRNClientState.Parser.ParseDelimitedFrom(recvStream);
    //}

    private void OnClientIDAssigned(int clientid)
    {
        CurrentPlayer.ClientID = clientid;
    }

    //public void SendPlayerMessage(string messageType, IMessage msg)
    //{
    //    NetworkManager.SendPlayerMessage(PlayerID, messageType, msg);
    //}


    public void RequestPlayerID()
    {
        if (_playerIDRequested)
        {
            Debug.LogError($"Player ID already requested, stack: {System.Environment.StackTrace}");
            return;
        }

        if (!NetworkManager.ClientConnected)
        {
            Debug.LogError("PlayerManager: Attempt to request player id when client ID not assigned");
            return;
        }

        if (NetworkManager.IsServer)
        {
            //if we are the server, assign the ID directly
            var playerID = GeneratePlayerID();
            HandlePlayerIDAssigned(playerID);
            return;
        }

        //var name = SystemManager.SystemConfig.MultiplayerName;
        //if (name == null || name.Length <= 0)
        //    name = SystemInfo.deviceName;

        var name = NetworkManager.GetMultiplayerName();

        var msg = new VRNRequestNewPlayerID
        {
            ClientID = NetworkManager.ClientID,
            PlayerName = name,
        };

        NetworkManager.SendNetMessage(VRNPacketType.RequestNewPlayerId, msg, 
            broadcast: false, reliable: true, clientID: 0);        
    }

    public void RegisterPlayerMessageHandler(PlayerMessageHandler handler)
    {
        _messageHandlers.Add(handler);

        Debug.Log($"PlayerManager: {_messageHandlers.Count} handlers registered");
    }

    public void UnregisterPlayerMessageHandler(PlayerMessageHandler handler)
    {
        _messageHandlers.Remove(handler);

        Debug.Log($"PlayerManager: {_messageHandlers.Count} handlers registered");
    }

    public void SendPlayerMessage(int playerID, VRNPlayerMessageType messageType, string textData)
    {
        VRNPlayerMessage msg = new VRNPlayerMessage
        {
            PlayerID = playerID,
            MessageType = messageType,
            TextData = textData,
        };

        SendPlayerMessage(msg);
    }

    public void SendPlayerMessage(int playerID, VRNPlayerMessageType messageType, float floatData)
    {
        VRNPlayerMessage msg = new VRNPlayerMessage
        {
            PlayerID = playerID,
            MessageType = messageType,
            FloatData = floatData,
        };

        SendPlayerMessage(msg);
    }

    public void SendPlayerMessage(int playerID, VRNPlayerMessageType messageType, int intData)
    {
        VRNPlayerMessage msg = new VRNPlayerMessage
        {
            PlayerID = playerID,
            MessageType = messageType,
            IntData = intData,
        };

        SendPlayerMessage(msg);
    }

    public void SendPlayerMessageToAll(VRNPlayerMessageType messageType, int intData)
    {
        VRNPlayerMessage msg = new VRNPlayerMessage
        {          
            MessageType = messageType,
            IntData = intData,
        };

        SendPlayerMessageToAll(msg);
    }

    public void SendPlayerMessage(int playerID, VRNPlayerMessageType messageType, bool boolData)
    {
        VRNPlayerMessage msg = new VRNPlayerMessage
        {
            PlayerID = playerID,
            MessageType = messageType,
            BoolData = boolData,
        };

        SendPlayerMessage(msg);
    }

    public void SendPlayerMessage(int playerID, VRNPlayerMessageType messageType, Vector3 vec3data)
    {
        VRNPlayerMessage msg = new VRNPlayerMessage
        {
            PlayerID = playerID,
            MessageType = messageType,
            Vec3Data = vec3data.ToVRNVector3(),
        };

        SendPlayerMessage(msg);
    }

    public void SendPlayerMessage(int playerID, VRNPlayerMessageType messageType, Quaternion quaternionData)
    {
        VRNPlayerMessage msg = new VRNPlayerMessage
        {
            PlayerID = playerID,
            MessageType = messageType,
            QuaternionData = quaternionData.ToVRNQuaternion(),
        };

        SendPlayerMessage(msg);
    }

    public void SendPlayerMessage(int playerID, VRNPlayerMessageType messageType, VRNBG4SimData simData)
    {
        VRNPlayerMessage msg = new VRNPlayerMessage
        {
            PlayerID = playerID,
            MessageType = messageType,
            Bg4Data = simData,
        };

        SendPlayerMessage(msg);
    }

    public void SendPlayerMessageToAll(VRNPlayerMessage msg)
    {
        foreach (var player in PlayerList.Values)
        {
            msg.PlayerID = player.PlayerID;
            SendPlayerMessage(msg);
        }
    }

    private void SendPlayerMessage(VRNPlayerMessage msg)
    {
        ProcessPlayerMessage(msg);
        NetworkManager.SendNetMessage(VRNPacketType.PlayerMessage, msg);
    }


    public void AssignPlayerRole(int playerID, VRNPlayerRole role, PlayerDominantHand handedness)
    {
        bool leftHandDominant = false;
        if (handedness == PlayerDominantHand.LeftHanded)
            leftHandDominant = true;

        VRNAssignPlayerRole msg = new VRNAssignPlayerRole
        {
            PlayerID = playerID,
            Role = role,
            LeftHandDominant = leftHandDominant,
        };

        Debug.Log($"Assigning player {playerID} role {role} handedness {handedness}");

        NetworkManager.SendNetMessage(VRNPacketType.AssignPlayerRole, msg);
        UpdatePlayerRole(msg);

    }

    private void OnServerDisconnectedFromRelay()
    {
        RemoveAllPlayers();
    }

    private void OnDisconnectedFromServer()
    {
        _playerIDRequested = false;
        CurrentPlayer.PlayerID = -1;
        CurrentPlayer.ClientID = -1;

        RemoveAllPlayers();
    }

    private void OnClientJoinedServer(int clientID)
    {

    }

    private void OnClientDisconnected(int clientID)
    {
        RemovePlayer(clientID);
    }


    private int GeneratePlayerID()
    {
        _lastPlayerIDAssigned++;
        return _lastPlayerIDAssigned;
    }

    private void RequestPlayerIDHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {        
        if (!NetworkManager.IsServer)
            return;

        //var req = VRNRequestNewPlayerID.Parser.ParseDelimitedFrom(recvStream);
        var req = new VRNRequestNewPlayerID();
        reader.ReadMessage(req);

        PlayerRepresentation oldPlayerRep;
        int playerID;
        bool playerIsNew = true;

        if (PlayerList.TryGetValue(clientID, out oldPlayerRep))
        {
            Debug.Log($"PlayerManager: Reassigning client {clientID} player ID {oldPlayerRep.PlayerID}");
            playerID = oldPlayerRep.PlayerID;
            playerIsNew = false;
        }
        else
        {
            //int clientID = NetworkManager.GetClientID(fromPeer);
            playerID = GeneratePlayerID();
        }

        var assignMsg = new VRNAssignPlayerID
        {
            ClientID = clientID,
            PlayerID = playerID,
        };

        Debug.Log($"PlayerManager({gameObject.name}): Assigning client {clientID} player ID {playerID}");
        NetworkManager.SendNetMessage(VRNPacketType.AssignPlayerId, assignMsg, broadcast: false, clientID: clientID);

        //check for a default role assignment
        VRNPlayerRole role = VRNPlayerRole.UnknownRole;
        if (SystemManager.SystemConfig != null && SystemManager.SystemConfig.PlayerRoleConfig != null)
        {            
            if (SystemManager.SystemConfig.PlayerRoleConfig.TryGetValue(req.PlayerName, out role))
            {
                AssignPlayerRole(playerID, role, PlayerDominantHand.RightHanded);
            }
        }

        if (playerIsNew)
            AddNewPlayer(clientID, playerID, req.PlayerName, role);
    }

    private void AssignPlayerIDHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var msg = VRNAssignPlayerID.Parser.ParseDelimitedFrom(recvStream);
        var msg = new VRNAssignPlayerID();
        reader.ReadMessage(msg);

        if (msg.ClientID != NetworkManager.ClientID)
            return;

        HandlePlayerIDAssigned(msg.PlayerID);
    }

    private void HandlePlayerIDAssigned(int playerID)
    {
        if (CurrentPlayer.PlayerID > 0)
        {
            if (CurrentPlayer.PlayerID != playerID)
                Debug.LogError($"Player ID reassigned from {CurrentPlayer.PlayerID} to {playerID} client {NetworkManager.ClientID}");
            else
                Debug.LogWarning($"Player ID reassigned from {CurrentPlayer.PlayerID} to {playerID} client {NetworkManager.ClientID}");
        }

        Debug.Log($"PlayerManager: Assigned PlayerID {playerID} client {NetworkManager.ClientID}");
        CurrentPlayer.PlayerID = playerID;
        PlayerIDAssigned?.Invoke(playerID);
    }

    private void AssignPlayerRoleHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var msg = VRNAssignPlayerRole.Parser.ParseDelimitedFrom(recvStream);
        var msg = new VRNAssignPlayerRole();
        reader.ReadMessage(msg);

        UpdatePlayerRole(msg);
    }

    private void OnSceneChanged(Scene arg0, Scene arg1)
    {
        if (NetworkManager.IsServer)
        {
            foreach (var player in PlayerList.Values)
            {
                AssignPlayerRole(player.PlayerID, player.PlayerRole, player.PlayerDominantHand);
            }
        }

        if (CurrentPlayer != null)
        {
            CurrentPlayer.OnLinkLine = false;
        }
    }

    public void RemoveAllPlayers()
    {
        var clientIds = new List<int>(PlayerList.Keys);
        foreach (var id in clientIds)
        {
            RemovePlayer(id);
        }

        _calDataCache.Clear();
    }

    public void RemovePlayer(int clientID)
    {
        PlayerRepresentation player = null;
        if (PlayerList.TryGetValue(clientID, out player))
        {
            try
            {
                player.CalTestPointChanged -= OnPlayerCalTestPointChanged;
            }
            catch (Exception) { }

            PlayerList.Remove(clientID);
            PlayerLeft?.Invoke(player);
        }
    }

    public int LookupPlayerID(int clientID)
    {
        PlayerRepresentation player;

        if (PlayerList.TryGetValue(clientID, out player))
        {
            return player.PlayerID;
        }

        return -1;
    }

    public void UpdatePlayerRole(VRNAssignPlayerRole msg)
    {
        UpdatePlayerRole(msg.PlayerID, msg.Role, 
            msg.LeftHandDominant ? PlayerDominantHand.LeftHanded : PlayerDominantHand.RightHanded);
        if (PlayerRoleAssigned != null)
        {
            var invList = PlayerRoleAssigned.GetInvocationList();
            if (invList != null)
                Debug.Log($"PlayerManager: Invoking PlayerRoleAssigned with {invList.Length} listeners");
        }
        PlayerRoleAssigned?.Invoke(msg);
    }

    private void OnPlayerCalTestPointChanged(Vector3 pt)
    {
        try
        {
            RaiseTestPointCentroidChanged();
        }
        catch (Exception) { }
    }


    public PlayerRepresentation AddNewPlayer(int clientID, int playerID, string name, VRNPlayerRole role)
    {
        if (PlayerColorManager == null)
            PlayerColorManager = PlayerColorManager.GetDefault();

        PlayerRepresentation player;
        player = new PlayerRepresentation();
        player.ClientID = clientID;
        player.PlayerID = playerID;
        player.Name = name;
        player.PlayerRole = role;
        player.Head = new ObjectPositionData();
        player.LeftController = new ObjectPositionData();
        player.RightController = new ObjectPositionData();
        player.PlayerColor = PlayerColorManager.GetPlayerColor(player.ClientID);

        PlayerRepresentation oldPlayerRep = null;
        if (PlayerList.TryGetValue(clientID, out oldPlayerRep))
        {
            Debug.LogError($"Client {clientID} ({name}) player ID reassigned from: {oldPlayerRep.PlayerID} to: {playerID}");
        }
        //PlayerList.Add(clientID, player);
        PlayerList[clientID] = player;

        player.CalTestPointChanged += OnPlayerCalTestPointChanged;

        PlayerJoined?.Invoke(player);

        VRNCalibrationOffsetData calData;
        if (_calDataCache.TryGetValue(playerID, out calData))
        {
            player.ApplyCalibrationOffset(calData);
        }

        return player;
    }

    public void UpdatePlayer(PlayerRepresentation data, bool showPlayer = true)
    {
        PlayerRepresentation player;

        if (!PlayerList.TryGetValue(data.ClientID, out player))
        {
            return;
        }

        data.CopyTo(player);

        if (player.CalibrationTransform != null)
        {
            player.CalibrationTransform.localPosition = data.CalibrationPos;
            player.CalibrationTransform.localRotation = data.CalibrationRot;
        }

        player.ShowPlayer(showPlayer);
    }

    public void UpdateVRClientData(VRNVRPlayerInfo vrinfo)
    {
        PlayerRepresentation player;        

        if (vrinfo.PlayerID <= 0)
        {
            Debug.LogError($"Received VRNVRPlayerInfo for invalid player ID {vrinfo.PlayerID}");
            return;
        }

        //var strid = vrinfo.ClientID.ToString();
        if (!PlayerList.TryGetValue(vrinfo.ClientID, out player))
        {
            if (NetworkManager.IsServer && !NetworkManager.IsPlaybackMode)
                return; // server should only add players when a player ID is requested

            player = AddNewPlayer(vrinfo.ClientID, vrinfo.PlayerID, vrinfo.Name, vrinfo.Role);
            //player = new PlayerRepresentation();
            //player.ClientID = vrinfo.ClientID;
            //player.PlayerID = -1;
            //player.Name = vrinfo.Name;
            //player.Head = new ObjectPositionData();
            //player.LeftController = new ObjectPositionData();
            //player.RightController = new ObjectPositionData();

            //PlayerList.Add(vrinfo.ClientID, player);
            //PlayerJoined?.Invoke(player);
        }

        UpdateVRClientData(vrinfo, player);
        

        //Debug.Log($"Got VRInfo {vrinfo.ClientID} Left: {vrinfo.LeftController.Position.ToString()} Right: {vrinfo.RightController.Position.ToString()}");

        PlayerList[vrinfo.ClientID] = player;

    }

    public void UpdateLocalVRClientData(VRNVRPlayerInfo vrinfo)
    {
        if (CurrentPlayer != null)
        {
            //UpdateVRClientData(vrinfo, CurrentPlayer);
            CurrentPlayer.UpdateVRClientData(vrinfo, false);
        }
    }

    private void UpdateVRClientData(VRNVRPlayerInfo vrinfo, PlayerRepresentation player)
    {
        player.UpdateVRClientData(vrinfo, NetworkManager.IsServer);
    }


    private void UpdatePlayerRole(int playerID, VRNPlayerRole role, PlayerDominantHand dominantHand)
    {
        if (PlayerList == null)
            return;

        if (CurrentPlayer.PlayerID == playerID)
        {
            CurrentPlayer.PlayerRole = role;
            CurrentPlayer.PlayerDominantHand = dominantHand;
            return;
        }

        foreach (var player in PlayerList.Values)
        {
            if (player.PlayerID == playerID)
            {
                if (player.PlayerRole != role)
                    player.PlayerRole = role;

                player.PlayerDominantHand = dominantHand;

                if (NetworkManager.IsServer)
                {
                    if (player.Name != null && player.Name.Length > 0 && SystemManager.SystemConfig != null)
                    {
                        SystemManager.SystemConfig.PlayerRoleConfig[player.Name] = role;
                        SystemManager.SystemConfig.SaveConfig();
                    }
                }
            }
        }
    }

    private void VRNPlayerMessageHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var msg = VRNPlayerMessage.Parser.ParseDelimitedFrom(recvStream);
        var msg = new VRNPlayerMessage();
        reader.ReadMessage(msg);

        if (msg.PlayerID != CurrentPlayer.PlayerID)
            return;

        ProcessPlayerMessage(msg);
    }

    public PlayerRepresentation GetPlayer(int playerID)
    {
        foreach (var player in PlayerList.Values)
        {
            if (player.PlayerID == playerID)
            {
                return player;
            }
        }

        return null;
    }

    public Vector3 ComputeTestPointCentroid()
    {
        if (PlayerList == null || PlayerList.Count <= 0)
            return Vector3.zero;

        Vector3 centroid = Vector3.zero;
        int count = 0;

        foreach (var player in PlayerList.Values)
        {
            if (player.CalTestPoint == Vector3.zero)
                continue;

            centroid += player.CalTestPoint;
            count++;
        }

        centroid *= (1.0f / (float)count);

        return centroid;
    }

    public float ComputeCalTestPointCloudSize()
    {
        if (PlayerList == null || PlayerList.Count <= 0)
            return -1;

        if (_testPointCache == null)
        {
            _testPointCache = new List<Vector3>(PlayerList.Count + 10);
        }

        _testPointCache.Clear();
        foreach (var player in PlayerList.Values)
        {
            if (player.CalTestPoint == Vector3.zero)
                continue;

            _testPointCache.Add(player.CalTestPoint);
        }

        float maxDist = 0;

        for (int i = 0; i < _testPointCache.Count; i++)
        {
            for (int j = 1; j <_testPointCache.Count; j++)
            {
                float dist = Vector3.Distance(_testPointCache[i], _testPointCache[j]);
                if (dist > maxDist)
                    maxDist = dist;
            }
        }

        return maxDist * 1000.0f;
    }


    private void VRNAltStatusHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var altStatus = VRNAntilatencyStatus.Parser.ParseDelimitedFrom(recvStream);
        var altStatus = new VRNAntilatencyStatus();
        reader.ReadMessage(altStatus);

        //Debug.Log($"Received Alt Status {altStatus.ToString()}");

        foreach (var player in PlayerList.Values)
        {
            if (player.PlayerID == altStatus.PlayerID)
            {
                player.AntilatencyStatusText = altStatus.StatusText;
                break;
            }
        }
    }

    private void VRNCalibrationDataHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var calData = VRNCalibrationOffsetData.Parser.ParseDelimitedFrom(recvStream);
        var calData = new VRNCalibrationOffsetData();
        reader.ReadMessage(calData);

        Debug.Log($"Received calibration data for player {calData.PlayerID}");

        var player = GetPlayer(calData.PlayerID);
        if (player != null)
        {
            player.ApplyCalibrationOffset(calData);
        }
        else
        {
            //couldn't find a player, add this data to a cache
            if (_calDataCache.ContainsKey(calData.PlayerID))
                _calDataCache[calData.PlayerID] = calData;
            else
                _calDataCache.Add(calData.PlayerID, calData);
        }
    }

    private void VRNBatteryStatusHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var battStatus = VRNBatteryStatus.Parser.ParseDelimitedFrom(recvStream);
        var battStatus = new VRNBatteryStatus();
        reader.ReadMessage(battStatus);

        PlayerRepresentation player;
        if (!PlayerList.TryGetValue(battStatus.ClientID, out player))
            return;

        player.BatteryStatus = battStatus.StatusMessage;
    }

    private void VRNSystemStatusHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var systemStatus = VRNSystemStatus.Parser.ParseDelimitedFrom(recvStream);
        var systemStatus = new VRNSystemStatus();
        reader.ReadMessage(systemStatus);

        PlayerRepresentation player;
        if (!PlayerList.TryGetValue(systemStatus.ClientID, out player))
            return;

        player.AppFocused = systemStatus.AppFocused;
        player.AppPaused = systemStatus.AppPaused;
        player.FrameRate = systemStatus.FrameRate;
    }

    private void VRNVRPlayerInfoHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        //var vrpi = VRNVRPlayerInfo.Parser.ParseDelimitedFrom(recvStream);
        var vrpi = new VRNVRPlayerInfo();
        reader.ReadMessage(vrpi);

        //VRPlayerInfoChanged.Invoke(vrpi);
        //UpdateVRClientData(vrpi);
        UpdateVRClientData(vrpi);        
    }

    private void VRNCalVerificationTestHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        if (!NetworkManager.IsServer)
            return;

        var calData = new VRNCalVerificationData();
        reader.ReadMessage(calData);

        var player = GetPlayer(calData.PlayerID);
        player.SaveCalTestPosition(calData.VerificationPoint.ToVector3());
    }

    private void ProcessPlayerMessage(VRNPlayerMessage msg)
    {
        foreach (var handler in _messageHandlers)
        {
            try
            {
                handler(msg.MessageType, msg);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in player message handler {ex.Message}");
            }
        }

        foreach (var player in PlayerList.Values)
        {
            player.ProcessPlayerMessage(msg);
        }

        if (CurrentPlayer != null)
            CurrentPlayer.ProcessPlayerMessage(msg);
    }

    //private void ProcessPlayerMessage()



}
