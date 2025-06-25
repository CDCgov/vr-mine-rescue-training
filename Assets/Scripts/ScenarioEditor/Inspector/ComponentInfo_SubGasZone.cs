using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NIOSH_MineCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SubGasZoneData
{

    //[Tooltip("The name of the component as it appears in the inspector, also used to reference this information so the name should be unique for the prefab. This should be assigned in both the editor and scenario prefab")]
    //public string componentName = "Sub Gas Zone";
    //protected ObjectInfo objectInfo;

    //public int Index;
    //public GameObject subRender;

    //public StaticMineAtmosphere AtmosphereScriptableInstance;
    //public StaticVentilationZoneStratified.AtmosphereStratumData StrataStruct;
    //public ComponentInfo_StaticGasZone GasZoneInfo;

    public ComponentInspector_SubGasZone ZoneInspector;
    public GameObject ZoneRenderInstance;
    public Renderer ZoneRenderer;

    public MineAtmosphere MineAtmosphere;

    public float UpperBoundHeight;
    public int Color;

    public event System.Action GasValuesChanged;

    private Color _color;
    //float strataTop = 0;
    //float strataBottom = 0;
    

    //bool initialized = false;
    //public void Initialize(ComponentInfo_StaticGasZone gasInfo, int index)
    //{
    //    if (initialized == true) return;
    //    //Debug.Log("Initialize Sub Gas Zone Info");
    //    GasZoneInfo = gasInfo;

    //    Index = index;

    //    //add myself
    //    if (!GasZoneInfo.SubGasZones.Contains(this)) GasZoneInfo.SubGasZones.Insert(index, this);
    //    if (GasZoneInfo.Component && !GasZoneInfo.Component.AtmosphereStrata.Contains(StrataStruct)) { GasZoneInfo.Component.AtmosphereStrata.Insert(Index, StrataStruct); }

    //    InitializeObjects();
    //    InitializeValues();

    //    //GasZoneInfo.CreateComponentInspector(this);
    //    initialized = true;

    //}

    //public void InitializeFromLoad(ComponentInfo_StaticGasZone gasInfo, int index)
    //{
    //    //Debug.Log("Initialize Sub Gas Zone Info");
    //    GasZoneInfo = gasInfo;

    //    Index = index;

    //    //add myself
    //    if (!GasZoneInfo.SubGasZones.Contains(this)) GasZoneInfo.SubGasZones.Insert(index, this);
    //    if (GasZoneInfo.Component && !GasZoneInfo.Component.AtmosphereStrata.Contains(StrataStruct)) { GasZoneInfo.Component.AtmosphereStrata.Insert(Index, StrataStruct); }

    //    InitializeObjects();
    //    initialized = true;
    //}

    //void InitializeObjects()
    //{
    //    StrataStruct = new StaticVentilationZoneStratified.AtmosphereStratumData();
    //    if (AtmosphereScriptableInstance == null) AtmosphereScriptableInstance = ScriptableObject.CreateInstance<StaticMineAtmosphere>();
    //    if (ZoneRenderInstance == null) GasZoneInfo.CreateRendererInstance(this);
    //    StrataStruct.StrataAtmosphere = AtmosphereScriptableInstance;

    //}

    //public void InitializeValues()
    //{
    //    componentName = "Sub Gas Zone " + Index;
    //    UpperBoundHeight = GasZoneInfo.zoneHeight / (Index + 1);
    //    if(StrataStruct.UpperBoundHeight == 0) StrataStruct.UpperBoundHeight = UpperBoundHeight;
    //    Debug.Log("Value initializing");
    //    AdjustRendererHeightBounds();

    //}

    public void SetRendererInstance(GameObject zoneRenderer)
    {
        ZoneRenderInstance = zoneRenderer;
        ZoneRenderer = zoneRenderer.GetComponent<Renderer>();
    }

    public void SetRenderHeight(float strataBottom, float strataTop, Vector3 size)
    {
        if (ZoneRenderInstance == null)
            return;

        //Center between upper and lower bounds
        var center = (strataBottom + strataTop) / 2;
        ZoneRenderInstance.transform.localPosition = new Vector3(0, center, 0);

        //set scale
        var scale = strataTop - strataBottom;
        ZoneRenderInstance.transform.localScale = new Vector3(size.x, scale, size.z);
    }

    public void RaiseValuesChanged()
    {
        GasValuesChanged?.Invoke();
    }

    public void SetGasValuePreset(MineAtmosphere preset)
    {
        MineAtmosphere = preset;
        //ChangeColor(preset.DefaultColor);
        GasValuesChanged?.Invoke();
    }

    //public void SetRenderScale(Vector3 size)
    //{
    //    if (ZoneRenderInstance == null)
    //        return;

    //    Vector3 scale = size;
    //    scale.y = 1;
    //    ZoneRenderInstance.transform.localScale = scale;
    //}

    //public void AdjustRendererHeightBounds(bool adjustLast = true, bool loading = false)
    //{
    //    if (ZoneRenderInstance == null) GasZoneInfo.CreateRendererInstance(this);
        

    //    //Debug.Log("Adjust Renderer");
    //    strataTop = Mathf.Clamp(StrataStruct.UpperBoundHeight + GasZoneInfo.zoneBottom, GasZoneInfo.zoneBottom, GasZoneInfo.zoneTop);

    //    //bottom is set by next strata top, otherwise is just bottom
    //    float strataBottom = GasZoneInfo.zoneBottom;
    //    if (Index < GasZoneInfo.Component.AtmosphereStrata.Count - 1)
    //    {
    //        var nextStrata = GasZoneInfo.Component.AtmosphereStrata[Index + 1];
    //        strataBottom = nextStrata.UpperBoundHeight + GasZoneInfo.zoneBottom;
    //    }

    //    //Center between upper and lower bounds
    //    var center = (strataBottom + strataTop) / 2;
    //    ZoneRenderInstance.transform.localPosition = new Vector3(0, center, 0);

    //    //set scale
    //    var scale = strataTop - strataBottom;
    //    ZoneRenderInstance.transform.localScale = new Vector3(1, scale, 1);

    //    //assign to component
    //    GasZoneInfo.Component.AtmosphereStrata[Index] = StrataStruct;

    //    //adjust previous struct bottom and assign
    //    if (adjustLast && Index > 0)
    //    {
    //        var lastStrataInfo = GasZoneInfo.SubGasZones[Index - 1];
    //        lastStrataInfo.AdjustRendererHeightBounds(false);
    //    }

    //}
    
    public void SetVisibility(bool state)
    {
        //if(GasZoneInfo  != null) { GasZoneInfo.CreateRendererInstance(this); }
        //if(ZoneRenderer == null && ZoneRenderInstance != null) ZoneRenderer = ZoneRenderInstance.GetComponent<Renderer>();
        //if(ZoneRenderer != null) ZoneRenderer.enabled = state;

        if (ZoneRenderInstance != null)
            ZoneRenderInstance.SetActive(state);

    }

    public void ChangeColor(Color color)
    {
        _color = color;
        if (ZoneRenderInstance == null || ZoneRenderer == null)
            return;

        ZoneRenderer.material.color = color;
    }

    //public void ChangeCoreColor(int i)
    //{
    //    if (ZoneRenderInstance == null) GasZoneInfo.CreateRendererInstance(this);

    //    Renderer subZoneRenderer = ZoneRenderInstance.GetComponent<Renderer>();
    //    if (i > 5) { i = 0; }
    //    Color color = new Color(0, 0, 0, 0);

    //    switch (i)
    //    {
    //        case 0:
    //            color = GasZoneInfo.GreenZone;
    //            if (ZoneInspector) ZoneInspector.ColorDropdown.SetValueWithoutNotify(0);
    //            //_colorLabel.text = "Green";
    //            break;
    //        case 1:
    //            color = GasZoneInfo.YellowZone;
    //            if (ZoneInspector) ZoneInspector.ColorDropdown.SetValueWithoutNotify(1);
    //            //_colorLabel.text = "Yellow";
    //            break;
    //        case 2:
    //            color = GasZoneInfo.OrangeZone;
    //            if (ZoneInspector) ZoneInspector.ColorDropdown.SetValueWithoutNotify(2);
    //            //_colorLabel.text = "Orange";
    //            break;
    //        case 3:
    //            color = GasZoneInfo.RedZone;
    //            if (ZoneInspector) ZoneInspector.ColorDropdown.SetValueWithoutNotify(3);
    //            //_colorLabel.text = "Red";
    //            break;
    //        case 4:
    //            color = GasZoneInfo.PurpleZone;
    //            if (ZoneInspector) ZoneInspector.ColorDropdown.SetValueWithoutNotify(4);
    //            //_colorLabel.text = "Purple";
    //            break;
    //        case 5:
    //            color = GasZoneInfo.BlueZone;
    //            if (ZoneInspector) ZoneInspector.ColorDropdown.SetValueWithoutNotify(5);
    //            //_colorLabel.text = "Blue";
    //            break;
    //    }

    //    if (ZoneRenderInstance != null)
    //    {
    //        // change the color of the sub render
    //        subZoneRenderer.material.color = color;
    //        Color = i;
    //    }
    //}



}
