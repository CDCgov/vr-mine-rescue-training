using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UIToggleDeafenButton : SelectedPlayerControl
{
    public PlayerManager PlayerManager;
    public Toggle UIToggle;

    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        UIToggle = GetComponent<Toggle>();
        UIToggle.onValueChanged.AddListener(OnValueChanged);
        
        if (_player != null)
        {
            UIToggle.SetIsOnWithoutNotify(_player.DeafenEnabled);
            _player.DeafenEnabledChanged += OnPlayerValueChanged;
        }
    }

    private void OnDestroy()
    {
        if (_player != null)
            _player.DeafenEnabledChanged -= OnPlayerValueChanged;
    }

    private void OnPlayerValueChanged(bool val)
    {
        UIToggle.SetIsOnWithoutNotify(val);
    }

    private void OnValueChanged(bool val)
    {
        if (_player == null)
            return;

        PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmDeafenPlayer, val);
        _player.DeafenEnabled = val;
    }
}
