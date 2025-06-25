using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIImageColorControllerTracked : MonoBehaviour, ISelectedPlayerView
{
    public bool RequireLeftControllerTracked = false;
    public bool RequireRightControllerTracked = false;

    public Color TrackedColor = Color.green;
    public Color UntrackedColor = Color.red;

    private PlayerRepresentation _player;
    private Image _image;

    void Start()
    {
        _image = GetComponent<Image>();

        UpdateColor();
    }

    void OnDestroy()
    {
        ClearPlayer();
    }

    public void ClearPlayer()
    {
        if (_player != null)
        {
            _player.PlayerControllerTrackedChanged -= OnControllerTrackedChanged;
            _player = null;
        }
    }

    public void SetPlayer(PlayerRepresentation player)
    {
        ClearPlayer();

        _player = player;
        _player.PlayerControllerTrackedChanged += OnControllerTrackedChanged;

        UpdateColor();
    }

    private void OnControllerTrackedChanged()
    {
        UpdateColor();
    }

    void UpdateColor()
    {
        if (_player == null || _image == null)
            return;

        var color = TrackedColor;
        if ((RequireLeftControllerTracked && !_player.LeftControllerTracked) ||
            (RequireRightControllerTracked && !_player.RightControllerTracked))
            color = UntrackedColor;

        _image.color = color;
    }
}
