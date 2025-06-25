using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetSendSystemStatus : MonoBehaviour
{
    public const float FrameRateWindowSeconds = 3.0f;
    public NetworkManager NetworkManager;

    private VRNSystemStatus _vrnSystemStatus;

    private float _startTime;
    private int _startFrame;
    private bool _appFocused = true;
    private bool _appPaused = false;

    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);

        _vrnSystemStatus = new VRNSystemStatus();

        Reset();
    }

    private void Reset()
    {
        _startTime = Time.time;
        _startFrame = Time.frameCount;
    }

    private void OnApplicationFocus(bool focus)
    {
        _appFocused = focus;
    }

    private void OnApplicationPause(bool pause)
    {
        _appPaused = pause;
    }

    private void Update()
    {
        float elapsed = Time.time - _startTime;
        if (elapsed > FrameRateWindowSeconds)
        {
            float frameCount = Time.frameCount - _startFrame;
            float frameRate = ((float)frameCount) / elapsed;

            SendFrameRate(frameRate);
            Reset();
            //Debug.Log($"Sent frame rate {frameRate:F1}");
        }
    }

    void SendFrameRate(float fps)
    {
        _vrnSystemStatus.ClientID = NetworkManager.ClientID;
        _vrnSystemStatus.FrameRate = fps;
        _vrnSystemStatus.AppFocused = _appFocused;
        _vrnSystemStatus.AppPaused = _appPaused;

        Debug.Log($"Sending system stats fps:{fps:F2} focused:{_appFocused} paused:{_appPaused}");

        NetworkManager.SendNetMessage(VRNPacketType.SendSystemStatus, _vrnSystemStatus, broadcast: false);
    }
}
