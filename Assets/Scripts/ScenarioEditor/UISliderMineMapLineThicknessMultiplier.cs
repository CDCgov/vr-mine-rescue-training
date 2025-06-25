using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISliderMineMapLineThicknessMultiplier : UISliderFieldBase
{
    protected override void Start()
    {
        base.Start();

        SetCurrentValue(ScenarioSaveLoad.Settings.MineMapLineWidthScale);
    }

    protected override void OnValueChanged(float val)
    {
        ScenarioSaveLoad.Settings.MineMapLineWidthScale = val;
    }
}
