using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cinemachine;


[RequireComponent(typeof(CinemachineBrain))]
public class SpectatorCameraController : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;

    public Transform VirtualCameraParent;
    public GameObject VirtualCameraPrefab;

    public VRNPlayerRole SelectedPlayerRole;
    public Vector3 PositionOffset;

    private PlayerRepresentation _targetPlayer;
    private Transform _targetTransform;
    private CinemachineBrain _cineBrain;
    private CinemachineVirtualCameraBase _virtCam;

    private int _overrideID;

    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        PlayerManager.PlayerJoined += OnPlayerJoined;
        PlayerManager.PlayerLeft += OnPlayerLeft;


        var targObj = new GameObject($"{gameObject.name}-Target");
        _targetTransform = targObj.transform;

        _cineBrain = GetComponent<CinemachineBrain>();

        var camObj = Instantiate<GameObject>(VirtualCameraPrefab);
        _virtCam = camObj.GetComponent<CinemachineVirtualCameraBase>();

        _virtCam.LookAt = _targetTransform;
        _virtCam.Follow = _targetTransform;

        foreach (var player in PlayerManager.PlayerList.Values)
        {
            player.PlayerRoleChanged += OnPlayerRoleChanged;
        }

        _overrideID = _cineBrain.SetCameraOverride(-1, _virtCam, _virtCam, 0, -1);

        RetargetPlayer();
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void OnDestroy()
    {
        _cineBrain.ReleaseCameraOverride(_overrideID);
    }

    private void OnPlayerLeft(PlayerRepresentation player)
    {
        player.PlayerRoleChanged -= OnPlayerRoleChanged;
    }

    private void OnPlayerJoined(PlayerRepresentation player)
    {
        player.PlayerRoleChanged += OnPlayerRoleChanged;
        RetargetPlayer();
    }

    private void OnPlayerRoleChanged(PlayerRepresentation player, VRNPlayerRole obj)
    {
        RetargetPlayer();
    }

    private void RetargetPlayer()
    {
        _targetPlayer = null;
        Debug.Log($"{gameObject.name} looking for player to follow {PlayerManager.PlayerList.Count} in sim");
        foreach (var player in PlayerManager.PlayerList.Values)
        {
            Debug.Log($"Player {player.Name} Role: {player.PlayerRole.ToString()}");
            if (player.PlayerRole == SelectedPlayerRole)
            {
                Debug.Log($"{gameObject.name} Found player to follow: {player.Name}");
                _targetPlayer = player;
                break;
            }
        }
    }

    public void FollowPlayer(PlayerRepresentation player)
    {
        _targetPlayer = player;
    }

    void Update()
    {
        //_virtCam.LookAt = _targetTransform;
        //_virtCam.Follow = _targetTransform;

        if (_targetPlayer != null)
        {
            var rot = _targetPlayer.Head.Rotation;
            var pos = _targetPlayer.Head.Position;
            var offset = rot * PositionOffset;
            //pos.y = 0;
            _targetTransform.position = pos + offset;
            _targetTransform.rotation = rot;
        }
        else
        {
            _targetTransform.position = Vector3.zero;
        }
    }
}
