using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PlayerStatusText : MonoBehaviour, IStatusText, ISelectedPlayerView
{
    private PlayerRepresentation _player;

    public void AppendStatusText(StringBuilder statusText)
    {
        if (_player == null)
            return;

        statusText.AppendLine($"Player ID : {_player.PlayerID}");
        statusText.AppendLine($"Client ID : {_player.ClientID}");
        statusText.AppendLine($"PlayerRole: {_player.PlayerRole.ToString()}");
        statusText.AppendLine($"Handedness: {_player.PlayerDominantHand.ToString()}");
        statusText.AppendLine($"OnLinkLine: {_player.OnLinkLine}");
    }

    public void SetPlayer(PlayerRepresentation player)
    {
        _player = player;
    }
}
