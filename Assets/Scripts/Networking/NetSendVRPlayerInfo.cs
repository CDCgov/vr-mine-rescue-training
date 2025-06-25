using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

public class NetSendVRPlayerInfo : MonoBehaviour
{
    private class ControllerData
    {
        public Vector3 LastPositionRelative;
        public Quaternion LastRotationRelative;
    }

    public SystemManager SystemManager;
    public NetworkManager NetManager;
    public PlayerManager PlayerManager;
    public SceneLoadManager SceneLoadManager;

    public float BroadcastDelay = 0.025f;
    public Transform HeadTransform;
    public Transform CalibrationOffsetTransform;
    public Transform LeftControllerTransform;
    public Transform RightControllerTransform;

    public GameObject CalibrationSuccessPrefab;
    public GameObject MessageDisplayPrefab;

    private float _lastSend = 0;
    private VRNVRPlayerInfo _vrpi;
    //private int _playerID = -1;
    private bool _joinWorldSent = false;
    private string _name;

    //private PlayerInfo _playerInfo;
    private TeleportController _playerTeleportController;
    private XRNode _LeftNode;
    private XRNode _RightNode;
    private InputDevice _leftDevice;
    private InputDevice _rightDevice;

    private ControllerData _leftControllerData;
    private ControllerData _rightControllerData;
    //private Vector3 _cachedLControllerRelPos;
    //private Vector3 _cachedLControllerRot;
    //private Vector3 _cachedRControllerRelPos;
    //private Vector3 _cachedRControllerRot;

    private bool _primaryWasPressed = false;
    private bool _secondaryWasPressed = false;
    private bool _axisWasClicked = false;

    private GameObject _messageDisplay;
    private float _messageDisplayDuration = 0;
    private float _messageDisplayStartTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (NetManager == null)
            NetManager = NetworkManager.GetDefault(gameObject);
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        if (SceneLoadManager == null)
            SceneLoadManager = SceneLoadManager.GetDefault(gameObject);


        NetManager.NetworkUpdateRateChanged += OnNetworkUpdateRateChanged;

        //_playerInfo = GetComponent<PlayerInfo>();
        //if (_playerInfo != null)
        //{
        //	_playerID = _playerInfo.PlayerID;
        //}
        //else
        //{
        //	Debug.LogError("NetSendVRPlayerInfo can't find PlayerInfo component");
        //	enabled = false;
        //}

        _playerTeleportController = GetComponent<TeleportController>();
        if (_playerTeleportController == null)
            _playerTeleportController = GetComponentInChildren<TeleportController>();

        //NetManager.ClientIDAssigned += (clientID) =>
        //{

        //    //if (!_joinWorldSent)
        //    //{
        //    //    _joinWorldSent = true;
        //    //    NetManager.SendRequestJoinWorld();
        //    //}
        //};

        //NetManager.PlayerIDAssigned += (playerID) =>
        //{
        //    _playerID = playerID;
        //};

        //_playerID = NetManager.PlayerID;


        //if (NetManager.ClientID >= 0 && NetManager.PlayerID < 0)
        //{
        //    //if (!_joinWorldSent)
        //    //{
        //    //    _joinWorldSent = true;
        //    //    NetManager.SendRequestJoinWorld();
        //    //}
        //}

        _name = SystemManager.SystemConfig.MultiplayerName;
        if (_name == null || _name.Length <= 0)
            _name = SystemInfo.deviceName;


        _vrpi = new VRNVRPlayerInfo();
        _vrpi.Name = _name;
        _vrpi.Head = new VRNTransformData();
        _vrpi.LeftController = new VRNTransformData();
        _vrpi.RightController = new VRNTransformData();
        _vrpi.RigOffset = new VRNTransformData();

        UpdateBroadcastDelay();

        PlayerManager.RegisterPlayerMessageHandler(OnPlayerMessage);

        //if (PlayerManager.CurrentPlayer != null)
        //{
        //    CalibrationOffsetTransform.localRotation = PlayerManager.CurrentPlayer.CalibrationRot;
        //    CalibrationOffsetTransform.localPosition = PlayerManager.CurrentPlayer.CalibrationPos;
        //}
        CalibrationOffsetTransform.localRotation = SystemManager.SystemConfig.CalibrationRotation.ToQuaternion();
        CalibrationOffsetTransform.localPosition = SystemManager.SystemConfig.CalibrationOffset.ToVector3();

