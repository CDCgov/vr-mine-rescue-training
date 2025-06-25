using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(UIDialogResult))]
public class UISaveLoadController : UIContextBase
{
    public TMP_Text TitleText;
    //public TMP_Text OKButtonText;

    public GameObject SaveButton;
    public GameObject LoadButton;

    private UIDialogResult _dialogResult;

    private void Awake()
    {
        _dialogResult = GetComponent<UIDialogResult>();
    }

    public void SetSaveMode()
    {
        string scenarioName = null;
        if (ScenarioSaveLoad.Instance != null)
            scenarioName = ScenarioSaveLoad.Instance.WorkingScenarioName;

        if (scenarioName == null || scenarioName.Length <= 0)
        {
            scenarioName = System.DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss_tt");
        }

        SetContextVariables(scenarioName);

        SetLabels("Save Custom Scenario");

        if (SaveButton != null)
            SaveButton.SetActive(true);
        if (LoadButton != null)
            LoadButton.SetActive(false);
    }

    public void SetContextVariables(string scenarioName)
    {
        if (gameObject.TryGetComponent<UIContextData>(out var context))
        {
            context.SetVariable("SCENARIO_FOLDER", ScenarioSaveLoad.Instance.GetScenarioFilePath());
            context.SetVariable("SELECTED_SCENARIO", scenarioName);
        }
    }

    public void SetLoadMode()
    {
        SetLabels("Load Custom Scenario");
        SetContextVariables("");

        if (SaveButton != null)
            SaveButton.SetActive(false);
        if (LoadButton != null)
            LoadButton.SetActive(true);
    }

    public void IsScenarioCloseAllowed(System.Action<bool> callback)
    {
        if (_dialogResult == null)
            _dialogResult = GetComponent<UIDialogResult>();

        if (callback == null || _dialogResult == null)
            return;

        if (!ScenarioSaveLoad.Instance.IsScenarioDirty)
        {
            callback(true);
            return;
        }

        ModalYesNoCancel.ShowDialog("Do you want to save the scenario?", "Save", "Don't Save", DialogResult.Yes, (result) =>
        {
            switch (result)
            {
                case DialogResult.Yes:
                    _dialogResult.ResetDialog();
                    SetSaveMode();
                    _dialogResult.ShowDialog((saveResult) =>
                    {
                        //delay the callback a frame to allow re-use of the save/load window
                        //gameobjects can't be disable and re-enabled in the same frame
                        if (saveResult == DialogResult.Yes)
                            DelayCallback(callback, true);
                        else
                            DelayCallback(callback, false);
                    });
                    break;

                case DialogResult.No:
                    callback(true);
                    break;

                case DialogResult.Cancel:
                    callback(false);
                    break;
            }
        });

    }

    private async void DelayCallback(System.Action<bool> callback, bool result)
    {
        await System.Threading.Tasks.Task.Yield();

        callback(result);
    }

    private void SetLabels(string windowTitle)
    {
        if (TitleText != null)
            TitleText.text = windowTitle;
    }



}
