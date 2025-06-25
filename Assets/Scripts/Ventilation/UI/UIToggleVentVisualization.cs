using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[RequireComponent(typeof(Toggle))]
public class UIToggleVentVisualization : SelectedPlayerControl
{
    public NetworkManager NetworkManager;
    public VentilationManager VentilationManager;
    public PlayerManager PlayerManager;
    public VentVisualizationData VisualizationData;

    public bool ToggleDM = false;
    public bool TogglePlayers = false;
    public bool ToggleSpectators = false;

    private Toggle _toggle;


    void Start()
    {

        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        _toggle = GetComponent<Toggle>();

        if (_player != null)
        {
            //_toggle.SetIsOnWithoutNotify(VentilationManager.i);
            //_player.UserTeleportEnabledChanged += OnPlayerValueChanged;
        }

        _toggle.onValueChanged.AddListener(OnValueChanged);

        VentilationManager.VentVisChanged += OnVentVisChanged;
        OnVentVisChanged();

    }

    private void OnVentVisChanged()
    {
        if (ToggleDM)
        {
            _toggle.SetIsOnWithoutNotify(VentilationManager.IsVisEnabledDM);
        }
        else if (TogglePlayers)
        {
            _toggle.SetIsOnWithoutNotify(VentilationManager.IsVisEnabledPlayer);
        }
        else if (ToggleSpectators)
        {
            _toggle.SetIsOnWithoutNotify(VentilationManager.IsVisEnabledSpectator);
        }
    }

    private void OnPlayerValueChanged(bool val)
    {
        _toggle.SetIsOnWithoutNotify(val);
    }

    private void OnValueChanged(bool val)
    {
        if (ToggleDM || TogglePlayers || ToggleSpectators)
        {
            if (ToggleDM)
                VentilationManager.IsVisEnabledDM = val;
            if (TogglePlayers)
                VentilationManager.IsVisEnabledPlayer = val;
            if (ToggleSpectators)
                VentilationManager.IsVisEnabledSpectator = val;
        }
        else
        {
            //old behavior - attempt to toggle the vis effect on a specific player
            if (_player == null)
                return;

            VRNVentVisualization ventVis = new VRNVentVisualization();
            ventVis.ToggleAction = val ? VRNToggleAction.ToggleOn : VRNToggleAction.ToggleOff;

            NetworkManager.SendNetMessage(VRNPacketType.SetVentVisualization, ventVis, clientID:_player.ClientID);
        }
    }
}
