using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDropdownScenarioAvatar : UIDropdownBase
{
    public class OptionData : TMP_Dropdown.OptionData
    {
        
    }

    protected override int GetCurrentSelectedIndex()
    {
        return 0;
    }

    protected override IEnumerable<TMP_Dropdown.OptionData> GetDropdownOptions()
    {
        yield return new OptionData { text = "Coal MR (BG4)" };
        yield return new OptionData { text = "Coal Basic" };
    }

    protected override void OnSelectionChanged(TMP_Dropdown.OptionData data)
    {
        
    }
}
