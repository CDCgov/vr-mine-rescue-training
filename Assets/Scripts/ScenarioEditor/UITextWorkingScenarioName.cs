using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class UITextWorkingScenarioName : MonoBehaviour
{
    private TMP_Text _text;
    public string FormatString = "Current Scenario: {0}";

    private void Start()
    {
        _text = GetComponent<TMP_Text>();

        if (ScenarioSaveLoad.Instance == null)
        {
            Debug.LogError($"UITextWorkingScenarioName: ScenarioSaveLoad has no instance");
            return;
        }

        ScenarioSaveLoad.Instance.WorkingScenarioNameChanged += OnWorkingScenarioNameChanged;

        UpdateText();
    }

    private void OnWorkingScenarioNameChanged(string obj)
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (ScenarioSaveLoad.Instance == null || _text == null)
            return;

        var scenarioName = ScenarioSaveLoad.Instance.WorkingScenarioName;
        if (scenarioName == null || scenarioName.Length <= 0)
            scenarioName = "Unknown";

        _text.text = string.Format(FormatString, scenarioName);
    }
}
