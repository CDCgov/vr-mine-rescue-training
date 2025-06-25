using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIResetToTeleportButton : SelectedPlayerControl
{
    public PlayerManager PlayerManager;

    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        var button = GetComponent<Button>();

        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (PlayerManager == null)
            return;

        if (_player != null)
        {
            PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmResetToTeleport, 0);
        }
        else if (PlayerManager.CurrentPlayer != null)
        {
            PlayerManager.SendPlayerMessage(PlayerManager.CurrentPlayer.PlayerID, VRNPlayerMessageType.PmResetToTeleport, 0);
        }
    }
}