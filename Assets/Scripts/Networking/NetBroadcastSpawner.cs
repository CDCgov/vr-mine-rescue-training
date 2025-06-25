using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetBroadcastSpawner : MonoBehaviour
{

    public NetworkManager NetManager;
    public PlayerColorManager PlayerColorManager;
    public PlayerRoleManager PlayerRoleManager;
    public PlayerManager PlayerManager;
    public TeleportManager TeleportManager;
    public SceneLoadManager SceneLoadManager;
    public LoadableAssetManager LoadableAssetManager;
    
    //public GameObject PlayerPrefab;
    public GameObject LeftControllerPrefab;
    public GameObject RightControllerPrefab;
    public Vector3 HeadRotationPreset = new Vector3(0, -90, -90);

    public GameObject PlayerPrefabOverride;

    private bool SmoothTranslation = true;
    private bool SmoothRotation = true;

    private float MoveSpeed = 4.0f;
    private float RotationSpeed = 360;

    private int _raycastMask;
    private MinerProfile _minerProfile;
    private GameObject _playerPrefab;
    private string _playerPrefabID = "";

    // Start is called before the first frame update
    void Start()
    {
        if (NetManager == null)
            NetManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        if (TeleportManager == null)
            TeleportManager = TeleportManager.GetDefault(gameObject);
        if (PlayerColorManager == null)
            PlayerColorManager = PlayerColorManager.GetDefault();
        if (PlayerRoleManager == null)
            PlayerRoleManager = PlayerRoleManager.GetDefault();
        if (SceneLoadManager == null)
            SceneLoadManager = SceneLoadManager.GetDefault(gameObject);
        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);

        PlayerManager.PlayerJoined += OnPlayerJoined;
        PlayerManager.PlayerLeft += OnPlayerLeft;

        TeleportManager.Teleporting += OnTeleport;

        _raycastMask = LayerMask.GetMask("Floor");

        foreach (var player in PlayerManager.PlayerList.Values)
        {
            OnPlayerJoined(player);
        }
    }

    private void OnDestroy()
    {
        if (TeleportManager != null)
            TeleportManager.Teleporting -= OnTeleport;

        if (PlayerManager != null)
        {
            PlayerManager.PlayerJoined -= OnPlayerJoined;
            PlayerManager.PlayerLeft -= OnPlayerLeft;
        }

        foreach (var player in PlayerManager.PlayerList.Values)
        {
            OnPlayerLeft(player);
        }
    }

    private void OnTeleport(Transform obj)
    {
        UpdateParentObject();
    }

    private void OnPlayerLeft(PlayerRepresentation player)
    {
        DespawnPlayer(player);
        player.PlayerRoleChanged -= OnPlayerRoleChanged;
    }

    private void OnPlayerJoined(PlayerRepresentation player)
    {
        player.PlayerRoleChanged += OnPlayerRoleChanged;
    }

    private void OnPlayerRoleChanged(PlayerRepresentation player, VRNPlayerRole playerRole)
    {
        UpdatePlayerVisual(player, playerRole);
    }

    private void GetPlayerPrefab(PlayerRepresentation player, out GameObject prefab, out string prefabID)
    {
        prefab = _playerPrefab;
        prefabID = _playerPrefabID;

        var roleData = PlayerRoleManager.GetPlayerRoleData(player.PlayerRole);
        if (roleData != null && roleData.ThirdPersonPrefabOverride != null)
        {
            prefab = roleData.ThirdPersonPrefabOverride;
            prefabID = prefab.name;
        }
    }


    void UpdatePlayerVisual(PlayerRepresentation player, VRNPlayerRole role)
    {
        if (player == null || player.Head.Object == null || player.Head.Object.transform == null)
            return;

        Debug.Log($"NetBroadcastSpawner: Updating player visual for {player.PlayerID}");

        GetPlayerPrefab(player, out var prefab, out var prefabID);
        if (prefabID != player.SpawnedPrefabID)
        {
            DespawnPlayer(player);
            SpawnPlayer(player);
            return;
        }

        var playerObj = player.Head.Object;


        var minerColor = playerObj.GetComponent<MinerColorChanger>();
        var LaserHelper = playerObj.GetComponent<ThirdPersonLaserHelper>();

        if (PlayerColorManager != null)
        {
            var newColor = PlayerColorManager.GetPlayerColor(role);
            player.PlayerColor = newColor;
        }

        if (minerColor != null)
        {
            minerColor.MinerColor = player.PlayerColor;
            minerColor.UpdateMiner();
        }

        //Mine map player laser enable
        if(player.PlayerRole == VRNPlayerRole.MapMan)
        {
            switch (player.PlayerDominantHand)
            {
                case PlayerDominantHand.RightHanded:
                    LaserHelper.EnableRightLaser();
                    break;
                case PlayerDominantHand.LeftHanded:
                    LaserHelper.EnableLeftLaser();
                    break;
                default:
                    break;
            }
        }
        else if (LaserHelper != null)
        {
            LaserHelper.DisableLasers();
        }

        //temporary - should add a controller to keep track of the number textures
        var textTextures = playerObj.GetComponentsInChildren<TextTexture>();
        if (textTextures != null)
        {
            foreach (var textTexture in textTextures)
            {
                if (textTexture.CompareTag("PlayerName"))
                {
                    string pName = player.Name;
                    if (pName.Length > 12)
                    {
                        pName = player.Name.Substring(0, 12);
                    }
                    textTexture.Text = pName;
                }
                else
                {
                    textTexture.Text = ((int)role).ToString();
                }
                textTexture.UpdateTexture();
            }
        }

        player.InvokePlayerVisualUpdated();

    }

    void UpdateParentObject()
    {
        foreach (var player in PlayerManager.PlayerList.Values)
        {
            if (player.RigTransform != null)
            {
                player.RigTransform.SetParent(TeleportManager.ActiveTeleportTarget, false);
            }

            //if (player.Head.Object == null)
            //{
            //    continue;
            //}

            //if (player.Head.Object != null)
            //    player.Head.Object.transform.SetParent(TeleportManager.ActiveTeleportTarget, false);

            //if (player.RightController.Object != null)
            //    player.RightController.Object.transform.SetParent(TeleportManager.ActiveTeleportTarget, false);

            //if (player.LeftController.Object != null)
            //    player.LeftController.Object.transform.SetParent(TeleportManager.ActiveTeleportTarget, false);
        }
    }

    private void DespawnPlayer(PlayerRepresentation player)
    {
        if (player.Head.Object != null)
        {
            Destroy(player.Head.Object);
            player.Head.Object = null;
        }

        if (player.RigTransform != null)
        {
            Destroy(player.RigTransform.gameObject);
            player.RigTransform = null;
        }

        //player.PlayerRoleChanged -= OnPlayerRoleChanged;
    }

    GameObject SpawnPlayer(PlayerRepresentation player)
    {
        MinerIK minerIK = null;
        MinerFinalIK minerFinalIK = null;
        GameObject headOffset = null;
        //GameObject headTarget = null;

        GetPlayerPrefab(player, out var prefab, out var prefabID);

        Debug.Log($"Spawning Player {player.ClientID} prefab {prefabID}");
        player.Head.Object = Instantiate<GameObject>(prefab);
        player.SpawnedPrefabID = prefabID;

        var headObj = new GameObject($"Head_{player.Name}_{player.PlayerID}");
        var rigObj = new GameObject($"Rig_{player.Name}_{player.PlayerID}");
        var offsetObj = new GameObject($"Offset_{player.Name}_{player.PlayerID}");

        player.RigTransform = rigObj.transform;
        player.CalibrationTransform = offsetObj.transform;
        player.HeadTransform = headObj.transform;

        headOffset = new GameObject($"Head_Offset_{player.Name}_{player.PlayerID}");
        headOffset.transform.SetParent(player.HeadTransform);
        //headOffset.transform.localPosition = Vector3.zero;
        headOffset.transform.localPosition = new Vector3(0, /*-0.0662744f*/0.025f, -0.1042147f);
        //headOffset.transform.localEulerAngles = new Vector3(0, -90, -90);
        headOffset.transform.localEulerAngles = HeadRotationPreset;

        player.CalibrationTransform.localPosition = player.CalibrationPos;
        player.CalibrationTransform.localRotation = player.CalibrationRot;

        //setup hierarchy to match rig (POI Anchor -> Rig -> Offset -> Player)
        if (TeleportManager.ActiveTeleportTarget != null)
        {
            player.RigTransform.SetParent(TeleportManager.ActiveTeleportTarget, false);
        }
        else
        {
            Debug.LogError($"Player {player.Name} spawned but no teleport POI active");
        }
        player.CalibrationTransform.SetParent(player.RigTransform, false);
        player.Head.Object.transform.SetParent(player.CalibrationTransform, false);
        player.HeadTransform.SetParent(player.CalibrationTransform, false);

        ThirdPersonGripHandler thirdPersonGripHandler = player.Head.Object.GetComponent<ThirdPersonGripHandler>();
        if (thirdPersonGripHandler != null)
        {
            thirdPersonGripHandler.SetPlayerRep(player);
            thirdPersonGripHandler.ID = player.PlayerID;
            thirdPersonGripHandler.ConfigurePlayerManagerAndEventHandler(PlayerManager);
        }

        MinePlayerInfo mplayerInfo = player.Head.Object.GetComponent<MinePlayerInfo>();
        if(mplayerInfo != null)
        {
            mplayerInfo.PlayerID = player.PlayerID;
        }

        //leftHandOffset.transform.SetParent(player.LeftController.tr)

        //player.Head.Object.transform.SetParent(TeleportManager.ActiveTeleportTarget, false);

        UpdatePlayerVisual(player, player.PlayerRole);

        player.PlayerObject = player.Head.Object;

        //player.PlayerRoleChanged += (playerRole) =>
        //{
        //    UpdatePlayerVisual(player, playerRole);
        //};


        CustomXRSocket[] xrSocks = player.Head.Object.transform.GetComponentsInChildren<CustomXRSocket>();
        Debug.Log("Player socket count: " + xrSocks.Length);
        foreach (CustomXRSocket sock in xrSocks)
        {
            sock.SocketID = $"{player.PlayerID}_{sock.SocketID}";
            sock.PlayerID = player.PlayerID;

            //sock._setExternally = true;
            //sock.RegisterWithSocketManager();
            sock.AssignSocketName(player.ClientID);
            //Debug.Log(sock.gameObject.name);
        }

        minerIK = player.Head.Object.GetComponent<MinerIK>();
        minerFinalIK = player.Head.Object.GetComponent<MinerFinalIK>();

        if (minerFinalIK != null)
        {
            var headTarget = new GameObject($"Head_Target_{player.Name}_{player.PlayerID}");
            HeadRotationCopy rotCopy = headTarget.AddComponent<HeadRotationCopy>();
            rotCopy.Head = headOffset.transform;
            //minerFinalIK.SetHead(headOffset.transform);//Scale height?
            //player.PlayerHeight = headOffset.transform.position.y;
            Debug.Log(player.Name + ": " + player.PlayerHeight);
            minerFinalIK.SetHead(headTarget.transform, headOffset.transform);//Variable height

            minerFinalIK.Player = player;
        }

        return player.Head.Object;
    }

    private GameObject CreateControllerObj(GameObject prefab, Transform parent)
    {
        GameObject controllerObj = null;

        //player.LeftController.Object = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        controllerObj = Instantiate<GameObject>(prefab);
        //player.LeftController.Object.transform.SetParent(TeleportManager.ActiveTeleportTarget, false);
        controllerObj.transform.SetParent(parent, false);
        controllerObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        /*ikOffset= new GameObject("ControllerIKOffset");
        ikOffset.transform.SetParent(controllerObj.transform);
        //ikOffset.transform.localPosition = new Vector3(-0.5f, 0, -1f);//May need to correct this offset
        //ikOffset.transform.localEulerAngles = new Vector3(90, 0, -90);
        ikOffset.transform.localPosition = offsetPos;
        ikOffset.transform.localRotation = offsetRot;*/

        return controllerObj;
    }

    void UpdatePlayerRepresentation(PlayerRepresentation player)
    {
        if (player.Head.Object == null)
        {
            SpawnPlayer(player);
        }
        MinerFinalIK minerFinalIK = null;

        minerFinalIK = player.Head.Object.GetComponent<MinerFinalIK>();
        player.RigTransform.localRotation = player.RigOffset.Rotation;
        player.RigTransform.localPosition = player.RigOffset.Position;

        //update head transform with raw tracking data
        player.HeadTransform.localPosition = player.Head.Position;
        player.HeadTransform.localRotation = player.Head.Rotation;

        var player_t = player.Head.Object.transform;

        //compute body position & rotation by moving the head position to zero y
        var bodyPos = player.Head.Position;

        //keep player model on zero ground plane - change this to use the world space ground plane
        //bodyPos.y = 0;
        var bodyPosGlobal = player_t.parent.TransformPoint(bodyPos);
        //bodyPosGlobal.y = 0;

        var dist = bodyPos.y + 1.0f;

        //raycast to find floor
        if (Physics.Raycast(bodyPosGlobal, Vector3.down, out var hit, dist, _raycastMask))
        {
            bodyPosGlobal.y = hit.point.y;
        }
        else if (player.Head.Object.transform.parent != null)
        {
            bodyPosGlobal.y = player.Head.Object.transform.parent.position.y;
        }
        else
        {
            bodyPosGlobal.y = 0;
        }

        var leftPosition = player.LeftController.Position;
        var rightPosition = player.RightController.Position;

        //compute rotation based on hand position
        var v1 = (rightPosition - leftPosition).normalized;
        var vforward = Vector3.Cross(v1, Vector3.up);
        vforward.y = 0;

        //var bodyYaw = Quaternion.FromToRotation(Vector3.forward, vforward);
        var bodyYaw = Quaternion.identity;
        if (vforward.x >= 0.1 || vforward.z >= 0.1)
            bodyYaw = Quaternion.LookRotation(vforward, Vector3.up);
        else
        {
            var headEuler = player.Head.Rotation.eulerAngles;
            headEuler.x = 0;
            headEuler.z = 0;
            bodyYaw = Quaternion.Euler(headEuler);
        }


        var translateDist = Vector3.Distance(player_t.position, bodyPosGlobal);
        if (SmoothTranslation && translateDist < 0.5f)
        {
            player_t.position = Vector3.MoveTowards(player_t.position, bodyPosGlobal, Time.deltaTime * MoveSpeed);
        }
        else
        {
            player.Head.Object.transform.position = bodyPosGlobal;
        }

        //player.Head.Object.transform.position = bodyPosGlobal;

        if (SmoothRotation)
        {
            player_t.localRotation = Quaternion.RotateTowards(player_t.localRotation, bodyYaw, Time.deltaTime * RotationSpeed);
            //player.Head.Object.transform.localRotation = bodyYaw;
        }
        else
        {
            player.Head.Object.transform.localRotation = bodyYaw;
        }


        /*
        if (player.LeftController.Object == null)
        {
            player.LeftController.Object = CreateControllerObj(LeftControllerPrefab, player.CalibrationTransform,
                new Vector3(-0.5f, 0, -1f), Quaternion.Euler(90, 0, -90), out var ikOffset);

            if (minerFinalIK != null)
                minerFinalIK.SetLeftHand(ikOffset.transform);
        }

        if (player.RightController.Object == null)
        {
            player.RightController.Object = CreateControllerObj(RightControllerPrefab, player.CalibrationTransform,
                new Vector3(0.5f, 0, -1f), Quaternion.Euler(-90, 0, 90), out var ikOffset);

            if (minerFinalIK != null)
                minerFinalIK.SetRightHand(ikOffset.transform);
        }
        */

        if (minerFinalIK == null)
            return;

        if (player.LeftController.Object == null)
        {
            if (minerFinalIK == null || minerFinalIK.LeftHandTarget == null || minerFinalIK.LeftHandTarget.parent == null)
                player.LeftController.Object = CreateControllerObj(LeftControllerPrefab, player.CalibrationTransform);
            else
            {
                minerFinalIK.LeftHandTarget.parent.SetParent(player.CalibrationTransform, false);
                player.LeftController.Object = minerFinalIK.LeftHandTarget.parent.gameObject;
            }
        }

        if (player.RightController.Object == null)
        {
            if (minerFinalIK == null || minerFinalIK.RightHandTarget == null || minerFinalIK.RightHandTarget.parent == null)
                player.RightController.Object = CreateControllerObj(RightControllerPrefab, player.CalibrationTransform);
            else
            {
                minerFinalIK.RightHandTarget.parent.SetParent(player.CalibrationTransform, false);
                player.RightController.Object = minerFinalIK.RightHandTarget.parent.gameObject;
            }
        }
        if (player.LeftControllerTracked || NetManager.IsPlaybackMode)
        {
            player.LeftController.Object.transform.localPosition = player.LeftController.Position;
            player.LeftController.Object.transform.localRotation = player.LeftController.Rotation;
            if(minerFinalIK.VRIK.solver.leftArm.rotationWeight <= 0)
            {
                minerFinalIK.VRIK.solver.leftArm.rotationWeight = 1;
            }
        }
        else
        {
            UntrackedControllerUpdater(player.LeftController.Object.transform, player.PlayerObject.transform, new Vector3(-0.3f, 1f, 0.141f));
            //Just let FinalIK handle the rotation of the hands if it's untracked.
            if (minerFinalIK.VRIK.solver.rightArm.rotationWeight != 0)
            {
                minerFinalIK.VRIK.solver.leftArm.rotationWeight = 0;
            }
            
        }
        if (player.RightControllerTracked || NetManager.IsPlaybackMode)
        {
            player.RightController.Object.transform.localPosition = player.RightController.Position;
            player.RightController.Object.transform.localRotation = player.RightController.Rotation;
            if (minerFinalIK.VRIK.solver.rightArm.rotationWeight <= 0)
            {
                minerFinalIK.VRIK.solver.rightArm.rotationWeight = 1;
            }
        }
        else
        {
            UntrackedControllerUpdater(player.RightController.Object.transform, player.PlayerObject.transform, new Vector3(0.3f, 1f, 0.141f));
            if (minerFinalIK.VRIK.solver.rightArm.rotationWeight != 0)
            {
                minerFinalIK.VRIK.solver.rightArm.rotationWeight = 0;
            }
            
        }
    }

    void UntrackedControllerUpdater(Transform controllerTransform, Transform calibrationTransform, Vector3 target)
    {
        if (SmoothTranslation)
        {
            controllerTransform.position = Vector3.MoveTowards(controllerTransform.position, calibrationTransform.TransformPoint(target), Time.deltaTime * MoveSpeed);
        }
        else
        {
            controllerTransform.position = calibrationTransform.TransformPoint(target);
        }
        //if (isRightHand)
        //{
        //    controllerTransform.position = calibrationTransform.TransformPoint(new Vector3(0.3f, 0.9f, 0.141f));
        //    //controllerTransform.eulerAngles = calibrationTransform.parent.TransformDirection(20, 0, 0);
        //}
        //else
        //{
        //    controllerTransform.position = calibrationTransform.TransformPoint(new Vector3(-0.3f, 0.9f, 0.141f));
        //    //controllerTransform.eulerAngles = calibrationTransform.parent.TransformDirection(20, 0, 0);
        //}
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (NetManager == null || SceneLoadManager.LoadInProgress)
            return;

        //initialize the player prefab here, after loading is complete
        if (_playerPrefab == null && PlayerPrefabOverride != null)
        {
            _playerPrefab = PlayerPrefabOverride;
            Debug.Log($"NetBroadcastSpawner: Using player prefab override {PlayerPrefabOverride.name}");

            _playerPrefabID = PlayerPrefabOverride.name;
        }
        else if (_playerPrefab == null && _minerProfile == null)
        {
            var profileID = ScenarioSaveLoad.Settings.MinerProfileID;

            _minerProfile = LoadableAssetManager.FindMinerProfile(profileID);
            _playerPrefab = _minerProfile.ThirdPersonPrefab;

            Debug.Log($"NetBroadcastSpawner: Using miner profile ID {_minerProfile.MinerProfileID}");

            _playerPrefabID = _minerProfile.MinerProfileID;
        }

        foreach (var player in PlayerManager.PlayerList.Values)
        {
            UpdatePlayerRepresentation(player);
        }
    }
}
