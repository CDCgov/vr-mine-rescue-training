using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIControllerCustomRootFolder : MonoBehaviour
{
    public SystemManager SystemManager;

    public TMP_Text ExternalAssetsText;
    public TMP_Text ScenariosText;
    public TMP_Text SessionLogsText;

    public ToggleSwitch CustomPathToggle;
    public TMP_Text CustomFilePathText;
    public Button CustomPathButton;

    public Color DisabledTextColor;

    private Color _enabledTextColor;

    // Start is called before the first frame update
    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        if (CustomPathToggle.TryGetComponent<Toggle>(out var toggle))
        {
            toggle.onValueChanged.AddListener(OnCustomPathToggleChanged);
        }

        CustomPathButton.onClick.AddListener(OnCustomPathButtonClicked);

        _enabledTextColor = CustomFilePathText.color;

        UpdateControls();
    }

    private void OnEnable()
    {
        if (SystemManager == null)
            return;

        UpdateControls();
    }

    private void OnCustomPathButtonClicked()
    {
        if (CustomPathToggle.GetToggleControl().isOn)
        {
            SetRootPath();
        }
    }

    private void OnCustomPathToggleChanged(bool val)
    {
        if (!val)
        {
            SystemManager.SystemConfig.RootDataFolderOverride = null;
            SystemManager.SystemConfig.SaveConfig();
        }
        else
        {
            SystemManager.SystemConfig.RootDataFolderOverride = SystemManager.SystemConfig.RootDataFolder;
        }

        UpdateControls();
    }

    private void SetRootPath()
    {
        var _fileBrowser = new StandaloneFileBrowserWindows();
        string[] paths = _fileBrowser.OpenFolderPanel("Root Path", SystemManager.SystemConfig.RootDataFolder, false);

        if (paths.Length > 0)
        {
            string text = paths[0];
            SystemManager.SystemConfig.RootDataFolderOverride = text;
            SystemManager.SystemConfig.SaveConfig();

        }

        UpdateControls();
    }

    private void UpdateControls()
    {
        ExternalAssetsText.text = "External Assets: " + SystemManager.SystemConfig.ExternalAssetsFolder;
        ScenariosText.text = "Scenarios: " + SystemManager.SystemConfig.ScenariosFolder;
        SessionLogsText.text = "Session Logs: " + SystemManager.SystemConfig.SessionLogsFolder;

        CustomFilePathText.text = SystemManager.SystemConfig.RootDataFolder;

        if (string.IsNullOrEmpty(SystemManager.SystemConfig.RootDataFolderOverride))
        {
            CustomPathToggle.ToggleInstantly(false);
            CustomFilePathText.color = DisabledTextColor;
            CustomPathButton.interactable = false;
        }
        else
        {
            CustomPathToggle.ToggleInstantly(true);
            CustomFilePathText.color = _enabledTextColor;
            CustomPathButton.interactable = true;
        }
    }

}
