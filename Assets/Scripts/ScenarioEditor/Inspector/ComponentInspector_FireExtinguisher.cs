using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Obsolete]
public class ComponentInspector_FireExtinguisher : ComponentInspector<ComponentInfo_FireExtinguisher>
{
    //Inspector inspector;
    public TMP_Text headerText;
    [SerializeField] SliderField chargeSliderField;
    //[SerializeField] ComponentInfo_FireExtinguisher TargetComponentInfo;
    [SerializeField] FireExtinguisherInteraction targetFireExtinguisherComponent;

    // Start is called before the first frame update
    //public int index;

    public override void Start()
    {
        base.Start();

        //inspector = Inspector.instance;
        //TargetComponentInfo = inspector.targetInfo.componentInfo_FireExtinguishers[index];
        //targetFireExtinguisherComponent = TargetComponentInfo.m_component;
        //InitializeValues();
        //chargeSliderField.onSubmitValue.AddListener(SetAudioSourcecharge);
    }

    private void OnDestroy()
    {
        //chargeSliderField.onSubmitValue.RemoveListener(SetAudioSourcecharge);

    }

    public void InitializeValues()
    {
        //if(targetFireExtinguisherComponent != null) targetFireExtinguisherComponent.PercentFull = TargetComponentInfo.charge*100;
        //chargeSliderField.startValue = TargetComponentInfo.charge*100;
        //chargeSliderField.ResetValues();
    }

    public void SetAudioSourcecharge(float value, bool enabled)
    {
        //TargetComponentInfo.charge = value;
        //if (targetFireExtinguisherComponent != null) targetFireExtinguisherComponent.PercentFull = value/100;
    }

}
