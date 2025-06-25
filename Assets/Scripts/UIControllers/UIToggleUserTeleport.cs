using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UIToggleUserTeleport : SelectedPlayerControl
{
    public PlayerManager PlayerManager;
    private Toggle _toggle;
    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        _toggle = GetComponent<Toggle>();

        UpdateToggleState();

        if (_player != null)
        {            
            _player.UserTeleportEnabledChanged += OnPlayerValueChanged;
        }

        _toggle.onValueChanged.AddListener(OnValueChanged);
    }

    private void UpdateToggleState()
    {
        if (PlayerManager == null && _player == null)
            return;

        var player = _player;
        if (player == null)
            player = PlayerManager.CurrentPlayer;

        if (player != null)
        {
            _toggle.SetIsOnWithoutNotify(player.UserTeleportEnabled);
        }
    }

    private void OnEnable()
    {
        UpdateToggleState();
    }

    private void OnDestroy()
    {
        if (_player != null)
            _player.UserTeleportEnabledChanged -= OnPlayerValueChanged;
    }

    private void OnPlayerValueChanged(bool val)
    {
        _toggle.SetIsOnWithoutNotify(val);
    }

    private void OnValueChanged(bool val)
    {
        var player = _player;
        if (player == null)
            player = PlayerManager.CurrentPlayer;

        if (player == null)
            return;

        PlayerManager.SendPlayerMessage(player.PlayerID, VRNPlayerMessageType.PmToggleUserTeleport, val);
    }
}
