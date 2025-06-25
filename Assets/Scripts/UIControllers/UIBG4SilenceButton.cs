using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBG4SilenceButton : SelectedPlayerControl
{
    public PlayerManager PlayerManager;
    public bool SilenceAlarm = true;

    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        var button = GetComponent<Button>();

        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmSilenceBg4Alarm, SilenceAlarm ? 1 : 0);
    }
}
