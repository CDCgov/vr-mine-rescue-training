using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIResetEquipmentButton : SelectedPlayerControl
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
        PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmResetEquipment, 0);
    }
}

