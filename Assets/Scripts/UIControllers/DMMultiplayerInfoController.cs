using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public class DMMultiplayerInfoController : MonoBehaviour, IMinimizableWindow
{
    public NetworkManager NetworkManager;
    public PlayerColorManager PlayerColorManager;
    public PlayerManager PlayerManager;
    public SpectatorManager SpectatorManager;

    public DMCameraController DMCamera;

    public TMPro.TextMeshProUGUI StatusText;

    public RectTransform PlayerInfoList;
    public GameObject PlayerInfoPrefab;

    private Dictionary<int, PlayerInfoPanelController> _playerInfo;

    public event Action<string> TitleChanged;

    private StringBuilder _statusText = new StringBuilder();

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerColorManager == null)
            PlayerColorManager = PlayerColorManager.GetDefault();
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        if (SpectatorManager == null)
            SpectatorManager = SpectatorManager.GetDefault(gameObject);

        _playerInfo = new Dictionary<int, PlayerInfoPanelController>();

        //NetworkManager.ClientStateChanged.AddListener(OnClientStateChanged);
        NetworkManager.ClientStateChanged += OnClientStateChanged;

        PlayerManager.PlayerJoined += OnPlayerJoined;
        PlayerManager.PlayerLeft += OnPlayerLeft;

        SpectatorManager.SpectatorListChanged += OnSpectatorListChanged;
        SpectatorManager.SpectatorJoined += OnSpectatorJoined;
        SpectatorManager.SpectatorLeft += OnSpectatorLeft;
    }

    private void OnSpectatorLeft(int clientID)
    {
        PlayerInfoPanelController panel;

        if (_playerInfo.TryGetValue(clientID, out panel))
        {
            _playerInfo.Remove(clientID);
            Destroy(panel.gameObject);
        }
    }

    private void OnSpectatorJoined(int clientID)
    {
        UpdateClientList();
    }

    private void OnSpectatorListChanged()
    {

    }

    private void OnPlayerLeft(PlayerRepresentation player)
    {
        PlayerInfoPanelController panel;

        if (_playerInfo.TryGetValue(player.ClientID, out panel))
        {
            _playerInfo.Remove(player.ClientID);
            Destroy(panel.gameObject);
        }
    }

    private void OnPlayerJoined(PlayerRepresentation player)
    {
        UpdateClientList();
    }

    void UpdateClientList()
    {
        if (PlayerInfoList == null || PlayerInfoPrefab == null)
            return;

        foreach (var player in PlayerManager.PlayerList.Values)
        {
            if (_playerInfo.ContainsKey(player.ClientID))
                continue;

            var pinfo = AddPanel(player.ClientID, player.Name, player.PlayerColor, player);

            player.PlayerColorChanged += (newColor) =>
            {
                pinfo.SetPlayerColor(newColor);
            };
        }

        foreach (var spectator in SpectatorManager.Spectators)
        {
            if (_playerInfo.ContainsKey(spectator.ClientID))
                continue;

            AddPanel(spectator.ClientID, spectator.Name + " (Spectator)", Color.white, null);
        }
    }

    PlayerInfoPanelController AddPanel(int clientID, string name, Color playerColor, PlayerRepresentation player)
    {
        //Addressables.LoadAssetAsync<GameObject>("AssetAddress").Completed += OnLoadDone;
        var obj = Instantiate<GameObject>(PlayerInfoPrefab, PlayerInfoList, false);
        var pinfo = obj.GetComponent<PlayerInfoPanelController>();

        if (player != null)
        {
            var playerViews = obj.GetComponentsInChildren<ISelectedPlayerView>();
            foreach (var playerView in playerViews)
            {
                playerView.SetPlayer(player);
            }
        }

        //pinfo.SetPlayerColor(PlayerColorManager.GetPlayerColor(player.ClientID));
        pinfo.SetPlayerColor(playerColor);
        pinfo.SetPlayerRepresentation(player);
        if (DMCamera != null)
        {
            if (pinfo.FirstPersonButton != null)
            {
                pinfo.FirstPersonButton.onClick.AddListener(() => { DMCamera.SwitchToFirstPerson(clientID); });
            }

            if (pinfo.ThirdPersonButton != null)
            {
                pinfo.ThirdPersonButton.onClick.AddListener(() => { DMCamera.SwitchToThirdPerson(clientID); });
            }
        }

        //var clientInfo = NetworkManager.GetClientInfo(clientID);
        pinfo.SetClient(clientID);//, clientInfo);

        _playerInfo.Add(clientID, pinfo);
        UpdateStatus(clientID, name, "");

        //obj.transform.SetParent(PlayerInfoList);

        return pinfo;
    }

    void OnClientStateChanged(VRNClientState state)
    {
        Debug.Log($"Got ClientStateChange: {state.ToString()}");
        string status = "";
        switch (state.SceneLoadState)
        {
            case VRNSceneLoadState.Loading:
                status = $"Loading...";
                break;

            case VRNSceneLoadState.ReadyToActivate:
                status = "Ready";
                break;

            default:
                status = "Active";
                break;
        }

        UpdateStatus(state.ClientID, state.PlayerName, status);
    }

    void UpdateStatus(int clientID, string name, string status)
    {
        PlayerInfoPanelController pinfo;
        if (_playerInfo.TryGetValue(clientID, out pinfo))
        {
            pinfo.PlayerName.text = name;
            pinfo.PlayerInfo.text = status;
        }

    }

    void Update()
    {
        if (StatusText != null)
        {
            _statusText.Clear();
            //_statusText.AppendLine($"FramePkts: {NetworkManager.LastFramePacketsSent,-7} ({NetworkManager.AvgFramePackets,-7:F1})");
            //_statusText.AppendLine($"FrameByts: {NetworkManager.LastFrameBytesSent,-7} ({NetworkManager.AvgFrameBytes,-7:F1})");
            //_statusText.AppendLine($"BytPerSec: {NetworkManager.LastBytesPerSecond,-7} ({NetworkManager.AvgBytesPerSecond,-7:F1})");

            float graphicsMem = (float)(SystemInfo.graphicsMemorySize);

            float curTexMem = (float)(Texture2D.currentTextureMemory) / 1024.0f / 1024.0f;
            float desTexMem = (float)(Texture2D.desiredTextureMemory) / 1024.0f / 1024.0f;
            float totalTexMem = (float)(Texture2D.totalTextureMemory) / 1024.0f / 1024.0f;

            _statusText.AppendFormat("GPU Memory: {0:F1} MiB\n", graphicsMem);
            _statusText.AppendFormat("Desired: {1:F1} MiB\n", curTexMem, desTexMem);
            //_statusText.AppendFormat("Current   : {0,5:F1}MiB Desired: {1:F1}\n", curTexMem, desTexMem);
            //_statusText.AppendFormat("Total     : {0:F1}MiB\n", totalTexMem);

            StatusText.text = _statusText.ToString();

        }
    }

    public string GetTitle()
    {
        return "Player Info";
    }

    public void Minimize(bool minimize)
    {
        gameObject.SetActive(minimize);
    }

    public void ToggleMinimize()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void AssignTaskbarButton(Button button)
    {
        
    }
}
