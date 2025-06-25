using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BG4StatusText : MonoBehaviour, IStatusText, ISelectedPlayerView
{
    public BG4SimManager BG4SimManager;
    private int _playerID;
    private PlayerRepresentation _player;

    void Start()
    {
        if (BG4SimManager == null)
            BG4SimManager = BG4SimManager.GetDefault(gameObject);
    }

    public void AppendStatusText(StringBuilder statusText)
    {
        if (_player != null)
            _playerID = _player.PlayerID;

        if (BG4SimManager == null)
            return;

        var simData = BG4SimManager.GetSimData(_playerID);
        if (simData != null)
        {
            statusText.AppendLine($"O2 Pressure: {simData.OxygenPressure}");
            statusText.AppendLine($"Alarm State: {simData.AlarmState.ToString()}");
            statusText.AppendLine($"Rem Time   : {simData.RemainingTime}");
        }
    }

    public void SetPlayerID(int playerID)
    {
        _playerID = playerID;
    }

    public void SetPlayer(PlayerRepresentation player)
    {
        _player = player;
    }
}
