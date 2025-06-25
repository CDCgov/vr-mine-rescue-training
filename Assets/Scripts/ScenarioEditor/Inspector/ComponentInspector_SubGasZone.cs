using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ComponentInspector_SubGasZone : MonoBehaviour
{
    enum SubGasPreset
    {
        Normal,
        COZone,
        Methane,
        Explosive,
        Fire,
        Custom = 100,
    }

    enum PrimaryColors
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Purple,
    }

    //Inspector _inspector;
    public int Index;
    public GameObject SubGasZoneInspectorPrefab;

    [SerializeField] TMP_Dropdown _presetDropdown;
    [SerializeField] TMP_Text _presetLabel;
    //public TMP_Dropdown ColorDropdown;
    [SerializeField] TMP_Text _colorLabel;
    [SerializeField] TMP_InputField _COField;
    InspectorField _CO_IF;
    [SerializeField] TMP_InputField _O2Field;
    InspectorField _O2_IF;
    [SerializeField] TMP_InputField _H2SField;
    InspectorField _H2S_IF;
    [SerializeField] TMP_InputField _CH4Field;
    InspectorField _CH4_IF;
    [SerializeField] TMP_InputField _heightLimitField;

    public ToggleSwitch EnableCO;
    public ToggleSwitch EnableO2;
    public ToggleSwitch EnableH2S;
    public ToggleSwitch EnableCH4;

    SubGasZoneData _subGasZoneData;


    
    [SerializeField] StaticMineAtmosphereContainer _presetContainer;
    [SerializeField] Button _addButton;
    [SerializeField] Button _removeButton;
    bool _customValues;
    //[SerializeField] float _upperBoundHeight;


    [SerializeField] GameObject _buttonParent;
    [SerializeField] public GameObject LastButtonParent;

    private ComponentInfo_StaticGasZone _staticGasZoneInfo;

    //Renderer SubZoneRenderer;

    //public ComponentInfo_StaticGasZone GasZoneInfo;
    //StaticMineAtmosphere instance;
    public void OnDestroy()
    {
        //_COField.onEndEdit.RemoveListener(SetCO);
        //_O2Field.onEndEdit.RemoveListener(SetO2);
        //_H2SField.onEndEdit.RemoveListener(SetH2S);
        //_CH4Field.onEndEdit.RemoveListener(SetCH4);
        _CO_IF.onSubmitValue.RemoveListener(SetCO);
        _O2_IF.onSubmitValue.RemoveListener(SetO2);
        _H2S_IF.onSubmitValue.RemoveListener(SetH2S);
        _CH4_IF.onSubmitValue.RemoveListener(SetCH4);
        _heightLimitField.onEndEdit.RemoveListener(UpperBoundHeight);

        EnableCO.onToggleComplete.RemoveListener(OnEnableCO);
        EnableO2.onToggleComplete.RemoveListener(OnEnableO2);
        EnableH2S.onToggleComplete.RemoveListener(OnEnableH2S);
        EnableCH4.onToggleComplete.RemoveListener(OnEnableCH4);

        //_presetDropdown.onValueChanged.RemoveListener(LoadPreset);
        //if(_subGasZoneData != null)ColorDropdown.onValueChanged.RemoveListener(_subGasZoneData.ChangeCoreColor);

        _addButton.onClick.RemoveListener(AddSubGasZone);
        _removeButton.onClick.RemoveListener(RemoveSubGasZone);

        if (_subGasZoneData != null)
            _subGasZoneData.GasValuesChanged -= OnGasValuesChanged;

    }
    private void Awake()
    {

        _CO_IF = _COField.GetComponent<InspectorField>();
        _O2_IF = _O2Field.GetComponent<InspectorField>();
        _H2S_IF = _H2SField.GetComponent<InspectorField>();
        _CH4_IF = _CH4Field.GetComponent<InspectorField>();

        //_COField.onEndEdit.AddListener(SetCO);
        //_O2Field.onEndEdit.AddListener(SetO2);
        //_H2SField.onEndEdit.AddListener(SetH2S);
        //_CH4Field.onEndEdit.AddListener(SetCH4);
        _CO_IF.onSubmitValue.AddListener(SetCO);
        _O2_IF.onSubmitValue.AddListener(SetO2);
        _H2S_IF.onSubmitValue.AddListener(SetH2S);
        _CH4_IF.onSubmitValue.AddListener(SetCH4);
        _heightLimitField.onEndEdit.AddListener(UpperBoundHeight);

        EnableCO.onToggleComplete.AddListener(OnEnableCO);
        EnableO2.onToggleComplete.AddListener(OnEnableO2);
        EnableH2S.onToggleComplete.AddListener(OnEnableH2S);
        EnableCH4.onToggleComplete.AddListener(OnEnableCH4);

        _addButton.onClick.AddListener(AddSubGasZone);
        _removeButton.onClick.AddListener(RemoveSubGasZone);
    }

    private void OnEnableCO(bool enabled)
    {
        _subGasZoneData.MineAtmosphere.CarbonMonoxideStrength = enabled ? 1.0f : 0.0f;
    }

    private void OnEnableO2(bool enabled)
    {
        _subGasZoneData.MineAtmosphere.OxygenStrength = enabled ? 1.0f : 0.0f;
    }

    private void OnEnableH2S(bool enabled)
    {
        _subGasZoneData.MineAtmosphere.HydrogenSulfideStrength = enabled ? 1.0f : 0.0f;
    }

    private void OnEnableCH4(bool enabled)
    {
        _subGasZoneData.MineAtmosphere.MethaneStrength = enabled ? 1.0f : 0.0f;
    }

    public void Initialize(ComponentInfo_StaticGasZone staticGasZone, SubGasZoneData info)
    {
        if (_subGasZoneData != null)
            _subGasZoneData.GasValuesChanged -= OnGasValuesChanged;

        //Debug.Log("ZoneInspectorInitialize");
        //_inspector = Inspector.instance;
        _subGasZoneData = info;
        _staticGasZoneInfo = staticGasZone;
        //GasZoneInfo = _info.GasZoneInfo;
        
        _subGasZoneData.GasValuesChanged += OnGasValuesChanged;

        //ColorDropdown.onValueChanged.AddListener(_info.ChangeCoreColor);

        var subInspectors = GetComponentsInChildren<ISelectedSubGasZoneView>();
        foreach (var inspector in subInspectors)
            inspector.SetSubGasZone(staticGasZone, info);


        UpdateValuesFromSource();
        //if (Index == 0) { _removeButton.gameObject.SetActive(false); _heightLimitField.gameObject.SetActive(false); }

        bool enableControls = Index != 0;
        _removeButton.gameObject.SetActive(enableControls); 
        _heightLimitField.gameObject.SetActive(enableControls);
    }

    private void OnGasValuesChanged()
    {
        UpdateValuesFromSource();
    }

    public void UpdateValuesFromSource()
    {
        _CO_IF.ChangeFromField(_subGasZoneData.MineAtmosphere.CarbonMonoxide * 1000000.0f);
        _O2_IF.ChangeFromField(_subGasZoneData.MineAtmosphere.Oxygen * 100.0f);
        _H2S_IF.ChangeFromField(_subGasZoneData.MineAtmosphere.HydrogenSulfide * 1000000.0f);
        _CH4_IF.ChangeFromField(_subGasZoneData.MineAtmosphere.Methane * 100.0f);

        _heightLimitField.onEndEdit.Invoke(_subGasZoneData.UpperBoundHeight.ToString());

        EnableCO.ToggleInstantly(_subGasZoneData.MineAtmosphere.CarbonMonoxideStrength > 0 ? true : false);
        EnableO2.ToggleInstantly(_subGasZoneData.MineAtmosphere.OxygenStrength > 0 ? true : false);
        EnableH2S.ToggleInstantly(_subGasZoneData.MineAtmosphere.HydrogenSulfideStrength > 0 ? true : false);
        EnableCH4.ToggleInstantly(_subGasZoneData.MineAtmosphere.MethaneStrength > 0 ? true : false);
    }

    //public void LoadPreset(int value)
    //{
    //    //_info.AtmosphereScriptableInstance = _presetContainer.staticMineAtmospheres[value];

    //    //_COField.text = _info.AtmosphereScriptableInstance.MineAtmosphere.CarbonMonoxide.ToString();
    //    _COField.onEndEdit.Invoke(_subGasZoneData.MineAtmosphere.CarbonMonoxide.ToString());
    //   //_O2Field.text = _info.AtmosphereScriptableInstance.MineAtmosphere.Oxygen.ToString();
    //    _O2Field.onEndEdit.Invoke(_subGasZoneData.MineAtmosphere.Oxygen.ToString());
    //   // _H2SField.text = _info.AtmosphereScriptableInstance.MineAtmosphere.HydrogenSulfide.ToString();
    //    _H2SField.onEndEdit.Invoke(_subGasZoneData.MineAtmosphere.HydrogenSulfide.ToString());
    //    //_CH4Field.text = _info.AtmosphereScriptableInstance.MineAtmosphere.Methane.ToString();
    //    _CH4Field.onEndEdit.Invoke(_subGasZoneData.MineAtmosphere.Methane.ToString());

    //    //InitializeValues();
    //}

    public void CustomValues()
    {
        
        //if (GasZoneInfo.Component)
        //{
        //    GasZoneInfo.Component.AtmosphereStrata[Index] = _subGasZoneData.StrataStruct;
        //    GasZoneInfo.Component.AtmosphereStrata[Index].StrataAtmosphere.MineAtmosphere.CarbonMonoxide = _subGasZoneData.AtmosphereScriptableInstance.MineAtmosphere.CarbonMonoxide;
        //    GasZoneInfo.Component.AtmosphereStrata[Index].StrataAtmosphere.MineAtmosphere.Oxygen = _subGasZoneData.AtmosphereScriptableInstance.MineAtmosphere.Oxygen;
        //    GasZoneInfo.Component.AtmosphereStrata[Index].StrataAtmosphere.MineAtmosphere.HydrogenSulfide = _subGasZoneData.AtmosphereScriptableInstance.MineAtmosphere.HydrogenSulfide;
        //    GasZoneInfo.Component.AtmosphereStrata[Index].StrataAtmosphere.MineAtmosphere.Methane = _subGasZoneData.AtmosphereScriptableInstance.MineAtmosphere.Methane;
        //}


        _presetLabel.text = "Custom";

        _customValues = true;
    }

    private void SetCO(float value, bool _ = true)
    {
        if (!_customValues) CustomValues();

        if (_customValues)
        {
            //float curValue = float.Parse(value);
            _subGasZoneData.MineAtmosphere.CarbonMonoxide = value / 1000000.0f;
            //if (_component != null) _component.StaticMineAtmosphere.MineAtmosphere.CarbonMonoxide = curValue;
        }
    }
    private void SetO2(float value, bool _ = true)
    {
        if (!_customValues) CustomValues();
        if (_customValues)
        {
            //float curValue = float.Parse(value);
            _subGasZoneData.MineAtmosphere.Oxygen = value / 100.0f;
            //if (_component != null) _component.StaticMineAtmosphere.MineAtmosphere.Oxygen = curValue;
        }
    }
    private void SetH2S(float value, bool _ = true)
    {
        if (!_customValues) CustomValues();
        if (_customValues)
        {
            //float curValue = float.Parse(value);
            _subGasZoneData.MineAtmosphere.HydrogenSulfide = value / 1000000.0f;
            //if (_component != null) _component.StaticMineAtmosphere.MineAtmosphere.HydrogenSulfide = curValue;
        }
    }
    private void SetCH4(float value, bool _ = true)
    {
        if (!_customValues) CustomValues();
        if (_customValues)
        {
            //float curValue = float.Parse(value);
            _subGasZoneData.MineAtmosphere.Methane = value / 100.0f;
            //if (_component != null) _component.StaticMineAtmosphere.MineAtmosphere.Methane = curValue;
        }
    }
    private void UpperBoundHeight(string value)
    {
        float curValue = float.Parse(value);
        if (curValue < 0)
        {
            curValue = 0;
            _heightLimitField.SetTextWithoutNotify("0");
        }
        _subGasZoneData.UpperBoundHeight = curValue;
        //Debug.Log("upper bounds : " + _info.UpperBoundHeight.ToString());
        _heightLimitField.text =  value.ToString();
        //_subGasZoneData.AdjustRendererHeightBounds();
        _staticGasZoneInfo.UpdateZoneHeights();
    }
    public void AddSubGasZone()
    {
        Debug.Log("AddSubGasZone");

        //creates new info
        //SubGasZoneData newInfo = new SubGasZoneData();

        if (_staticGasZoneInfo == null || _staticGasZoneInfo.ZoneRenderPrefab == null)
        {
            Debug.LogError("Gas Zone Info Null");
            return;
        }
        //if (GasZoneInfo.ZoneRenderPrefab == null) Debug.Log("Gas Zone Info Null");

        //newInfo.ZoneRenderInstance = Instantiate(_staticGasZoneInfo.ZoneRenderPrefab, _staticGasZoneInfo.transform);
        //newInfo.Initialize(GasZoneInfo, Index + 1);

        var newInfo = _staticGasZoneInfo.AddNewSubGasZone();

        //add component inspector
        //GameObject obj = Instantiate(_inspector.componentInspectorPrefab_SubGasZone, _inspector.modularItemContainer);
        GameObject obj = Instantiate(SubGasZoneInspectorPrefab, transform.parent);
        ComponentInspector_SubGasZone compInspector = obj.GetComponent<ComponentInspector_SubGasZone>();
        compInspector.Index = Index + 1;
        compInspector.LastButtonParent = _buttonParent;

        compInspector.Initialize(_staticGasZoneInfo, newInfo);
        //compInspector.ColorDropdown.value = Index + 1;
        //compInspector.ColorDropdown.onValueChanged.Invoke(Index + 1);
        

        //hideButton
        _buttonParent.SetActive(false);

        //Rebuild Layout
        //_inspector.SizeContainerContent(true);
    }
    public void RemoveSubGasZone()
    {
        if (_staticGasZoneInfo == null)
            return;

        //Unhide previous nodes
        if (LastButtonParent != null) 
        { 
            LastButtonParent.SetActive(true); 
        }

        //delete component info
        //var info = GasZoneInfo.SubGasZones[Index];
        //GasZoneInfo.SubGasZones.Remove(info);
        //Destroy(info);
        //_subGasZoneData.GasZoneInfo.SubGasZones.Remove(info);

        _staticGasZoneInfo.RemoveSubGasZone(Index);

        Destroy(this.gameObject);
    }
    
}
