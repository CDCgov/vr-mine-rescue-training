using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UISelectedPlayerFrameRate : MonoBehaviour, ISelectedPlayerView
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
            player.FrameRateChanged -= OnFrameRateChanged;
            _player = null;
        }

        _player = player;
        _player.FrameRateChanged += OnFrameRateChanged;

        UpdateText();
    }

    private void OnFrameRateChanged()
    {
        UpdateText();
    }

    void UpdateText()
    {
        string txt = "";
        if (_player != null)
        {
            txt = $"FPS: {_player.FrameRate:F1}";
        }

        if (_text != null)
            _text.text = txt;
    }

}
