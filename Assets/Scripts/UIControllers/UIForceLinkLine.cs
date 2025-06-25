using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIForceLinkLine : SelectedPlayerControl
{
    public PlayerManager PlayerManager;
    public bool LinkOn;

    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        var button = GetComponent<Button>();

        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        //PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmSetDominantHand, (int)DominantHand);
        PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmForceLinkLine, LinkOn);
    }
}
