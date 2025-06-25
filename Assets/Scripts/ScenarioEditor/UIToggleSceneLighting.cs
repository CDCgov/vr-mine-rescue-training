using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIToggleSceneLighting : UIToggleBase
{

    protected override void Start()
    {
        base.Start();

        if (ScenarioSaveLoad.Instance == null)
        {
            Debug.LogError($"UIToggleSceneLighting: Couldn't find ScenarioSaveLoad instance");
            return;
        }

        ScenarioSaveLoad.Instance.SceneLightingChanged += OnSceneLightingChanged;
        OnSceneLightingChanged();
    }

    private void OnSceneLightingChanged()
    {
        _toggle.SetIsOnWithoutNotify(ScenarioSaveLoad.Instance.IsSceneLightingEnabled);
    }

    protected override void OnToggleChanged(bool value)
    {
        if (ScenarioSaveLoad.Instance == null)
            return;

        ScenarioSaveLoad.Instance.EnableSceneLighting(value);
    }
}
