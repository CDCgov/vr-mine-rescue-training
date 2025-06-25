using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DebriefVentilationController : SelectedPlayerControl

{
    public NetworkManager NetworkManager;
    public VentilationManager VentilationManager;
    public PlayerManager PlayerManager;
    public DebriefSceneLoader DebriefSceneLoader;

    public TMP_Dropdown VentDropDown;
    public Toggle VentToggle;
    public List<VentVisualizationData> VentVisualDataObjects;

    private bool _visOn = false;
    void Start()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;

        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);


        if (DebriefSceneLoader == null)
            DebriefSceneLoader = FindObjectOfType<DebriefSceneLoader>();

        if(DebriefSceneLoader != null)
        {
            DebriefSceneLoader.SceneLoaded += ResetController;
        }
        //var button = GetComponent<Button>();

        //button.onClick.AddListener(OnButtonClicked);

        if (VentToggle != null)
            VentToggle.onValueChanged.AddListener(delegate { OnToggleChanged(VentToggle); });

        VentDropDown.onValueChanged.AddListener(delegate { OnDropdownChanged(VentDropDown); });

        //make sure the current settings get set in the ventilation manager
        //VentilationManager.ShowVentVisualization(
        //        true,
        //        VentVisualDataObjects[VentDropDown.value]);
        //VentilationManager.ShowVentVisualization(
        //        VentToggle.isOn,
        //        VentVisualDataObjects[VentDropDown.value]);
    }

    private void ResetController()
    {
        if (VentToggle != null)
            VentToggle.isOn = false;
    }

    private void OnSceneChanged(Scene arg0, Scene arg1)
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);
        else
            VentilationManager.VentVisualizationChanged -= OnVentVisualizationChanged;

        VentilationManager.VentVisualizationChanged += OnVentVisualizationChanged;
        
    }

    private void OnVentVisualizationChanged(bool visualizationOn)
    {
        if (VentToggle != null)
            VentToggle.SetIsOnWithoutNotify(visualizationOn);
    }

    void OnToggleChanged(Toggle toggle)
    {
        _visOn = toggle.isOn;
        if (_player != null)
        {
            //PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmSetVentVisualization, (int)ToggleAction);

            VRNVentVisualization ventVis = new VRNVentVisualization();
            //ventVis.ToggleAction = ToggleAction;
            NetworkManager.SendNetMessage(VRNPacketType.SetVentVisualization, ventVis, clientID: _player.ClientID);
        }
        else
        {
            VentilationManager.ShowVentVisualization(
                            _visOn,
                            VentVisualDataObjects[VentDropDown.value]);
        }
    }

    void OnDropdownChanged(TMP_Dropdown dropdown)
    {
        if(dropdown.value >= VentVisualDataObjects.Count)
        {
            Debug.LogError("Vent dropdown box has more options than Vent visualization objects.");
            return;
        }
        VentilationManager.ShowVentVisualization(
                        true,
                        VentVisualDataObjects[dropdown.value]);
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;

        if (DebriefSceneLoader != null)
        {
            DebriefSceneLoader.SceneLoaded -= ResetController;
        }
    }
}
