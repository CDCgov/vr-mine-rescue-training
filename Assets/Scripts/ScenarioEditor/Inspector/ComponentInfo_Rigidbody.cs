using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ComponentInfo_Rigidbody : ModularComponentInfo, ISaveableComponent
{
    [Tooltip("The name of the component as it appears in the inspector, also used to reference this information so the name should be unique for the prefab. This should be assigned in both the editor and scenario prefab")]
    public string componentName = "Physics Rigidbody";
    //protected ObjectInfo objectInfo;

    [Tooltip("The assigned component that lives in the prefab. The reference to the component does not save and must be assigned in the scenario prefab. It should be assigned in the editor prefab if the available, but is not required ")]
    public Rigidbody m_component;
    public Inspector.ExposureLevel massExposureLevel;
    public float mass = 1f;
    //public bool physicsOn = true;

    private void Awake()
    {
        //objectInfo = GetComponent<ObjectInfo>();
        //if (objectInfo == null) objectInfo = GetComponentInParent<ObjectInfo>();
        //if (objectInfo != null)
        //{
        //    if (!objectInfo.componentInfo_Rigidbodies.Contains(this)) objectInfo.componentInfo_Rigidbodies.Add(this);
        //}
    }
    public string[] SaveInfo()
    {
        //Debug.Log("Save Volume At: " + mass);
        return new string[] { "Mass|" + mass };
    }
    public string SaveName()
    {
        return componentName;
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
        {
            Debug.Log("Failed to load Rigidbody component info. Saved component is null for " + gameObject.name); return;
        }
        componentName = component.GetComponentName();
        float.TryParse(component.GetParamValueAsStringByName("Mass"), out mass);
        //bool.TryParse(component.GetParamValueAsStringByName("PhysicsOn"), out physicsOn);
        if (m_component) 
            m_component.mass = mass;
        //SetKinematic(physicsOn);
    }


    public void SetKinematic(bool physicsOn)
    {
        if(m_component == null)
        {
            m_component = GetComponentInParent<Rigidbody>();
        }
        if(m_component != null)
        {
            m_component.useGravity = physicsOn;
            m_component.isKinematic = !physicsOn;

            CustomXRInteractable xr = m_component.GetComponentInChildren<CustomXRInteractable>();
            if(xr != null)
            {
                xr.ThrowOnDetach = physicsOn;
            }

            if (!physicsOn)
            {
                m_component.gameObject.AddComponent<NetSyncServerKinematicConstraint>();
            }
            else
            {
                NetSyncServerKinematicConstraint constraint = m_component.gameObject.GetComponent<NetSyncServerKinematicConstraint>();
                if(constraint != null)
                {
                    Destroy(constraint);
                }
            }
            Debug.Log($"Setting physics: Kinematic-{m_component.isKinematic}, Gravity: {m_component.useGravity}, Obj Name: {m_component.name}");

        }
    }
}
