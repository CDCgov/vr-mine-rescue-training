using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISliderMineMapSymbolScale : UISliderFieldBase
{
    protected override void Start()
    {
        base.Start();

        SetCurrentValue(ScenarioSaveLoad.Settings.MineMapSymbolScale);
    }

    protected override void OnValueChanged(float val)
    {
        ScenarioSaveLoad.Settings.MineMapSymbolScale = val;
    }
}
