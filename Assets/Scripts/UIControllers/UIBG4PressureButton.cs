using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBG4PressureButton : SelectedPlayerControl
{
    public PlayerManager PlayerManager;
    public int PressureToSet;

    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        var button = GetComponent<Button>();

        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (_player == null)
        {
            PlayerManager.SendPlayerMessageToAll(VRNPlayerMessageType.PmSetBg4Pressure, PressureToSet);
        }
        else
        {
            PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmSetBg4Pressure, PressureToSet);
        }
    }
}
