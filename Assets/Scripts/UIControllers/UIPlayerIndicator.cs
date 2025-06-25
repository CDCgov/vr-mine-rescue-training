using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPlayerIndicator : MonoBehaviour, ISelectedPlayerView
{
    public const float MIN_FRAME_RATE = 50;

    public GameObject TargetObject;

    public bool LowFrameRate = false;
    public bool LowBattery = false;
    public bool LostFocus = false;

    private PlayerRepresentation _player;

    private bool _needsUpdate = true;
    private float _batteryBlinkCooldown = 1;
    private bool _blinkPrior = true;

    // Start is called before the first frame update
    void Start()
    {
        if(_player != null)
            UpdateIndicator();

        if(_player == null)
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (_needsUpdate && _player != null)
            UpdateIndicator();
    }

    void UpdateIndicator()
    {
        if (TargetObject == null || _player == null)
            return;

        _needsUpdate = false;

        bool setActive = false;

        if (LowFrameRate && _player.FrameRate < MIN_FRAME_RATE)
            setActive = true;
        if (LowBattery && _player.IsBatteryLow())
        {
            if (!_player.IsBatteryCritical())
            {
                setActive = true;
            }
            else
            {
                if (Time.time > _batteryBlinkCooldown)
                {
                    _blinkPrior = !_blinkPrior;
                    _batteryBlinkCooldown = Time.time + 0.5f;
                }
                setActive = _blinkPrior;
            }
        }
            
        if (LostFocus && (!_player.AppFocused || _player.AppPaused))
            setActive = true;

        TargetObject.SetActive(setActive);
    }

    public void ClearPlayer()
    {
        if (_player == null)
            return;

        _player.BatteryStatusChanged -= OnPlayerStatusChanged;
        _player.ClientStateChanged -= OnPlayerClientStateChanged;
        _player.FrameRateChanged -= OnPlayerStatusChanged;

        _player = null;
    }

    public void SetPlayer(PlayerRepresentation player)
    {
        _player = player;

        _player.BatteryStatusChanged += OnPlayerStatusChanged;
        _player.ClientStateChanged += OnPlayerClientStateChanged;
        _player.FrameRateChanged += OnPlayerStatusChanged;

        UpdateIndicator();

        if(_player != null && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    private void OnPlayerClientStateChanged(VRNClientState obj)
    {
        _needsUpdate = true;
    }

    private void OnPlayerStatusChanged()
    {
        _needsUpdate = true;
    }
}
