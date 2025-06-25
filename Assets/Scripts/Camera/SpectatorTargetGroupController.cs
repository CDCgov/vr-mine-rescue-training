using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using Cinemachine;
using System;
using System.Threading.Tasks;

[RequireComponent(typeof(CinemachineTargetGroup))]
public class SpectatorTargetGroupController : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;
    public TeleportManager TeleportManager;
    public SceneLoadManager SceneLoadManager;

    public bool SinglePlayer = false;
    public bool TeleportPointOnly = false;
    public VRNPlayerRole SelectedPlayerRole;

    private CinemachineTargetGroup _targetGroup;
    private List<CinemachineTargetGroup.Target> _targetList;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        if (TeleportManager == null)
            TeleportManager = TeleportManager.GetDefault(gameObject);
        if (SceneLoadManager == null)
            SceneLoadManager = SceneLoadManager.GetDefault(gameObject);

        _targetGroup = GetComponent<CinemachineTargetGroup>();
        _targetList = new List<CinemachineTargetGroup.Target>();

        PlayerManager.PlayerJoined += OnPlayerJoined;
        PlayerManager.PlayerLeft += OnPlayerLeft;

        foreach (var player in PlayerManager.PlayerList.Values)
        {
            player.PlayerRoleChanged += OnPlayerRoleChanged;
            player.PlayerVisualUpdated += OnPlayerVisualUpdated;
        }

        TeleportManager.Teleporting += OnTeleporting;

        SceneManager.activeSceneChanged += OnSceneChanged;

        SceneLoadManager.EnteredSimulationScene += OnEnteredSimulationScene;

        RebuildTargetGroup();
    }

    private async void OnEnteredSimulationScene()
    {
        await Task.Delay(150);
        RebuildTargetGroup();
    }

    private async void OnSceneChanged(Scene arg0, Scene arg1)
    {
        await Task.Delay(150);
        RebuildTargetGroup();
    }

    private void OnTeleporting(Transform obj)
    {
        RebuildTargetGroup();
    }

    private void OnPlayerVisualUpdated()
    {
        RebuildTargetGroup();
    }

    private void OnPlayerLeft(PlayerRepresentation player)
    {
        player.PlayerRoleChanged -= OnPlayerRoleChanged;
    }

    private void OnPlayerJoined(PlayerRepresentation player)
    {
        player.PlayerRoleChanged += OnPlayerRoleChanged;
        RebuildTargetGroup();
    }

    private void OnPlayerRoleChanged(PlayerRepresentation player, VRNPlayerRole obj)
    {
        RebuildTargetGroup();
    }

    private void RebuildTargetGroup()
    {
        if (_targetGroup == null)
            return;

        _targetList.Clear();


        if (!TeleportPointOnly)
        {
            foreach (var player in PlayerManager.PlayerList.Values)
            {
                if (SinglePlayer && player.PlayerRole != SelectedPlayerRole)
                    continue;

                if (player.HeadTransform == null)
                    continue;

                AddTarget(player.HeadTransform);
                if (SinglePlayer) //stop adding if in single player mode
                    break;
            }
        }


        if (TeleportManager.ActiveTeleportTarget != null && (!SinglePlayer || _targetList.Count <= 0))
        {
            AddTarget(TeleportManager.ActiveTeleportTarget);
        }

        if (_targetList.Count > 0)
            _targetGroup.m_Targets = _targetList.ToArray();

        //Debug.Log($"{gameObject.name} looking for player to follow {PlayerManager.PlayerList.Count} in sim");
        //foreach (var player in PlayerManager.PlayerList.Values)
        //{
        //    Debug.Log($"Player {player.Name} Role: {player.PlayerRole.ToString()}");
        //    if (player.PlayerRole == SelectedPlayerRole)
        //    {
        //        Debug.Log($"{gameObject.name} Found player to follow: {player.Name}");
        //        _targetPlayer = player;
        //        break;
        //    }
        //}
    }

    private void AddTarget(Transform targetTransform)
    {
        var cineTarget = new CinemachineTargetGroup.Target
        {
            target = targetTransform,
            radius = 2,
            weight = 1,
        };

        _targetList.Add(cineTarget);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
