using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class PlayerInfoPanelController : MonoBehaviour
{
    public TextMeshProUGUI PlayerName;
    public TextMeshProUGUI PlayerInfo;
    public TextMeshProUGUI NetInfo;
    public TextMeshProUGUI BG4Pressure;
    public Image BorderImage;

    public Button FirstPersonButton;
    public Button ThirdPersonButton;
    public BG4SimManager BG4SimManager;
    public NetworkManager NetworkManager;
        
    private int _clientID;
    //private NetworkManager.ClientInfo _clientInfo;
    private float _lastUpdate;
    private PlayerRepresentation _player = null;

    private StringBuilder _sb;

    //public void SetPlayer(PlayerRepresentation player)
    //{
    //    _player = player;
    //    player.PlayerRoleChanged += (role) =>
    //    {

    //    };
    //}

    private void Start()
    {
        _sb = new StringBuilder();

        if (BG4SimManager == null)
            BG4SimManager = BG4SimManager.GetDefault(gameObject);
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
    }


    public void SetPlayerColor(Color color)
    {
        if (BorderImage != null)
        {
            BorderImage.color = color;
        }
    }

    public void SetClient(int clientID)//, NetworkManager.ClientInfo clientInfo)
    {
        _clientID = clientID;
        //_clientInfo = clientInfo;
    }

    public void SetPlayerRepresentation(PlayerRepresentation player)
    {
        _player = player;
    }

    private void Update()
    {
        float elapsed = Time.time - _lastUpdate;
        if (elapsed < 1.0f)
            return;

        _lastUpdate = Time.time;
        //if (NetInfo != null && _clientInfo != null)
        //{
        //    if (_sb == null)
        //        _sb = new StringBuilder();

        //    _sb.Clear();
        //    //_sb.AppendFormat("Ping:{0,3}, OutPkts: {1,6}",
        //    //    _clientInfo.peer.Ping, 
        //    //    _clientInfo.peer.PacketsCountInReliableQueue + _clientInfo.peer.PacketsCountInReliableOrderedQueue);

        //    NetInfo.text = _sb.ToString();
        //    //var status = $"Ping:{_clientInfo.peer.Ping}, OutPkts: {_clientInfo.peer.PacketsCountInReliableQueue + _clientInfo.peer.PacketsCountInReliableOrderedQueue}";
        //    //NetInfo.text = status;
        //}

        try
        {
            var stats = NetworkManager.GetClientStats(_clientID);
            _sb.Clear();
            _sb.AppendFormat("{0,4:F0}ms /{1,4:F0}ms Queue:{2,3:F0} {3,5:F0}kbps",
                stats.AvgPingRTT, 
                stats.ReliablePipelineRTT,
                stats.ReliableSendQueueCount,
                stats.AvgBitRate);

            NetInfo.text = _sb.ToString();
        }
        catch (System.Exception) { }

        var minerProfile = ScenarioSaveLoad.Settings.MinerProfile;
        bool bg4Enabled = true;
        if (minerProfile != null && !minerProfile.EnableBG4)
            bg4Enabled = false;

        if (_player != null && bg4Enabled)
        {
            if (!BG4Pressure.gameObject.activeSelf)
            {
                BG4Pressure.gameObject.SetActive(true);
            }

            var simData = BG4SimManager.GetSimData(_player.PlayerID);
            if (simData != null)
            {
                BG4Pressure.text = $"O2 Pressure: {simData.OxygenPressure}";
            }
            else
            {
                BG4Pressure.text = "";
            }
        }
        else
        {
            BG4Pressure.gameObject.SetActive(false);
        }
    }
}
