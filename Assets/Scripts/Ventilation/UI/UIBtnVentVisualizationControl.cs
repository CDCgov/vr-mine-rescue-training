using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIBtnVentVisualizationControl: SelectedPlayerControl
{

    public NetworkManager NetworkManager;
    public VentilationManager VentilationManager;
    public PlayerManager PlayerManager;
    
    public VRNToggleAction ToggleAction;
    public VentVisualizationData VisualizationData;


    void Start()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;

        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        var button = GetComponent<Button>();

        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene arg0, Scene arg1)
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);
    }

    private void OnButtonClicked()
    {
        if (VentilationManager == null)
            return;

        if (_player != null)
        {
            //PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmSetVentVisualization, (int)ToggleAction);

            VRNVentVisualization ventVis = new VRNVentVisualization();
            ventVis.ToggleAction = ToggleAction;

            NetworkManager.SendNetMessage(VRNPacketType.SetVentVisualization, ventVis, clientID:_player.ClientID);
        }
        else
        {
            switch (ToggleAction)
            {
                case VRNToggleAction.ToggleOff:
                    VentilationManager.ShowVentVisualization(false);
                    break;

                case VRNToggleAction.ToggleOn:
                    VentilationManager.ShowVentVisualization(true, VisualizationData);
                    break;

                default:
                case VRNToggleAction.ToggleSwitch:
                    VentilationManager.ShowVentVisualization(
                        !VentilationManager.IsVentVisualizationVisible, 
                        VisualizationData);
                    break;
            }
            
        }
    }
}
