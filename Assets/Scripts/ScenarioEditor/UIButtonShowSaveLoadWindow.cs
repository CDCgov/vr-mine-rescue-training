using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonShowSaveLoadWindow : UIButtonBase
{
    public enum SaveLoadMode
    {
        Unknown,
        Save,
        SaveAs,
        Load
    }

    public GameObject WindowInstance;
    public SaveLoadMode Mode = SaveLoadMode.Unknown;

    private UISaveLoadController _controller;

    protected override void Start()
    {
        base.Start();

        if (WindowInstance == null)
            return;

        _controller = WindowInstance.GetComponent<UISaveLoadController>();
    }

    protected override void OnButtonClicked()
    {
        if (WindowInstance == null || _controller == null)
            return;

        switch (Mode)
        {
            case SaveLoadMode.Save:
                SaveActiveScenario();
                break;
            case SaveLoadMode.SaveAs:
                ShowSaveWindow();
                break;
            case SaveLoadMode.Load:
                ShowLoadWindow();
                break;
        }
        
    }

    private void SaveActiveScenario()
    {
        var workingScenario = ScenarioSaveLoad.Instance.WorkingScenarioName;
        if (workingScenario == null || workingScenario.Length <= 0)
        {
            ShowSaveWindow();
            return;
        }

        Debug.Log($"Saving scenario {workingScenario} with overwrite enabled");
        _ = ScenarioSaveLoad.Instance.SaveCurrentScenario(workingScenario);
    }

    private void ShowSaveWindow()
    {     
        _controller.SetSaveMode();        
        WindowInstance.SetActive(true);
    }

    private void ShowLoadWindow()
    {
        _controller.IsScenarioCloseAllowed((result) =>
        {
            if (result)
            {
                _controller.SetLoadMode();
                WindowInstance.SetActive(true);
            }
        });              
    }
}
