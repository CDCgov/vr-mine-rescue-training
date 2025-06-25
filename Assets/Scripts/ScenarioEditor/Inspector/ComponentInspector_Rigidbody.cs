using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ComponentInspector_Rigidbody : ComponentInspector<ComponentInfo_Rigidbody>
{
    //Inspector inspector;
    public TMP_Text headerText;
    [SerializeField] SliderField massSliderField;
    //[SerializeField] ToggleSwitch physicsToggle;
    //[SerializeField] ComponentInfo_Rigidbody TargetComponentInfo;
    [SerializeField] Rigidbody targetRigidbodyComponent;

    // Start is called before the first frame update
    //public int index;

    public override void Start()
    {
        base.Start();

        //inspector = Inspector.instance;
        //targetRigidbodyInfo = inspector.targetInfo.componentInfo_Rigidbodies[index];
        targetRigidbodyComponent = TargetComponentInfo.m_component;
        InitializeValues();
        massSliderField.onSubmitValue.AddListener(SetRigidbodyVolume);
        //physicsToggle.onToggleComplete.AddListener(SetRigidbodyOn);


    }
    private void OnDestroy()
    {
        massSliderField.onSubmitValue.RemoveListener(SetRigidbodyVolume);
        //physicsToggle.onToggleComplete.RemoveListener(SetRigidbodyOn);
    }
    public void InitializeValues()
    {
        if (targetRigidbodyComponent != null) targetRigidbodyComponent.mass = TargetComponentInfo.mass; 
        massSliderField.startValue = TargetComponentInfo.mass;
        massSliderField.ResetValues();
        if(TargetComponentInfo.componentName != null)
        {
            headerText.text = TargetComponentInfo.componentName;
        }
        //if (targetRigidbodyComponent != null) targetRigidbodyComponent.isKinematic = TargetComponentInfo.physicsOn;
        //physicsToggle.Toggle(TargetComponentInfo.physicsOn);
    }
    public void SetRigidbodyVolume(float value, bool enabled)
    {
        TargetComponentInfo.mass = value;
        if (targetRigidbodyComponent != null) targetRigidbodyComponent.mass = value;
    }

    //public void SetRigidbodyOn(bool value)
    //{
    //    TargetComponentInfo.physicsOn = value;
    //    if (targetRigidbodyComponent != null) 
    //    { 
    //        targetRigidbodyComponent.isKinematic = value;
    //        TargetComponentInfo.SetKinematic(value);
    //    }
    //}
}
