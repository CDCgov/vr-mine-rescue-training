using NIOSH_MineCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

public class ComponentInspector_JunctionProperties : ComponentInspector<ComponentInfo_JunctionProperties>
{
    //Inspector inspector;
    //public int index;
    public TMP_Text headerText;
    [SerializeField] Toggle startJunctionToggleField;
    [SerializeField] Toggle inAtmosphereToggleField;
    [SerializeField] TMP_InputField ch4InputField;
    [SerializeField] TMP_InputField contaminantInputField;
    //[SerializeField] ComponentInfo_JunctionProperties TargetComponentInfo;
    [SerializeField] private VentilationLayerNode targetNodeComponent;

    public override void Start()
    {
        base.Start();

        //inspector = Inspector.instance;
        //TargetComponentInfo = inspector.targetInfo.componentInfo_JunctionProperties[index];
        targetNodeComponent = TargetComponentInfo.nodeComponent;
        InitializeValues();
        
        startJunctionToggleField.onValueChanged.AddListener(SetStartJunction);
        inAtmosphereToggleField.onValueChanged.AddListener(SetInAtmosphere);
        ch4InputField.onValueChanged.AddListener(SetCH4);
        contaminantInputField.onValueChanged.AddListener(SetContaminant);
    }
    public void OnDestroy()
    {
        startJunctionToggleField.onValueChanged.RemoveListener(SetStartJunction);
    }

    public void InitializeValues()
    {
        if (targetNodeComponent != null)
        {
            if(targetNodeComponent.nodeJunction != null)
            {
                startJunctionToggleField.isOn = targetNodeComponent.nodeJunction.IsStartJunction;
                inAtmosphereToggleField.isOn = targetNodeComponent.nodeJunction.IsInAtmosphere;
                ch4InputField.text = targetNodeComponent.nodeJunction.MFJunction.CH4Concentration.ToString();
                contaminantInputField.text = targetNodeComponent.nodeJunction.MFJunction.ContamConcentration.ToString();
            }
        }

    }

    public void SetStartJunction(bool value)
    {
        TargetComponentInfo.isStartJunction = value;
        TargetComponentInfo.isInAtmosphere = value;
        targetNodeComponent.StartJunctionRenderChange(value);
        if (targetNodeComponent != null) targetNodeComponent.SetJunctionIsStart(value);
        if (targetNodeComponent != null) targetNodeComponent.SetJunctionInAtmosphere(value);
    }

    public void SetInAtmosphere(bool value)
    {
        TargetComponentInfo.isInAtmosphere = value;
        if (targetNodeComponent != null) targetNodeComponent.SetJunctionInAtmosphere(value);

    }

    public void SetCH4(string value)
    {
        float inValue = float.Parse(value);
        
        if (inValue < 0)
        {
            ch4InputField.text = "0";
            return;
        }
        
        TargetComponentInfo.ch4Concentration = inValue;
        if (targetNodeComponent != null) targetNodeComponent.nodeJunction.MFJunction.CH4Concentration = inValue;
    }

    public void SetContaminant(string value)
    {
        float inValue = float.Parse(value);
        
        if (inValue < 0)
        {
            contaminantInputField.text = "0";
            return;
        }
        
        TargetComponentInfo.contaminantConcentration = inValue;
        if (targetNodeComponent != null) targetNodeComponent.nodeJunction.MFJunction.ContamConcentration = inValue;
    }

}


