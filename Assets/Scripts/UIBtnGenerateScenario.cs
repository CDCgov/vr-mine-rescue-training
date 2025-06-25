using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBtnGenerateScenario : UIButtonBase
{
    public SceneLoader SceneLoader;
    public UISaveLoadController SaveLoadController;

    protected override void OnButtonClicked()
    {
        if (SaveLoadController != null)
        {
            SaveLoadController.IsScenarioCloseAllowed((result) =>
            {
                if (result)
                    GenerateScenario();
            });
        }
        else
        {
            GenerateScenario();
        }
    }

    private void GenerateScenario()
    {
        if (SceneLoader == null)
            return;

        ScenarioInitializer.Instance.GenerateMineOnStart = true;
        SceneLoader.LoadScene("BAH_ScenarioEditor");
    }
}
