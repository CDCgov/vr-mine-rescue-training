using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UIToggleMuteAll : MonoBehaviour
{
    public PlayerManager PlayerManager;
    private Toggle _toggle;

    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(bool val)
    {
        foreach (KeyValuePair<int, PlayerRepresentation> kvp in PlayerManager.PlayerList)
        {
            PlayerManager.SendPlayerMessage(kvp.Key, VRNPlayerMessageType.PmMutePlayer, val);
        }
        //PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmDeafenPlayer, val);
    }
}
