using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedPlayerControl : MonoBehaviour, ISelectedPlayerView
{
    protected PlayerRepresentation _player;

    public void SetPlayer(PlayerRepresentation player)
    {
        _player = player;
    }
}
