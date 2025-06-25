using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Google.Protobuf;

public class ScenarioTimer : MonoBehaviour
{
    private MeshRenderer _labelMeshRenderer;
    private TMPro.TextMeshPro _tmProLabel;
    private NetworkedObject _netObj;
    private float _startTime;

    private double _seconds = 0;

    private int _priorSec = 0;
    private bool _stopTimer = false;
    

    private void Start()
    {
        _labelMeshRenderer = GetComponent<MeshRenderer>();
        _tmProLabel = GetComponent<TMPro.TextMeshPro>();
        _netObj = GetComponentInParent<NetworkedObject>();

        if(_netObj != null)
        {
            if (_netObj.HasAuthority)
            {
                _startTime = Time.time;
                //InvokeRepeating(nameof(DoTimeCheck), 1.0f, 1.0f);
                ResetTimerActionHandle rst = _netObj.gameObject.AddComponent<ResetTimerActionHandle>();
                StartTimerHandler startTH = _netObj.gameObject.AddComponent<StartTimerHandler>();
                StopTimerHandler stopTH = _netObj.gameObject.AddComponent<StopTimerHandler>();
                rst.ScenTimer = this;
                startTH.ScenTimer = this;
                stopTH.ScenTimer = this;
            }

            _netObj.RegisterMessageHandler(ClientSync);

        }
        _priorSec = Mathf.RoundToInt(Time.time);

        if (_netObj != null && _netObj.NetManager != null && _netObj.NetManager.IsPlaybackMode)
        {
            this.enabled = false;
        }
    }

    private void Update()
    {
        if (_stopTimer)
        {
            return;
        }
        if (_netObj != null)
        {
            if (_netObj.HasAuthority && (Mathf.RoundToInt(Time.time) != _priorSec))
            {
                DoTimeCheck();
                _priorSec = Mathf.RoundToInt(Time.time);
            }
        }
    }

    private void ClientSync(string messageType, CodedInputStream reader)
    {
        var msg = new VRNFloatData();
        reader.ReadMessage(msg);
        if (messageType == "TIMER_SYNC")
        {
            _seconds = msg.FloatData;
            UpdateWatchText();
        }
    }

    private void OnDestroy()
    {
        if (_netObj != null)
        {
            CancelInvoke();
            _netObj.UnregisterMessageHandler(ClientSync);
        }
    }

    void DoTimeCheck()
    {
        if (_netObj.HasAuthority)
        {
            _seconds += (Mathf.RoundToInt(Time.time) - _priorSec);
            SyncTime();
            UpdateWatchText();
        }
    }

    void SyncTime()
    {
        VRNFloatData vRNFloatData = new VRNFloatData();
        vRNFloatData.FloatData = (float)_seconds;
        _netObj.SendMessage("TIMER_SYNC", vRNFloatData);
    }

    public void ResetTimer()
    {
        _startTime = Time.time;
        _priorSec = Mathf.RoundToInt(Time.time);
        _seconds = 0;
        
        DoTimeCheck();
    }

    public void StartTimer()
    {
        _priorSec = Mathf.RoundToInt(Time.time);
        _stopTimer = false;
    }

    public void StopTimer()
    {
        _stopTimer = true;
    }

    void UpdateWatchText()
    {
        if (!_labelMeshRenderer.enabled)
            return;

        TimeSpan t = TimeSpan.FromSeconds(_seconds);
        string hour = t.Hours.ToString("00");
        string minute = t.Minutes.ToString("00");
        string second = t.Seconds.ToString("00");
        
        _tmProLabel.text = $"{hour}:{minute}:{second}";
        

    }
}
