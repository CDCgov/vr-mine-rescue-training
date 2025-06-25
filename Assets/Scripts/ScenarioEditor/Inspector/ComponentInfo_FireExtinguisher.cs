using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentInfo_FireExtinguisher : ModularComponentInfo, ISaveableComponent, IInspectableComponent
{
    [Tooltip("The assigned component that lives in the prefab. The reference to the component does not save and must be assigned in the scenario prefab. It should be assigned in the editor prefab if the available, but is not required ")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_component")]
    public FireExtinguisherInteraction FireExtinguisherInteraction;

    //[Tooltip("The name of the component as it appears in the inspector, also used to reference this information so the name should be unique for the prefab. This should be assigned in both the editor and scenario prefab")]
    //public string componentName = "Fire Extinguisher";
    //protected ObjectInfo objectInfo;
    
    [InspectableNumericProperty("Charge Level", MinValue = 0, MaxValue = 100, SliderControl = true, Units = NumericPropertyUnitType.Ratio)]
    public float ChargeLevel
    {
        get
        {
            return _chargeLevel;
        }
        set
        {
            _chargeLevel = value;
            if (FireExtinguisherInteraction != null)
            {
                FireExtinguisherInteraction.SetPercentFull(value);
            }
        }
    }

    public string ComponentInspectorTitle => "Fire Extinguisher";

    private float _chargeLevel = 1.0f;
    
    //private void Awake()
    //{
    //    objectInfo = GetComponent<ObjectInfo>();
    //    if (objectInfo == null) objectInfo = GetComponentInParent<ObjectInfo>();
    //    if (objectInfo != null)
    //    {
    //        if (!objectInfo.componentInfo_FireExtinguishers.Contains(this)) objectInfo.componentInfo_FireExtinguishers.Add(this);
    //    }
    //}
    public string[] SaveInfo()
    {
       
        return new string[] { "Charge|" + ChargeLevel };
    }
    public string SaveName()
    {
        return "Fire Extinguisher";
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
        {
            Debug.Log("Failed to load fire extinguisher component info. Saved component is null for " + gameObject.name); 
            return;
        }

        ChargeLevel = component.GetParamValueFloat("Charge", 1.0f);



        //componentName = component.GetComponentName();
        //float.TryParse(component.GetParamValueAsStringByName("Charge"), out charge);

        //if (m_component) 
        //{
        //    //m_component.PercentFull = charge / 100; 
        //    m_component.SetPercentFull(charge / 100);
        //}

    }
}
