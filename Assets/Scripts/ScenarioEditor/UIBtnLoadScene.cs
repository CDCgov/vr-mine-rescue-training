using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBtnLoadScene : UIButtonBase
{
    public SceneLoader SceneLoader;
    public UISaveLoadController SaveLoadController;
    public string SceneName;

    protected override void OnButtonClicked()
    {
        if (SaveLoadController != null)
        {
            SaveLoadController.IsScenarioCloseAllowed((result) =>
            {
                if (result)
                    LoadScenario();
            });
        }
        else
        {
            LoadScenario();
        }
    }

    private void LoadScenario()
    {
        if (SceneLoader == null || SceneName == null || SceneName.Length <= 0)
            return;

        SceneLoader.LoadScene(SceneName);
    }
}
