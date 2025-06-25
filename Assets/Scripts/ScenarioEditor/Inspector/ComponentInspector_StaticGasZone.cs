using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ComponentInspector_StaticGasZone : ComponentInspector<ComponentInfo_StaticGasZone>
{


    //Inspector inspector;
    //public int Index;
    public TMP_Text HeaderText;
    public TMP_InputField SizeXField;
    public TMP_InputField SizeYField;
    public TMP_InputField SizeZField;

    [SerializeField] TMP_InputField _falloffField;

    InspectorField _falloff_IF;
    InspectorField _sizeX_IF;
    InspectorField _sizeY_IF;
    InspectorField _sizeZ_IF;
   
    //public ComponentInfo_StaticGasZone TargetComponentInfo;
    //StaticVentilationZoneStratified _component;
    [SerializeField] StaticMineAtmosphereContainer presetContainer;
    GameObject falloffRender;
    //GameObject coreRender;
    bool customValues;

    public override void Start()
    {
        base.Start();

        //inspector = Inspector.instance;
        //TargetComponentInfo = inspector.targetInfo.componentInfo_StaticGasZone;
        //_component = _staticGasZoneInfo.Component;
        falloffRender = TargetComponentInfo.FalloffObject;
        //coreRender = _staticGasZoneInfo.CoreRender;

        if (TargetComponentInfo != null)
            TargetComponentInfo.SizeChanged += OnGasZoneSizeChanged;

        if (_falloffField != null)
            _falloffField.onEndEdit.AddListener(SetFalloff);
        
        if (SizeXField != null)
            SizeXField.onEndEdit.AddListener(OnSizeChanged);
        if (SizeYField != null)
            SizeYField.onEndEdit.AddListener(OnSizeChanged);
        if (SizeZField != null)
            SizeZField.onEndEdit.AddListener(OnSizeChanged);

        UpdateValuesFromSource();

        //_staticGasZoneInfo.ZoneBounds.extents = _staticGasZoneInfo.CoreRender.transform.localScale / 2;
    }


    public void OnDestroy()
    {
        if (TargetComponentInfo != null)
            TargetComponentInfo.SizeChanged -= OnGasZoneSizeChanged;

        if (_falloffField != null)
            _falloffField.onEndEdit.RemoveListener(SetFalloff);

        if (SizeXField != null)
            SizeXField.onEndEdit.RemoveListener(OnSizeChanged);
        if (SizeYField != null)
            SizeYField.onEndEdit.RemoveListener(OnSizeChanged);
        if (SizeZField != null)
            SizeZField.onEndEdit.RemoveListener(OnSizeChanged);
    }

    private Vector3 ParseSizeVector()
    {
        if (SizeXField == null || SizeYField == null || SizeZField == null)
            return Vector3.one;

        Vector3 size = Vector3.one;
        float.TryParse(SizeXField.text, out size.x);
        float.TryParse(SizeYField.text, out size.y);
        float.TryParse(SizeZField.text, out size.z);

        return size;
    }

    private void OnGasZoneSizeChanged()
    {
        UpdateValuesFromSource();
    }

    private void OnSizeChanged(string arg0)
    {
        if (TargetComponentInfo == null)
            return;

        var size = ParseSizeVector();

        TargetComponentInfo.Size = size;
    }    

    public void UpdateValuesFromSource()
    {
        if (TargetComponentInfo == null)
            return;

        //if (_falloffField != null)
        //    _falloff_IF = _falloffField.GetComponent<InspectorField>();
        //if (_falloff_IF != null)
        //    _falloff_IF.ChangeFromField(_staticGasZoneInfo.Falloff.ToString());

        //if (SizeXField != null)
        //    _sizeX_IF = SizeXField.GetComponent<InspectorField>();

        //_falloffField.text = _staticGasZoneInfo.Falloff.ToString();        

        UpdateInputFieldValue(_falloffField, TargetComponentInfo.Falloff.ToString("F2"));
        UpdateInputFieldValue(SizeXField, TargetComponentInfo.Size.x.ToString("F3"));
        UpdateInputFieldValue(SizeYField, TargetComponentInfo.Size.y.ToString("F3"));
        UpdateInputFieldValue(SizeZField, TargetComponentInfo.Size.z.ToString("F3"));
    }

    private void UpdateInputFieldValue(TMP_InputField inputField, string initValue)
    {
        if (inputField == null)
            return;

        var inspector = inputField.GetComponent<InspectorField>();
        if (inspector != null)
        {

            inspector.ChangeFromField(initValue);
        }
        else
        {
            inputField.text = initValue;
        }
    }

    //public void LoadPreset(int value)
    //{
    //    //_component.StaticMineAtmosphere = presetContainer.staticMineAtmospheres[value];
    //    InitializeValues();
    //}

    public void SetFalloff(string value)
    {
        if (TargetComponentInfo == null)
            return;

        float falloff = float.Parse(value);
        TargetComponentInfo.Falloff = falloff;
    }


}