        //switch (SystemManager.SystemConfig.PlatformType)
        //{
        //    case PlatformType.Collocation:
        //        PlayerManager.CurrentPlayer.PlayerHeight = HeadTransform.position.y;
        //        break;
        //    case PlatformType.Standing:
        //        PlayerManager.CurrentPlayer.PlayerHeight = HeadTransform.position.y;
        //        break;
        //    case PlatformType.Seated:
        //        PlayerManager.CurrentPlayer.PlayerHeight = 1.58f;
        //        break;
        //    case PlatformType.Desktop:
        //        PlayerManager.CurrentPlayer.PlayerHeight = 1.58f;
        //        break;
        //    default:
        //        break;
        //}

        _LeftNode = XRNode.LeftHand;
        _RightNode = XRNode.RightHand;
        _leftDevice = InputDevices.GetDeviceAtXRNode(_LeftNode);
        _rightDevice = InputDevices.GetDeviceAtXRNode(_RightNode);

        if (SystemManager != null && SystemManager.SystemConfig != null)
        {
            UpdateCalibration(SystemManager.SystemConfig.CalibrationOffset.ToVector3(),
                SystemManager.SystemConfig.CalibrationRotation.ToQuaternion());
        }

        SendCalibrationData();

        NetManager.ClientJoinedServer += OnClientJoined;
        NetManager.ClientJoinedScene += OnClientJoined;
        NetManager.ClientRequestedWorldState += OnClientJoined;

        //_cachedLControllerRelPos = Vector3.zero;
        //_cachedLControllerRot = Vector3.zero;
        //_cachedRControllerRelPos = Vector3.zero;
        //_cachedRControllerRot = Vector3.zero;

