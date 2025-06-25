using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UIToggleDebugInterface : SelectedPlayerControl
{
    public PlayerManager PlayerManager;
    private Toggle _toggle;

    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        _toggle = GetComponent<Toggle>();

        if (_player != null)
        {
            _toggle.SetIsOnWithoutNotify(_player.DebugInterfaceEnabled);
            _player.DebugInterfaceEnabledChanged += OnPlayerValueChanged;
        }

        _toggle.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDestroy()
    {
        if (_player != null)
            _player.DebugInterfaceEnabledChanged -= OnPlayerValueChanged;
    }

    private void OnPlayerValueChanged(bool val)
    {
        _toggle.SetIsOnWithoutNotify(val);
    }

    private void OnValueChanged(bool val)
    {
        if (_player == null)
            return;

        PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmToggleDebugMode, val);
    }
}
