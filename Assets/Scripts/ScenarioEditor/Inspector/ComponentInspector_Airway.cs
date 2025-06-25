using NIOSH_MineCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

public class ComponentInspector_Airway : ComponentInspector<ComponentInfo_Airway>
{
    //Inspector inspector;
    //public ComponentInfo_Airway TargetComponentInfo;

    //public int index;
    public TMP_Text headerText;
    [SerializeField] TMP_InputField _addedResistanceInputField;
    [SerializeField] TMP_InputField _overrideResistanceInputField;
    [SerializeField] TMP_InputField _ch4RateInputField;
    [SerializeField] TMP_InputField _frictionInputField;
    
    [SerializeField] private VentilationConnectionAirway _targetAirwayComponent;

    public override void Start()
    {
        base.Start();
        //inspector = Inspector.instance;
        //_targetAirwayInfo = inspector.targetInfo.componentInfo_Airway[index];

        if (TargetComponentInfo == null)
        {
            Debug.LogError($"No TargetComponentInfo set on {gameObject.name}");
            return;
        }

        _targetAirwayComponent = TargetComponentInfo.AirwayComponent;
        InitializeValues();
        
        _addedResistanceInputField.onValueChanged.AddListener(SetAddedResistance);
        _overrideResistanceInputField.onValueChanged.AddListener(SetOverrideResistance);
        _ch4RateInputField.onValueChanged.AddListener(SetCH4Rate);
        _frictionInputField.onValueChanged.AddListener(SetFricitonFactor);
    }



    public void OnDestroy()
    {
        _addedResistanceInputField.onValueChanged.RemoveListener(SetAddedResistance);
        _overrideResistanceInputField.onValueChanged.RemoveListener(SetOverrideResistance);
        _ch4RateInputField.onValueChanged.RemoveListener(SetCH4Rate);
        _frictionInputField.onValueChanged.RemoveListener(SetFricitonFactor);
    }

    public void InitializeValues()
    {
        if (_targetAirwayComponent != null)
        {
            if(_targetAirwayComponent.GetAirway() != null)
            {
                _addedResistanceInputField.text = _targetAirwayComponent.GetAirway().AddedResistance.ToString();
                _overrideResistanceInputField.text = _targetAirwayComponent.GetAirway().OverrideResistance.ToString();
                _ch4RateInputField.text = _targetAirwayComponent.GetAirway().MFAirway.CH4EmissionRateAirway.ToString();
                _frictionInputField.text = _targetAirwayComponent.GetAirway().MFAirway.FrictionFactor.ToString();
            }
        }

    }

    public void SetAddedResistance(string value)
    {
        float inValue = float.Parse(value);
        
        if (inValue < 0)
        {
            _addedResistanceInputField.text = "0";
            return;
        }
        
        TargetComponentInfo.AddedResistance = inValue;
        if (_targetAirwayComponent != null) _targetAirwayComponent.GetAirway().AddedResistance = inValue;
    }
    
    public void SetOverrideResistance(string value)
    {
        float inValue = float.Parse(value);

        TargetComponentInfo.OverrideResistance = inValue;
        if (_targetAirwayComponent != null) _targetAirwayComponent.GetAirway().OverrideResistance = inValue;
    }

    public void SetCH4Rate(string value)
    {
        float inValue = float.Parse(value);
        
        if (inValue < 0)
        {
            _ch4RateInputField.text = "0";
            return;
        }
        
        TargetComponentInfo.CH4EmissionRate = inValue;
        if (_targetAirwayComponent != null) _targetAirwayComponent.GetAirway().MFAirway.CH4EmissionRateAirway = inValue;
    }
    
    public void SetFricitonFactor(string value)
    {
        float inValue = float.Parse(value);
        
        if (inValue < 0)
        {
            _frictionInputField.text = "0";
            return;
        }
        
        TargetComponentInfo.FrictionFactor = inValue;
        if (_targetAirwayComponent != null) _targetAirwayComponent.GetAirway().MFAirway.FrictionFactor = inValue;
    }
}


