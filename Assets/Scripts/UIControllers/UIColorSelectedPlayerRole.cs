using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIColorSelectedPlayerRole : MonoBehaviour, ISelectedPlayerView
{
    public PlayerColorManager PlayerColorManager;

    private PlayerRepresentation _player;

    private Graphic _graphic;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerColorManager == null)
            PlayerColorManager = PlayerColorManager.GetDefault();

        _graphic = GetComponent<Graphic>();
        UpdateColor();
    }

    void OnDestroy()
    {
        ClearPlayer();
    }

    private void ClearPlayer()
    {
        if (_player != null)
        {
            _player.PlayerRoleChanged -= OnPlayerRoleChanged;
        }
        _player = null;
    }

    public void SetPlayer(PlayerRepresentation player)
    {
        ClearPlayer();

        _player = player;
        _player.PlayerRoleChanged += OnPlayerRoleChanged;
        UpdateColor();
    }

    private void OnPlayerRoleChanged(PlayerRepresentation player, VRNPlayerRole obj)
    {
        UpdateColor();
    }

    void UpdateColor()
    {
        if (_player == null || PlayerColorManager == null)
            return;

        //var color = _player.PlayerColor;
        var color = PlayerColorManager.GetPlayerColor(_player.PlayerRole);

        if (_graphic != null)
        {
            _graphic.color = color;
        }

    }
}