        _leftControllerData = new ControllerData();
        _rightControllerData = new ControllerData();
    }

    private void OnDestroy()
    {
        PlayerManager.UnregisterPlayerMessageHandler(OnPlayerMessage);

        if (NetManager != null)
        {
            NetManager.ClientJoinedServer -= OnClientJoined;
            NetManager.ClientJoinedScene -= OnClientJoined;
            NetManager.ClientRequestedWorldState -= OnClientJoined;
        }
    }

    private void CheckControllerInput(InputDevice device)
    {
        if (SceneLoadManager == null)
            return;

        if (!SceneLoadManager.InWaitingRoom && !ScenarioSaveLoad.Settings.AllowSelfCalibration)
            //(MineSceneConfiguration.CurrentScene == null || !MineSceneConfiguration.CurrentScene.AllowSelfCalibration))
            return;

        bool triggerPressed = false;
        bool gripPressed = false;
        bool primaryPressed = false;
        bool secondaryPressed = false;
        bool axisClicked = false;

        //Debug.Log("NetSendVRPI: Checking controller");

        if (!device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed))
            return;

        if (!device.TryGetFeatureValue(CommonUsages.gripButton, out gripPressed))
            return;

        if (!device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryPressed))
            return;

        if (!device.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryPressed))
            return;

        if (!device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out axisClicked))
            return;


        //Debug.Log($"NetSendVRPI: Input: {triggerPressed} {gripPressed} {primaryPressed} {secondaryPressed}");

        if (triggerPressed && gripPressed)
        {
            if (primaryPressed && !_primaryWasPressed)
            {
                //device.SendHapticImpulse(0, 0.5f, 0.5f);
                LocalCalOrigin();
            }

            else if (secondaryPressed && !_secondaryWasPressed)
            {
                //device.SendHapticImpulse(0, 1.0f, 0.25f);

                //SpawnCalibrationSuccessPrefab();
                LocalCalRotation();
            }

            else if (axisClicked && !_axisWasClicked)
            {
                LocalCalVerification();
            }
        }

        _primaryWasPressed = primaryPressed;
        _secondaryWasPressed = secondaryPressed;
        _axisWasClicked = axisClicked;
    }

    bool IsControllerStable()
    {
        if (PlayerManager.CurrentPlayer == null)
            return false;

        bool isTracking = false;
        if (!_rightDevice.TryGetFeatureValue(CommonUsages.isTracked, out isTracking))
        {
            ShowMessage("Controller\nNot\nTracked", 1.5f);
            return false;
        }

        if (!isTracking)
        {
            ShowMessage("Controller\nNot\nTracked", 1.5f);
            Debug.Log($"NetSendVRPI: Controller not tracking");
            return false;
        }


        float stabilityDist;
        PlayerManager.CurrentPlayer.GetRightControllerStability(out stabilityDist);

        Debug.Log($"NetSendVRPI: Controller Stability {stabilityDist:F2}");

        if (stabilityDist < 3.0f && stabilityDist >= 0)
        {
            return true;
        }

        ShowMessage($"{stabilityDist:F0}", 1.5f);

        return false;
    }

    void LocalCalOrigin()
    {
        if (!IsControllerStable())
            return;

        RecomputeCalibrationOffset(VRNCalibrationSource.CalRightController);
        SetFloorCalibration(VRNCalibrationSource.CalRightController);

        SpawnCalibrationSuccessPrefab();
        //ShowMessage("Origin\nCalibrated", 5);
        _leftDevice.SendHapticImpulse(0, 1.0f, 0.25f);
    }

    async void LocalCalRotation()
    {
        if (!IsControllerStable())
            return;

        SetCalibrationRotation(VRNCalibrationSource.CalRightController);

        SpawnCalibrationSuccessPrefab();
        //ShowMessage("Rotation\nCalibrated", 5);
        _leftDevice.SendHapticImpulse(0, 1.0f, 0.25f);
        await Task.Delay(750);
        _leftDevice.SendHapticImpulse(0, 1.0f, 0.25f);

    }

    async void LocalCalVerification()
    {
        if (!IsControllerStable())
            return;

        if (PlayerManager.CurrentPlayer == null)
            return;

        var pos = PlayerManager.CurrentPlayer.GetRightControllerPOISpace();

        VRNCalVerificationData calData = new VRNCalVerificationData();
        calData.ClientID = NetManager.ClientID;
        calData.PlayerID = PlayerManager.CurrentPlayer.PlayerID;
        calData.VerificationPoint = pos.ToVRNVector3();

        NetManager.SendNetMessage(VRNPacketType.CalVerificationTest, calData, broadcast: false);

        SpawnCalibrationSuccessPrefab();
        //ShowMessage("Rotation\nCalibrated", 5);
        _leftDevice.SendHapticImpulse(0, 1.0f, 0.25f);
        await Task.Delay(400);
        _leftDevice.SendHapticImpulse(0, 1.0f, 0.25f);
        await Task.Delay(400);
        _leftDevice.SendHapticImpulse(0, 1.0f, 0.25f);

    }

    void SpawnCalibrationSuccessPrefab()
    {
        if (CalibrationSuccessPrefab == null || RightControllerTransform == null)
            return;

        Instantiate<GameObject>(CalibrationSuccessPrefab, RightControllerTransform.position, Quaternion.identity, null);
    }

    void ShowMessage(string message, float duration)
    {
        if (MessageDisplayPrefab == null || HeadTransform == null)
            return;

        if (_messageDisplay == null)
        {
            _messageDisplay = Instantiate<GameObject>(MessageDisplayPrefab, HeadTransform, false);
        }

        _messageDisplayStartTime = Time.time;
        _messageDisplayDuration = duration;

        //var msgObj = Instantiate<GameObject>(MessageDisplayPrefab, HeadTransform, false);
        var txt = _messageDisplay.GetComponent<TMPro.TMP_Text>();

        if (txt != null)
        {
            txt.text = message;
        }

        _messageDisplay.SetActive(true);
    }

    private void OnNetworkUpdateRateChanged()
    {
        UpdateBroadcastDelay();
    }

    private void UpdateBroadcastDelay()
    {
        if (SystemManager.SystemConfig != null && SystemManager.SystemConfig.MPVRUpdateRateHz > 0)
        {
            BroadcastDelay = 1.0f / SystemManager.SystemConfig.MPVRUpdateRateHz;
            Debug.Log($"VR Broadcast Delay set to {(BroadcastDelay * 1000.0f):F1}ms");
        }
    }

    private async void OnClientJoined(int obj)
    {
        await Task.Delay(1000);
        SendCalibrationData();
    }

    private void OnPlayerMessage(VRNPlayerMessageType messageType, VRNPlayerMessage msg)
    {
        if (msg.PlayerID != PlayerManager.CurrentPlayer.PlayerID)
            return;

        VRNCalibrationSource source = (VRNCalibrationSource)msg.IntData;



        switch (messageType)
        {
            case VRNPlayerMessageType.PmRecomputeCalibrationOffset:
                RecomputeCalibrationOffset(source);
                break;

            case VRNPlayerMessageType.PmResetCalibrationOffset:
                ResetCalibrationOffset();
                break;

            case VRNPlayerMessageType.PmSetCalibrationRotation:
                SetCalibrationRotation(source);
                break;

            case VRNPlayerMessageType.PmTuneCalibrationRotation:
                TuneCalibrationRotation(source);
                break;

            case VRNPlayerMessageType.PmSetFloorCalibration:
                SetFloorCalibration(source);
                break;

            case VRNPlayerMessageType.PmResetFloorCalibration:
                ResetFloorCalibration();
                break;

            case VRNPlayerMessageType.PmTwoControllerCalibration:
                TwoControllerCalibration();
                break;
        }

    }


    public void ResetCalibrationOffset()
    {
        CalibrationOffsetTransform.localRotation = Quaternion.identity;
        CalibrationOffsetTransform.localPosition = Vector3.zero;

        if (PlayerManager != null && PlayerManager.CurrentPlayer != null)
        {
            PlayerManager.CurrentPlayer.CalibrationPos = Vector3.zero;
            PlayerManager.CurrentPlayer.CalibrationRot = Quaternion.identity;
        }
        SendCalibrationData();
        SaveCalibration();
    }

    private Transform GetCalSource(VRNCalibrationSource source)
    {
        Transform calTransform = HeadTransform;

        switch (source)
        {
            case VRNCalibrationSource.CalHead:
                break;

            case VRNCalibrationSource.CalLeftController:
                calTransform = LeftControllerTransform;
                break;

            case VRNCalibrationSource.CalRightController:
                calTransform = RightControllerTransform;
                break;
        }

        return calTransform;
    }

    /// <summary>
    /// Recompute the calibration offset and rotation so that the specified
    /// source is located at the origin
    /// </summary>
    /// <param name="source"></param>
    public void RecomputeCalibrationOffset(VRNCalibrationSource source)
    {

        //CalibrationOffsetTransform.localRotation = Quaternion.identity;
        //CalibrationOffsetTransform.localPosition = Vector3.zero;

        ////compute the calibration values to cancel out the current local position
        ////and rotation of the HMD

        //var euler = HeadTransform.localRotation.eulerAngles;
        //euler.x = 0;
        //euler.z = 0;
        //var calRot = Quaternion.Inverse(Quaternion.Euler(euler));

        //CalibrationOffsetTransform.localRotation = calRot;

        //var calOffset = HeadTransform.localPosition * -1.0f;
        //calOffset.y = 0;
        //calOffset = HeadTransform.TransformDirection(calOffset);
        //calOffset = CalibrationOffsetTransform.InverseTransformDirection(calOffset);

        //CalibrationOffsetTransform.localPosition = calOffset;

        Transform calTransform = GetCalSource(source);

        Util.ComputeInverseTransform(CalibrationOffsetTransform, calTransform);

        //if (source == VRNCalibrationSource.CalRightController)
        //{
        //    //rotate 180 to match controller jig
        //    Vector3 euler = CalibrationOffsetTransform.localRotation.eulerAngles;

        //    string msg = $"({euler.x:F0}, {euler.y:F0}, {euler.z:F0})\n";

        //    euler.x = 0;
        //    euler.y += 180;
        //    euler.z = 0;

        if (source == VRNCalibrationSource.CalRightController)
        {
            /*
            Vector3 forward = calTransform.forward;
            forward *= -1;

            var forwardPos = calTransform.position + forward;
            */

            var forwardVec = SystemManager.SystemConfig.CalibrationInitialForwardVector.ToVector3();
            forwardVec = forwardVec.normalized;
            if (float.IsNaN(forwardVec.x) || float.IsNaN(forwardVec.y) || float.IsNaN(forwardVec.z))
                forwardVec = Vector3.forward;

            //transform forward vector form the coordinate space of the controller to world space
            forwardVec = calTransform.TransformDirection(forwardVec);
            var forwardPos = calTransform.position + forwardVec;

            Util.UpdateCalibrationRotation(CalibrationOffsetTransform, HeadTransform, forwardPos);
        }



        //    CalibrationOffsetTransform.localRotation = Quaternion.Euler(euler);

        //    euler = CalibrationOffsetTransform.localRotation.eulerAngles;
        //    msg = msg +  $"({euler.x:F0}, {euler.y:F0}, {euler.z:F0})";
        //    ShowMessage(msg, 5);
        //}

        UpdateCalibration(CalibrationOffsetTransform.localPosition, CalibrationOffsetTransform.localRotation);
        SaveCalibration();

        //if (PlayerManager != null && PlayerManager.CurrentPlayer != null)
        //{
        //    PlayerManager.CurrentPlayer.CalibrationRot = CalibrationOffsetTransform.localRotation;
        //    PlayerManager.CurrentPlayer.CalibrationPos = CalibrationOffsetTransform.localPosition;
        //}

        //SendCalibrationData();
    }

    /// <summary>
    /// Update the calibration rotation so that the specified source
    /// is along the forward (positive z) axis
    /// </summary>
    /// <param name="source"></param>
    public void SetCalibrationRotation(VRNCalibrationSource source)
    {
        Transform calTransform = GetCalSource(source);

        Util.UpdateCalibrationRotation(CalibrationOffsetTransform, HeadTransform, calTransform.position);

        UpdateCalibration(CalibrationOffsetTransform.localPosition, CalibrationOffsetTransform.localRotation);
        SaveCalibration();
        //if (PlayerManager != null && PlayerManager.CurrentPlayer != null)
        //{
        //    PlayerManager.CurrentPlayer.CalibrationRot = CalibrationOffsetTransform.localRotation;
        //    PlayerManager.CurrentPlayer.CalibrationPos = CalibrationOffsetTransform.localPosition;
        //}

        //SendCalibrationData();
    }

    public void ResetFloorCalibration()
    {
        var pos = CalibrationOffsetTransform.localPosition;
        pos.y = 0;
        CalibrationOffsetTransform.localPosition = pos;

        UpdateCalibration(CalibrationOffsetTransform.localPosition, CalibrationOffsetTransform.localRotation);
        SaveCalibration();
    }

    public void SetFloorCalibration(VRNCalibrationSource source)
    {
        Transform calTransform = GetCalSource(source);

        var worldPos = calTransform.position;
        /*var delta = Vector3.zero;
        if (CalibrationOffsetTransform.parent == null)
            delta = new Vector3(0, worldPos.y * -1, 0);
        else
            delta = new Vector3(0, CalibrationOffsetTransform.parent.position.y - worldPos.y);

        CalibrationOffsetTransform.position += delta; */

        Vector3 parentPos = Vector3.zero;
        if (CalibrationOffsetTransform.parent != null)
            worldPos = CalibrationOffsetTransform.parent.InverseTransformPoint(worldPos);

        var delta = new Vector3(0, worldPos.y * -1, 0);
        CalibrationOffsetTransform.position += delta;

        UpdateCalibration(CalibrationOffsetTransform.localPosition, CalibrationOffsetTransform.localRotation);
        SaveCalibration();
    }

    public void TwoControllerCalibration()
    {
        Util.ComputeInverseTransform(CalibrationOffsetTransform, RightControllerTransform);
        Util.UpdateCalibrationRotation(CalibrationOffsetTransform, HeadTransform, LeftControllerTransform.position);
        UpdateCalibration(CalibrationOffsetTransform.localPosition, CalibrationOffsetTransform.localRotation);
        SaveCalibration();
    }

    public void TuneCalibrationRotation(VRNCalibrationSource source)
    {
        SendCalibrationData();
    }

    void UpdateCalibration(Vector3 calOffset, Quaternion calRotation)
    {
        CalibrationOffsetTransform.localPosition = calOffset;
        CalibrationOffsetTransform.localRotation = calRotation;

        if (PlayerManager != null && PlayerManager.CurrentPlayer != null)
        {
            PlayerManager.CurrentPlayer.CalibrationRot = calRotation;
            PlayerManager.CurrentPlayer.CalibrationPos = calOffset;
            //switch (SystemManager.SystemConfig.PlatformType)
            //{
            //    case PlatformType.Collocation:
            //        PlayerManager.SendPlayerMessage(PlayerManager.CurrentPlayer.PlayerID, VRNPlayerMessageType.PmSetPlayerHeight, HeadTransform.position.y);
            //        break;
            //    case PlatformType.Standing:
            //        break;
            //    case PlatformType.Seated:
            //        break;
            //    case PlatformType.Desktop:
            //        break;
            //    default:
            //        break;
            //}
        }

        SendCalibrationData();
    }

    void SaveCalibration()
    {
        SystemManager.SystemConfig.CalibrationOffset = YAMLVec3.FromVector3(CalibrationOffsetTransform.localPosition);
        SystemManager.SystemConfig.CalibrationRotation = YAMLQuaternion.FromQuaternion(CalibrationOffsetTransform.localRotation);
        SystemManager.SystemConfig.SaveConfig();
    }

    void SendCalibrationData()
    {
        if (NetManager == null || PlayerManager == null || PlayerManager.CurrentPlayer == null ||
            PlayerManager.CurrentPlayer.PlayerID <= 0)
            return;

        VRNCalibrationOffsetData calData = new VRNCalibrationOffsetData
        {
            ClientID = NetManager.ClientID,
            PlayerID = PlayerManager.CurrentPlayer.PlayerID,
            OffsetPos = CalibrationOffsetTransform.localPosition.ToVRNVector3(),
            OffsetRot = CalibrationOffsetTransform.localRotation.ToVRNQuaternion(),
        };

        NetManager.SendNetMessage(VRNPacketType.SendCalibrationData, calData);
        PlayerManager.SendPlayerMessage(PlayerManager.CurrentPlayer.PlayerID, VRNPlayerMessageType.PmSetPlayerMode, (int)SystemManager.SystemConfig.PlatformType);
        switch (SystemManager.SystemConfig.PlatformType)
        {
            case PlatformType.Collocation:

                PlayerManager.SendPlayerMessage(PlayerManager.CurrentPlayer.PlayerID, VRNPlayerMessageType.PmSetPlayerHeight, PlayerManager.CurrentPlayer.PlayerHeight);
                break;
            case PlatformType.Standing:
                PlayerManager.SendPlayerMessage(PlayerManager.CurrentPlayer.PlayerID, VRNPlayerMessageType.PmSetPlayerHeight, PlayerManager.CurrentPlayer.PlayerHeight);
                break;
            case PlatformType.Seated:
                PlayerManager.SendPlayerMessage(PlayerManager.CurrentPlayer.PlayerID, VRNPlayerMessageType.PmSetPlayerHeight, 0);
                break;
            case PlatformType.Desktop:
                PlayerManager.SendPlayerMessage(PlayerManager.CurrentPlayer.PlayerID, VRNPlayerMessageType.PmSetPlayerHeight, 0);
                break;
            default:
                break;
        }
    }

    private Vector3 ComputeRelativePosition(Transform t)
    {
        return t.localPosition;
        //return transform.InverseTransformPoint(t.position);
    }

    private Quaternion ComputeRelativeRotation(Transform t)
    {
        return t.localRotation;
        //Quaternion q = t.localRotation;
        //int x = 10;
        //var parent = t.parent;
        //while (x > 0 && parent != transform)
        //{
        //    x--;
        //    q = parent.localRotation * q;
        //}
        //return q;
    }

    bool UpdateControllerData(InputDevice inputDevice, Transform controller, ControllerData data, VRNTransformData transformData)
    {
        bool isTracking = false;
        if (inputDevice.isValid)
        {
            if (!inputDevice.TryGetFeatureValue(CommonUsages.isTracked, out isTracking))
                isTracking = false;
        }

        /*
        if (isTracking)
        {
            transformData.Position = controller.localPosition.ToVRNVector3();
            transformData.Rotation = controller.localRotation.ToVRNQuaternion();

            if (HeadTransform != null)
            {
                //update the last valid position in the space of the head transform
                data.LastPositionRelative = HeadTransform.InverseTransformPoint(controller.position);
                //data.LastRotationRelative = controller.localRotation * HeadTransform.localRotation;
                data.LastRotationRelative = (Quaternion.Inverse(HeadTransform.localRotation) * data.LastRotationRelative);
            }
        }
        else
        {
            if (HeadTransform != null)
            {
                //transform the last valid position back from the coordinate space of the head tranform
                var worldSpace = HeadTransform.TransformPoint(data.LastPositionRelative);
                transformData.Position = controller.parent.InverseTransformPoint(worldSpace).ToVRNVector3();
                //transformData.Rotation = (data.LastRotationRelative * Quaternion.Inverse(HeadTransform.localRotation)).ToVRNQuaternion();
                transformData.Rotation = (HeadTransform.localRotation * HeadTransform.localRotation).ToVRNQuaternion();
            }
        }
        */

        transformData.Position = controller.localPosition.ToVRNVector3();
        transformData.Rotation = controller.localRotation.ToVRNQuaternion();

        return isTracking;
    }

    void SendVRPlayerInfo()
    {
        if (NetManager == null || !NetManager.IsInGame)
            return;

        _vrpi.ClientID = NetManager.ClientID;
        _vrpi.PlayerID = PlayerManager.CurrentPlayer.PlayerID;
        _vrpi.OnLinkLine = PlayerManager.CurrentPlayer.OnLinkLine;

        //if (_playerTeleportController != null)
        //{
        //    _vrpi.OnLinkLine = PlayerManager.CurrentPlayer.OnLinkLine;
        //}

        _vrpi.Role = PlayerManager.CurrentPlayer.PlayerRole;
        _vrpi.PlayerHeight = PlayerManager.CurrentPlayer.PlayerHeight;
        _vrpi.LeftGrip = PlayerManager.CurrentPlayer.LeftGrip;
        _vrpi.RightGrip = PlayerManager.CurrentPlayer.RightGrip;
        //if (_playerInfo != null)
        //{
        //	_vrpi.Role = _playerInfo.PlayerRole;
        //}

        if (HeadTransform != null)
        {
            //_vrpi.Head.Position = HeadTransform.localPosition.ToVRNVector3();
            //_vrpi.Head.Rotation = HeadTransform.localRotation.ToVRNQuaternion();
            _vrpi.Head.Position = ComputeRelativePosition(HeadTransform).ToVRNVector3();
            _vrpi.Head.Rotation = ComputeRelativeRotation(HeadTransform).ToVRNQuaternion();
        }

        if (LeftControllerTransform != null)
        {
            if (!_leftDevice.isValid)
                _leftDevice = InputDevices.GetDeviceAtXRNode(_LeftNode);

            _vrpi.LeftControllerTracked = UpdateControllerData(_leftDevice, LeftControllerTransform, 
                _leftControllerData, _vrpi.LeftController);
        }

        if (RightControllerTransform != null)
        {
            if (!_rightDevice.isValid)
                _rightDevice = InputDevices.GetDeviceAtXRNode(_RightNode);

            _vrpi.RightControllerTracked = UpdateControllerData(_rightDevice, RightControllerTransform,
                _rightControllerData, _vrpi.RightController);
        }

        /*
        if (LeftControllerTransform != null)
        {
            if (!_leftDevice.isValid)
                _leftDevice = InputDevices.GetDeviceAtXRNode(_LeftNode);


            Vector3 headYOnly = HeadTransform.rotation.eulerAngles;
            headYOnly.x = 0;
            headYOnly.z = 0;
            Vector3 forwardVector = Vector3.ProjectOnPlane(HeadTransform.forward, Vector3.up).normalized;
            Vector3 leftDirVector = Vector3.ProjectOnPlane(HeadTransform.right * -1, Vector3.up).normalized;
            Vector3 defaultPosition = HeadTransform.position + forwardVector * (0.308f) + leftDirVector * (0.308f) + Vector3.up * -1f;

            //_vrpi.LeftController.Position = LeftControllerTransform.localPosition.ToVRNVector3();
            //_vrpi.LeftController.Rotation = LeftControllerTransform.localRotation.ToVRNQuaternion();

            if (_leftDevice.isValid && _leftDevice.TryGetFeatureValue(CommonUsages.isTracked, out bool isTracking))
            {

                if (isTracking)
                {
                    _vrpi.LeftControllerTracked = true;
                    _vrpi.LeftController.Position = ComputeRelativePosition(LeftControllerTransform).ToVRNVector3();
                    _vrpi.LeftController.Rotation = ComputeRelativeRotation(LeftControllerTransform).ToVRNQuaternion();
                    if (HeadTransform != null)
                    {
                        _cachedLControllerRelPos = LeftControllerTransform.position - HeadTransform.position;
                    }
                    else
                        _cachedLControllerRelPos = defaultPosition;
                    _cachedLControllerRot = LeftControllerTransform.rotation.eulerAngles;
                }
                else
                {
                    _vrpi.LeftControllerTracked = false;
                    if (HeadTransform != null)
                        _vrpi.LeftController.Position = LeftControllerTransform.parent.InverseTransformPoint(HeadTransform.position + _cachedLControllerRelPos).ToVRNVector3();
                    else
                        _vrpi.LeftController.Position = LeftControllerTransform.parent.InverseTransformPoint(defaultPosition).ToVRNVector3();
                    _vrpi.LeftController.Rotation = Quaternion.Euler(LeftControllerTransform.parent.InverseTransformDirection(_cachedLControllerRot)).ToVRNQuaternion();
                }
            }
            else
            {
                _vrpi.LeftControllerTracked = false;
                _vrpi.LeftController.Position = LeftControllerTransform.parent.InverseTransformPoint(defaultPosition).ToVRNVector3();
                _vrpi.LeftController.Rotation = Quaternion.Euler(LeftControllerTransform.parent.InverseTransformDirection(headYOnly)).ToVRNQuaternion();
            }
            //_vrpi.LeftController.Position = ComputeRelativePosition(LeftControllerTransform).ToVRNVector3();
            //_vrpi.LeftController.Rotation = ComputeRelativeRotation(LeftControllerTransform).ToVRNQuaternion();
        }

        if (RightControllerTransform != null)
        {
            if (!_rightDevice.isValid)
                _rightDevice = InputDevices.GetDeviceAtXRNode(_RightNode);

            //_vrpi.RightController.Position = RightControllerTransform.localPosition.ToVRNVector3();
            //_vrpi.RightController.Rotation = RightControllerTransform.localRotation.ToVRNQuaternion();

            //if (PlayerManager.CurrentPlayer != null)
            //    PlayerManager.CurrentPlayer.SampleControllerStability(RightControllerTransform.position);

            Vector3 headYOnly = HeadTransform.rotation.eulerAngles;
            headYOnly.x = 0;
            headYOnly.z = 0;
            Vector3 forwardVector = Vector3.ProjectOnPlane(HeadTransform.forward, Vector3.up).normalized;
            Vector3 rightDirVector = Vector3.ProjectOnPlane(HeadTransform.right, Vector3.up).normalized;
            Vector3 defaultPosition = HeadTransform.position + forwardVector * (0.308f) + rightDirVector * (0.308f) + Vector3.up * -1;
            if (_rightDevice.isValid && _rightDevice.TryGetFeatureValue(CommonUsages.isTracked, out bool isTracking))
            {

                if (isTracking)
                {
                    _vrpi.RightControllerTracked = true;
                    _vrpi.RightController.Position = ComputeRelativePosition(RightControllerTransform).ToVRNVector3();
                    _vrpi.RightController.Rotation = ComputeRelativeRotation(RightControllerTransform).ToVRNQuaternion();
                    if (HeadTransform != null)
                    {
                        _cachedRControllerRelPos = RightControllerTransform.position - HeadTransform.position;
                    }
                    else
                        _cachedRControllerRelPos = defaultPosition;
                    _cachedRControllerRot = RightControllerTransform.rotation.eulerAngles;
                }
                else
                {
                    _vrpi.RightControllerTracked = false;
                    if (HeadTransform != null)
                        _vrpi.RightController.Position = RightControllerTransform.parent.InverseTransformPoint(HeadTransform.position + _cachedRControllerRelPos).ToVRNVector3();
                    else
                        _vrpi.RightController.Position = RightControllerTransform.parent.InverseTransformPoint(defaultPosition).ToVRNVector3();
                    _vrpi.RightController.Rotation = Quaternion.Euler(RightControllerTransform.parent.InverseTransformDirection(_cachedRControllerRot)).ToVRNQuaternion();
                }
            }
            else
            {
                _vrpi.RightControllerTracked = false;
                _vrpi.RightController.Position = RightControllerTransform.parent.InverseTransformPoint(defaultPosition).ToVRNVector3();
                _vrpi.RightController.Rotation = Quaternion.Euler(RightControllerTransform.parent.InverseTransformDirection(headYOnly)).ToVRNQuaternion();
            }
            //_vrpi.RightController.Position = ComputeRelativePosition(RightControllerTransform).ToVRNVector3();
            //_vrpi.RightController.Rotation = ComputeRelativeRotation(RightControllerTransform).ToVRNQuaternion();
        } */

        if (transform.parent != null)
        {
            _vrpi.RigOffset.Position = transform.localPosition.ToVRNVector3();
            _vrpi.RigOffset.Rotation = transform.localRotation.ToVRNQuaternion();
        }
        else
        {
            _vrpi.RigOffset.Position = Vector3.zero.ToVRNVector3();
            _vrpi.RigOffset.Rotation = Quaternion.identity.ToVRNQuaternion();
        }

        //Debug.Log($"Source Left: {LeftControllerTransform.position.ToString()} Right: {RightControllerTransform.position.ToString()}");
        //Debug.Log($"Send VRInfo {_vrpi.ClientID} Left: {_vrpi.LeftController.Position.ToString()} Right: {_vrpi.RightController.Position.ToString()}");

        //NetManager.SendVRPlayerInfo(_vrpi);

        //Debug.Log($"Sending VRPI LTrack: {_vrpi.LeftControllerTracked} RTrack: {_vrpi.RightControllerTracked}");

        if (PlayerManager.CurrentPlayer.PlayerID >= 0)
        {
            PlayerManager.UpdateLocalVRClientData(_vrpi);
            //NetManager.SendVRPlayerInfo(_vrpi, null, LiteNetLib.DeliveryMethod.Unreliable);
            NetManager.SendNetMessage(VRNPacketType.VrplayerInfo, _vrpi, reliable: false);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (_messageDisplay != null && _messageDisplay.activeSelf)
        {
            var duration = Time.time - _messageDisplayStartTime;
            if (duration > _messageDisplayDuration)
                _messageDisplay.SetActive(false);
        }

        CheckControllerInput(_leftDevice);

        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            Debug.Log("Computing Calibration Offset");
            RecomputeCalibrationOffset(VRNCalibrationSource.CalRightController);
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Debug.Log("Setting floor calibration");
            SetFloorCalibration(VRNCalibrationSource.CalRightController);
        }

        if (PlayerManager.CurrentPlayer != null && RightControllerTransform != null)
            PlayerManager.CurrentPlayer.SampleControllerStability(RightControllerTransform.position, 90);

        float elapsed = Time.time - _lastSend;
        if (elapsed > BroadcastDelay)
        {
            _lastSend = Time.time;

            SendVRPlayerInfo();
        }
    }
}
