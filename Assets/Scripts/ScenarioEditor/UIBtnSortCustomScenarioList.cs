using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBtnSortCustomScenarioList : UIButtonBase
{

    public UIListCustomScenarios CustomScenarioList; 
    public UIListCustomScenarios.CustomScenarioSortType SortType;


    protected override void OnButtonClicked()
    {
        if (CustomScenarioList == null)
            return;

        bool reversed = false;
        if (CustomScenarioList.LastSortType == SortType)
            reversed = !CustomScenarioList.WasLastSortReversed;

        CustomScenarioList.Sort(SortType, reversed);
    }
}
