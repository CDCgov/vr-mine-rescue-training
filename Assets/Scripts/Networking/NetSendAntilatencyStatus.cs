using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NetSendAntilatencyStatus : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;
    public AntilatencyManager AntilatencyManager;
    public float UpdateDelay = 3.0f;

    private StringBuilder _statusText;
    private AltPoseDriver _poseDriver;
    private VRNAntilatencyStatus _altStatus;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        if (AntilatencyManager == null)
            AntilatencyManager = AntilatencyManager.GetDefault();

        _altStatus = new VRNAntilatencyStatus();

        _statusText = new StringBuilder(1000);
        AntilatencyManager.NetworkChanged += OnNetworkChanged;
    }

    private void OnEnable()
    {
        //StartCoroutine(SendUpdate());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    void OnNetworkChanged()
    {

    }

    IEnumerator SendUpdate()
    {
        while (true)
        {
            if (NetworkManager == null || !NetworkManager.IsConnected || PlayerManager.CurrentPlayer.PlayerID <= 0)
                yield return new WaitForSeconds(UpdateDelay * 3);

            UpdateStatusText();

            _altStatus.ClientID = NetworkManager.ClientID;
            _altStatus.PlayerID = PlayerManager.CurrentPlayer.PlayerID;
            _altStatus.StatusText = _statusText.ToString();

            NetworkManager.SendNetMessage(VRNPacketType.SendAltStatus, _altStatus, reliable: false);

            yield return new WaitForSeconds(UpdateDelay);
        }
    }

    public void UpdateStatusText()
    {
        try
        {
            _statusText.Clear();
            AppendStatusText(_statusText);
        }
        catch (Exception)
        {

        }
    }

    public void AppendStatusText(StringBuilder statusText)
    {
        try
        {
            if (_poseDriver == null)
            {
                _poseDriver = GameObject.FindObjectOfType<AltPoseDriver>();
            }

            if (AntilatencyManager.Trackers == null || AntilatencyManager.Trackers.Count <= 0)
            {
                statusText.AppendLine("No Trackers Found\n");
                return;
            }

            statusText.AppendLine($"{AntilatencyManager.Trackers.Count} Tracker(s)");

            if (_poseDriver != null)
            {
                statusText.AppendLine(_poseDriver.GetStatusText());
            }
        }
        catch (Exception) { }
    }
}
