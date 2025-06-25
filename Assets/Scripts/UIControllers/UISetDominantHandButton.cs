using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UISetDominantHandButton : SelectedPlayerControl
{
    public PlayerManager PlayerManager;
    public PlayerDominantHand DominantHand;

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
        PlayerManager.AssignPlayerRole(_player.PlayerID, _player.PlayerRole, DominantHand);
    }
}

