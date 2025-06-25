using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ComponentInspector_ShowOnMapMan : ComponentInspector<ComponentInfo_ShowOnMapMan>
{
    public TMP_Text headerText;
    [SerializeField] ToggleSwitch _mapmanToggle;
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        _mapmanToggle.onToggleComplete.AddListener(SetOnMapman);
        InitializeValues(TargetComponentInfo);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        _mapmanToggle.onToggleComplete.RemoveListener(SetOnMapman);
    }

    public void InitializeValues(ComponentInfo_ShowOnMapMan componentInfo)
    {
        _mapmanToggle.ToggleWithoutNotify(componentInfo.ShowOnMapBoard);
    }

    public void SetOnMapman(bool value)
    {
        TargetComponentInfo.ShowOnMapBoard = value;
    }
    
}
