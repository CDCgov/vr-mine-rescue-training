using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerSpawner : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public SceneLoadManager SceneLoadManager;
    public PlayerManager PlayerManager;
    public POIManager POIManager;

    //Main player rig
    public GameObject PlayerRigPrefab;
    //Player rig which ignores all network messages, to be used when not connected
    public GameObject LobbyPlayerRigPrefab;
    public bool DebugMode = false;

    //[System.NonSerialized]
    //public int PlayerID = -1;
    //[System.NonSerialized]
    //public VRNPlayerRole PlayerRole;

    private GameObject _playerRig = null;
    private GameObject _audioListenerObj;

    public void Awake()
    {
        
    }

    IEnumerator Start()
    {
        _audioListenerObj = new GameObject("AudioListener");
        _audioListenerObj.AddComponent<AudioListener>();

        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (SceneLoadManager == null)
            SceneLoadManager = SceneLoadManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        if (POIManager == null)
            POIManager = POIManager.GetDefault(gameObject);

        yield return new WaitForEndOfFrame();

        NetworkManager.ClientIDAssigned += OnClientIDAssigned;
        PlayerManager.PlayerIDAssigned += OnPlayerIDAssigned;
        //PlayerManager.PlayerRoleAssigned += OnPlayerRoleAssigned;
        //NetworkManager.PlayerIDAssigned += OnPlayerIDAssigned;

        SceneLoadManager.EnteredSimulationScene += OnEnteredSimulationScene;

        if (NetworkManager.ClientConnected)
        {
            //we already have a client ID
            OnClientIDAssigned(NetworkManager.ClientID);
        }

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
    {
        if (_audioListenerObj != null)
            Destroy(_audioListenerObj);

        _audioListenerObj = new GameObject("AudioListener");
        _audioListenerObj.AddComponent<AudioListener>();
    }

    private void OnDestroy()
    {
        if (SceneLoadManager != null)
        {
            SceneLoadManager.EnteredSimulationScene -= OnEnteredSimulationScene;
        }

        if (NetworkManager != null)
        {
            NetworkManager.ClientIDAssigned -= OnClientIDAssigned;
        }

        if (PlayerManager != null)
        {
            PlayerManager.PlayerIDAssigned -= OnPlayerIDAssigned;
        }
    }

    //private void OnPlayerRoleAssigned(VRNAssignPlayerRole msg)
    //{
    //    var playerID = PlayerManager.CurrentPlayer.PlayerID;
    //    Debug.Log($"PlayerSpawner: received role assign for {msg.PlayerID} role {msg.Role.ToString()}, our ID {playerID}");
    //    if (msg.PlayerID == playerID)
    //        PlayerRole = msg.Role;
    //}

    private void OnPlayerIDAssigned(int playerID)
    {
        Debug.Log($"PlayerSpawner({gameObject.name}): Player ID Assigned: {playerID}");
        //PlayerID = playerID;

        if (_playerRig == null && SceneLoadManager.InSimulationScene)
        {
            SpawnPlayerRig();
        }
        else if (SceneLoadManager.InSimulationScene)
        {
            Destroy(_playerRig);
            SpawnPlayerRig();
        }
    }

    private void OnClientIDAssigned(int clientID)
    {
        Debug.Log($"PlayerSpawner({gameObject.name}): Client ID Assigned: {clientID}");
        if (PlayerManager.CurrentPlayer.PlayerID < 0)
        {
            PlayerManager.RequestPlayerID();
        }
    }

    private void OnEnteredSimulationScene()
    {
        if (_playerRig == null)
        {
            SpawnPlayerRig();
        }
    }

    private void SpawnPlayerRig()
    {
        //await Task.Delay(1000);


        GameObject rigPrefab = null;
        if (PlayerManager.CurrentPlayer.PlayerID >= 0)
        {
            Debug.Log($"Spawning Player Rig, Player ID {PlayerManager.CurrentPlayer.PlayerID}");
            rigPrefab = PlayerRigPrefab;
        }
        else
        {
            Debug.Log($"Spawning Lobby Player Rig, Player ID {PlayerManager.CurrentPlayer.PlayerID}");
            rigPrefab = LobbyPlayerRigPrefab;
        }

        //if (PlayerManager.CurrentPlayer.PlayerID < 0)
        //{
        //    Debug.LogWarning("Spawning player rig with invalid player ID");
        //}

        if (rigPrefab == null)
        {
            Debug.LogError("PlayerSpawner missing rig prefab");
            return;
        }

        Debug.Log("PlayerSpawner: Loading player rig");

        _playerRig = GameObject.Instantiate<GameObject>(rigPrefab, new Vector3(0, 1000, 0), Quaternion.identity);
        if (_playerRig.TryGetComponent<XRRig>(out var xrrig))
        {
            if (xrrig.cameraGameObject != null && _audioListenerObj != null)
            {
                _audioListenerObj.transform.SetParent(xrrig.cameraGameObject.transform, false);
                _audioListenerObj.transform.localPosition = Vector3.zero;
                _audioListenerObj.transform.localRotation = Quaternion.identity;
            }
        }

        PlayerManager.CurrentPlayer.RigTransform = _playerRig.transform;

        var netSend = _playerRig.GetComponent<NetSendVRPlayerInfo>();
        if (netSend != null)
        {
            netSend.PlayerManager = PlayerManager;
            netSend.NetManager = NetworkManager;

            PlayerManager.CurrentPlayer.RightController.Object = netSend.RightControllerTransform.gameObject;
            PlayerManager.CurrentPlayer.LeftController.Object = netSend.LeftControllerTransform.gameObject;
        }


        //var playerInfo = _playerRig.GetComponent<PlayerInfo>();
        //if (playerInfo != null)
        //{
        //    //playerInfo.PlayerID = PlayerID;
        //    //playerInfo.PlayerRole = PlayerRole;
        //    playerInfo.PlayerSpawner = this;
        //    PlayerManager.CurrentPlayer = playerInfo;
        //}
        //else
        //{
        //    Debug.LogWarning($"Warning: No PlayerInfo component on player rig {_playerRig.name}");
        //}

        //var spawnPoint = POIManager.GetFirstOfType(POIType.SpawnPoint);
        //if (spawnPoint != null)
        //{
        //    Debug.Log($"Spawning player at {spawnPoint.transform.position.ToString()}");
        //    _playerRig.transform.position = spawnPoint.transform.position;
        //    _playerRig.transform.rotation = spawnPoint.transform.rotation;
        //}
        //else
        //{
        //    Debug.LogWarning("Coulding find spawn point for player");
        //}

        if (DebugMode)
        {
            PlayerManager.CurrentPlayer.DebugInterfaceEnabled = true;
            PlayerManager.CurrentPlayer.TranslationEnabled = true;
            PlayerManager.CurrentPlayer.RotationEnabled = true;
            PlayerManager.CurrentPlayer.UserTeleportEnabled = true;
            //ConfigureMinerXRRig configureMinerXRRig = _playerRig.GetComponentInChildren<ConfigureMinerXRRig>();
            //if(configureMinerXRRig != null)
            //{
            //    configureMinerXRRig.DebugMode = DebugMode;
            //}
            //else
            //{
            //    Debug.LogWarning("Couldn't find the xr rig config component.");
            //}
        }
    }
}
