using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UISelectedPlayerNameText : MonoBehaviour, ISelectedPlayerView
{
    public string FormatString = "PlayerName: {name}";

    private PlayerRepresentation _player;
    private TextMeshProUGUI _guiText;
    private TextMeshPro _text;

    // Start is called before the first frame update
    void Start()
    {
        _text = GetComponent<TextMeshPro>();
        _guiText = GetComponent<TextMeshProUGUI>();

        UpdateText();
    }

    void OnDestroy()
    {
        ClearPlayer();
    }

    public void ClearPlayer()
    {
        if (_player == null)
            return;

        _player.PlayerNameChanged -= OnPlayerNameChanged;
        _player.PlayerRoleChanged -= OnPlayerRoleChanged;
        _player = null;
    }

    public void SetPlayer(PlayerRepresentation player)
    {
        ClearPlayer();

        _player = player;
        _player.PlayerNameChanged += OnPlayerNameChanged;
        _player.PlayerRoleChanged += OnPlayerRoleChanged;

        UpdateText();
    }

    private void OnPlayerRoleChanged(PlayerRepresentation player, VRNPlayerRole role)
    {
        UpdateText();
    }

    private void OnPlayerNameChanged(string newName)
    {
        UpdateText();
    }

    void UpdateText()
    {
        string txt;
        if (_player == null)
            txt = FormatString.Replace("{name}", "Unknown");
        else
        {
            txt = FormatString.Replace("{name}", _player.Name);
            txt = txt.Replace("{role}", _player.PlayerRole.ToString());
        }

        if (_guiText != null)
            _guiText.text = txt;
        if (_text != null)
            _text.text = txt;
    }

}
