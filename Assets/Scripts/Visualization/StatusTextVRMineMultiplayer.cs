using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatusTextVRMineMultiplayer : MonoBehaviour, IStatusText
{
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;
    public SpectatorManager SpectatorManager;
    public SystemManager SystemManager;

    public bool IncludePlayerList = true;
    public bool IncludeSpectatorList = true;
    public bool IncludeDebugText = false;

    public bool UpdateTextDirectly = true;

    private StringBuilder _statusText;
    private TextMeshProUGUI _textMeshUGUI;
    private TextMeshPro _textMesh;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        GitVersion.UpdateVersion();
        GitVersion.SaveVersion();
#else
        GitVersion.ReadVersion();
#endif

        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        if (SpectatorManager == null)
            SpectatorManager = SpectatorManager.GetDefault(gameObject);
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        _statusText = new StringBuilder(1000);

        _textMesh = GetComponent<TextMeshPro>();
        _textMeshUGUI = GetComponent<TextMeshProUGUI>();

        if (_textMesh == null && _textMeshUGUI == null)
        {
            Debug.LogError("No text mesh on object with StatusTextVRMineMultiplayer");
            enabled = false;
            return;
        }

        if (UpdateTextDirectly)
            UpdateStatusText();
    }

    private void OnEnable()
    {
        if (UpdateTextDirectly)
            StartCoroutine(UpdateStatusTextCo());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
    IEnumerator UpdateStatusTextCo()
    {
        yield return new WaitForEndOfFrame();

        while (true)
        {
            UpdateStatusText();    

            yield return new WaitForSeconds(0.2f);
        }
    }

    public void UpdateStatusText()
    {
        if (_statusText == null || SystemManager == null || PlayerManager == null || NetworkManager == null)
            return;

        if (!UpdateTextDirectly)
            return;

        try
        {
            _statusText.Clear();

            AppendStatusText(_statusText);

            string statusText = _statusText.ToString();

            if (_textMesh != null)
                _textMesh.text = statusText;
            if (_textMeshUGUI != null)
                _textMeshUGUI.text = statusText;
        }
        catch (Exception ex)
        {
            Debug.LogError($"StatusText: Error updating - {ex.Message}");
        }
    }

    public void AppendStatusText(StringBuilder statusText)
    {
        statusText.AppendLine($"Welcome to the VR Mine Version {GitVersion.Version}\n");

        if (IncludeDebugText)
            NetworkManager.GetStatusText(statusText);
        else
        {
            if (NetworkManager.IsConnected)
                statusText.AppendLine("Connected");
            else
                statusText.AppendLine("Connecting....");
        }

        //_statusText.AppendLine();

        if (IncludePlayerList && PlayerManager != null && PlayerManager.PlayerList != null)
        {
            //if (NetworkManager.IsServer)
            //    _statusText.AppendLine($"Connected Players: {PlayerManager.PlayerList.Count}");
            //else
            //    _statusText.AppendLine($"Other Players: {PlayerManager.PlayerList.Count}");


            var name = SystemManager.SystemConfig.MultiplayerName;
            if (name == null || name.Length <= 0)
                name = SystemInfo.deviceName;

            if (PlayerManager.CurrentPlayer != null && PlayerManager.CurrentPlayer.PlayerObject != null)
            {
                statusText.AppendLine($"Connected Players: {PlayerManager.PlayerList.Count + 1}");
                var pinfo = PlayerManager.CurrentPlayer;

                if (IncludeDebugText)
                    statusText.AppendLine($"{name} {NetworkManager.ClientID}:{pinfo.PlayerID}");
                else
                    statusText.AppendLine($"{name} ({((int)pinfo.PlayerRole).ToString()})");
            }
            else
            {
                statusText.AppendLine($"Connected Players: {PlayerManager.PlayerList.Count}");
            }

            foreach (var player in PlayerManager.PlayerList.Values)
            {
                if (IncludeDebugText)
                    statusText.AppendLine($"{player.Name} {player.ClientID}:{player.PlayerID}");
                else
                    statusText.AppendLine($"{player.Name} ({((int)player.PlayerRole).ToString()})");
            }
        }

        if (IncludeSpectatorList && SpectatorManager != null && SpectatorManager.Spectators != null)
        {
            statusText.AppendLine($"Connected Spectators: {SpectatorManager.Spectators.Count}");
        }
    }
}
