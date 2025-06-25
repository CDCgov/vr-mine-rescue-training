using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AntilatencyNetworkStatusText : MonoBehaviour, IStatusText, ISelectedPlayerView
{
    public PlayerManager PlayerManager;

    private PlayerRepresentation _player;

    public void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
    }

    public void AppendStatusText(StringBuilder statusText)
    {
        if (_player != null)
        {
            statusText.AppendLine(_player.AntilatencyStatusText);
        }
    }

    public void SetPlayer(PlayerRepresentation player)
    {
        _player = player;
    }
}
