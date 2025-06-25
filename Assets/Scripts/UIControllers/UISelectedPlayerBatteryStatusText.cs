using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UISelectedPlayerBatteryStatusText : MonoBehaviour, ISelectedPlayerView
{
    public string FormatString = "PlayerName: {name}";


    private PlayerRepresentation _player;
    private TMP_Text _text;

    // Start is called before the first frame update
    void Start()
    {
        _text = GetComponent<TMP_Text>();
        UpdateText();
    }

    public void SetPlayer(PlayerRepresentation player)
    {
        if (_player != null)
        {
            _player.BatteryStatusChanged -= OnBatteryStatusChanged;
            _player = null;
        }

        _player = player;
        _player.BatteryStatusChanged += OnBatteryStatusChanged;

        UpdateText();

        if(_player == null)
        {
            _text.enabled = false;
        }
    }

    private void OnBatteryStatusChanged()
    {
        UpdateText();
    }

    void UpdateText()
    {
        string txt = "";
        if (_player != null && _player.BatteryStatus != null)
        {
            var batStatus = _player.BatteryStatus;
            
            batStatus = batStatus.Replace("\n", "");
            batStatus = batStatus.Replace("\r", "");
            batStatus = batStatus.Replace("\t", " ");

            txt = $"Batt: {batStatus}";
        }

        if (_text != null)
            _text.text = txt;
    }

}
