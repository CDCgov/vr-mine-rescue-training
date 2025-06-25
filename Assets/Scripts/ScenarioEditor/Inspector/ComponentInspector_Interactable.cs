using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComponentInspector_Interactable : ComponentInspector<ComponentInfo_Interactable>
{
    //Inspector inspector;
    public TMP_Text headerText;
    //[SerializeField] SliderField massSliderField;
    [SerializeField] ToggleSwitch interactableToggleSwitch;
    [SerializeField] Toggle interactableToggleUI;
    //[SerializeField] ComponentInfo_Interactable TargetComponentInfo;
    [SerializeField] CustomXRInteractable targetInteractableComponent;

    // Start is called before the first frame update
    //public int index;

    public override void Start()
    {
        base.Start();

        //inspector = Inspector.instance;
        //TargetComponentInfo = inspector.targetInfo.componentInfo_Interactables[index];
        targetInteractableComponent = TargetComponentInfo.m_component;
        InitializeValues();
        //massSliderField.onSubmitValue.AddListener(SetRigidbodyVolume);
        interactableToggleSwitch.onToggleComplete.AddListener(SetInteractable);


    }
    private void OnDestroy()
    {
        //massSliderField.onSubmitValue.RemoveListener(SetRigidbodyVolume);
        interactableToggleSwitch.onToggleComplete.RemoveListener(SetInteractable);
    }
    public void InitializeValues()
    {
        //if (targetInteractableComponent != null) targetInteractableComponent.mass = targetInteractableInfo.mass;
        //massSliderField.startValue = targetInteractableInfo.mass;
        //massSliderField.ResetValues();

        //if (targetInteractableComponent != null) 
        //{ 
        //    targetInteractableComponent.enabled = targetInteractableInfo.IsInteractable; 
        //}
        interactableToggleSwitch.OnStateText = "Interactable";
        interactableToggleSwitch.OffStateText = "Static";
        //interactableToggleUI.SetIsOnWithoutNotify(targetInteractableInfo.IsInteractable);
        interactableToggleSwitch.Toggle(TargetComponentInfo.IsInteractable);
    }
    //public void SetRigidbodyVolume(float value, bool enabled)
    //{
    //    targetInteractableInfo.mass = value;
    //    if (targetInteractableComponent != null) targetInteractableComponent.mass = value;
    //}

    public void SetInteractable(bool value)
    {
        TargetComponentInfo.IsInteractable = value;
        if (targetInteractableComponent != null)
        {
            targetInteractableComponent.enabled = value;
            //targetInteractableInfo.SetKinematic(value);
        }
    }
}
