using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonSaveScenario : UIButtonBase
{
    public bool AllowOverwrite = false;
    public Transform OverwriteConfirmWindow;

    protected override void OnButtonClicked()
    {
        if (_context == null)
        {
            Debug.LogError($"UIButtonSaveScenario: No UI context found");
            return;
        }

        var selectedScenario = _context.GetStringVariable("SELECTED_SCENARIO");
        if (selectedScenario == null)
        {
            Debug.LogError("UIButtonSaveScenario: No scenario filename selected");
            return;
        }
        
        var scenarioFolder = _context.GetStringVariable("SCENARIO_FOLDER");
        if (scenarioFolder != null && scenarioFolder.Length > 0)
            ScenarioSaveLoad.Instance.ChangeSaveDestination(scenarioFolder);

        var filename = CustomScenarioUtil.GetScenarioFilename(selectedScenario);

        if (!AllowOverwrite && File.Exists(filename))
        {
            if (OverwriteConfirmWindow != null)
                OverwriteConfirmWindow.gameObject.SetActive(true);
        }
        else
        {


            if (_context != null)
            {
                if (_context.TryGetComponent<UIDialogResult>(out var dlgResult))
                {
                    dlgResult.SetDialogResult(DialogResult.Yes);
                }
                _context.gameObject.SetActive(false);
            }

            Debug.Log($"Saving to {selectedScenario}");
            _ = ScenarioSaveLoad.Instance.SaveCurrentScenario(selectedScenario);
        }

        
    }
}
