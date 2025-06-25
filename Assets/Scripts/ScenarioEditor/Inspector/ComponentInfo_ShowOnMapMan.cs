using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ComponentInfo_ShowOnMapMan : ModularComponentInfo, ISaveableComponent
{
    [Tooltip("The name of the component as it appears in the inspector, also used to reference this information so the name should be unique for the prefab. This should be assigned in both the editor and scenario prefab")]
    public string componentName = "Map Man Symbol Display";
    public MineMapSymbolRenderer SymbolRenderer;
    public bool ShowOnMapBoard = false;
    public void LoadInfo(SavedComponent component)
    {
        ShowOnMapBoard = component.GetParamValueBool("ShowOnMap");
        SymbolRenderer.ShowOnMapMan = ShowOnMapBoard;
    }

    public string[] SaveInfo()
    {
        return new string[] { "ShowOnMap|" + (ShowOnMapBoard) };
    }

    public string SaveName()
    {
        return componentName;
    }
}
