using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBtnScenarioEditorLoadScenario : UIButtonBase
{
    protected override void OnButtonClicked()
    {
        if (_context == null)
        {
            Debug.LogError($"UIButtonSaveScenario: No UI context found");
            return;
        }

        var selectedScenario = _context.GetStringVariable("SELECTED_SCENARIO");
        if (selectedScenario == null)
            return;

        var scenarioFolder = _context.GetStringVariable("SCENARIO_FOLDER");
        if (scenarioFolder != null && scenarioFolder.Length > 0)
            ScenarioSaveLoad.Instance.ChangeSaveDestination(scenarioFolder);

        ScenarioSaveLoad.Instance.LoadScenarioFromFile(selectedScenario, true, true);

        if (_context != null)
            _context.gameObject.SetActive(false);
    }
}
