using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BG4Sim))]
public class NetSendBG4SimData : MonoBehaviour
{
    public SystemManager SystemManager;
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;
    public BG4SimManager BG4SimManager;

    public float BroadcastDelay = 0.025f;

    private float _lastSend = 0;
    private VRNBG4SimData _bg4data;

    //private PlayerInfo _playerInfo;
    private BG4Sim _bg4Sim;
    private VRNBG4SimData _simData;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        if (BG4SimManager == null)
            BG4SimManager = BG4SimManager.GetDefault(gameObject);

        _simData = new VRNBG4SimData();

        //_playerInfo = GetComponent<PlayerInfo>();
        //if (_playerInfo == null)
        //{
        //    Debug.LogError("NetSendBg4SimData can't find PlayerInfo component");
        //    enabled = false;
        //    return;
        //}


        _bg4data = new VRNBG4SimData();
        _bg4data.ClientID = NetworkManager.ClientID;
        //_bg4data.PlayerID = _playerInfo.PlayerID;
        _bg4data.PlayerID = PlayerManager.CurrentPlayer.PlayerID;

        _bg4Sim = GetComponent<BG4Sim>();

        PlayerManager.RegisterPlayerMessageHandler(OnPlayerMessage);
    }

    private void OnDestroy()
    {
        if (PlayerManager != null)
        {
            PlayerManager.UnregisterPlayerMessageHandler(OnPlayerMessage);
        }
    }

    private void OnPlayerMessage(VRNPlayerMessageType messageType, VRNPlayerMessage msg)
    {
        Debug.Log($"Got player message {messageType.ToString()} {msg.ToString()}");
        switch (messageType)
        {
            case VRNPlayerMessageType.PmSetBg4Pressure:
                if (msg.PlayerMessageDataCase == VRNPlayerMessage.PlayerMessageDataOneofCase.IntData)
                {
                    SetBG4Pressure(msg.IntData);
                }
                break;

            case VRNPlayerMessageType.PmSilenceBg4Alarm:
                if (msg.PlayerMessageDataCase == VRNPlayerMessage.PlayerMessageDataOneofCase.IntData)
                {
                    if (msg.IntData > 0)
                        SilenceBG4Alarm(true);
                    else
                        SilenceBG4Alarm(false);
                }
                break;
        }
    }

    private void SetBG4Pressure(int pressure)
    {
        Debug.Log($"Setting BG4 Pressure to {pressure}");
        if (_bg4Sim == null)
            return;

        _bg4Sim.OxygenPressure = pressure;
    }

    private void SilenceBG4Alarm(bool silence)
    {
        Debug.Log($"Setting bg4 alarm silence: {silence}");
        if (_bg4Sim == null)
            return;

        _bg4Sim.SilenceAlarm();

    }

    // Update is called once per frame
    void Update()
    {
        //if (_playerInfo == null || _playerInfo.PlayerID < 0)
        //    return;
        if (PlayerManager.CurrentPlayer.PlayerID < 0)
            return;

        float elapsed = Time.time - _lastSend;
        if (elapsed > BroadcastDelay)
        {
            _lastSend = Time.time;

            if (NetworkManager != null && NetworkManager.IsInGame)
            {
                _simData.ClientID = NetworkManager.ClientID;
                //_simData.PlayerID = _playerInfo.PlayerID;
                _simData.PlayerID = PlayerManager.CurrentPlayer.PlayerID;
                //_simData.AlarmState = VRNBG4AlarmState.Off;
                _simData.AlarmState = _bg4Sim.GetAlarmState();
                _simData.OxygenPressure = _bg4Sim.OxygenPressure;
                _simData.BaselinePressure = _bg4Sim.BaselinePressure;
                _simData.RemainingTime = _bg4Sim.GetRemainingTime();
                _simData.LowPressure = _bg4Sim.CheckLowPressure();
                _simData.CriticalPressure = _bg4Sim.CheckCriticalPressure();

                BG4SimManager.UpdateLocalBG4SimData(PlayerManager.CurrentPlayer.PlayerID, _simData);
            }
        }
    }
}
