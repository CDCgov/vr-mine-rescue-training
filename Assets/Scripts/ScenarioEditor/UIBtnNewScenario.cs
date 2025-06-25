using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBtnNewScenario : UIButtonBase
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
                    NewScenario();
            });
        }
        else
        {
            NewScenario();
        }
    }

    private void NewScenario()
    {
        if (SceneLoader == null)
            return;

        ScenarioInitializer.Instance.GenerateMineOnStart = false;
        SceneLoader.LoadScene("BAH_ScenarioEditor");
    }
}
