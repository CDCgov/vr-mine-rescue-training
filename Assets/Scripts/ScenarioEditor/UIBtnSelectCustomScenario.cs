using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBtnSelectCustomScenario : UIContextBase
{
    public TMPro.TMP_Text ScenarioNameText;
    public TMPro.TMP_Text ScenarioDateText;
    
    public Image BackgroundImage;

    public Color SelectedColor = new Color(0, 0.5f, 0);
    public Color NormalColor = Color.black;

    private Button _button;
    private const string CONTEXT_VARIABLE = "SELECTED_SCENARIO";
    private CustomScenarioData _scenarioData;

    public void SetScenarioData(CustomScenarioData data)
    {

        _scenarioData = data;

        UpdateLabels();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        _button = GetComponent<Button>();

        _button.onClick.AddListener(OnButtonClicked);
        _context.ContextDataChanged += OnContextDataChanged;

        UpdateLabels();

        OnContextDataChanged(null);
    }

    private void UpdateLabels()
    {
        if (ScenarioNameText != null)
            ScenarioNameText.text = _scenarioData.ScenarioName;
        if (ScenarioDateText != null)
            ScenarioDateText.text = _scenarioData.ScenarioDate.ToString("yyyy-MM-dd hh:mm:ss tt");

    }

    private void OnDestroy()
    {
        if (_context != null)
            _context.ContextDataChanged -= OnContextDataChanged;
    }

    private void OnContextDataChanged(string obj)
    {
        if (BackgroundImage == null)
            return;

        var selectedScenario = _context.GetStringVariable(CONTEXT_VARIABLE);

        if (selectedScenario == _scenarioData.ScenarioName)
            BackgroundImage.color = SelectedColor;
        else
            BackgroundImage.color = NormalColor;
    }

    private void OnButtonClicked()
    {
        _context.SetVariable(CONTEXT_VARIABLE, _scenarioData.ScenarioName);
    }
}
