using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class ComponentInfo_Interactable : ModularComponentInfo, ISaveableComponent
{
    [Tooltip("The name of the component as it appears in the inspector, also used to reference this information so the name should be unique for the prefab. This should be assigned in both the editor and scenario prefab")]
    public string componentName = "Interactable";
    //protected ObjectInfo objectInfo;

    [Tooltip("The assigned component that lives in the prefab. The reference to the component does not save and must be assigned in the scenario prefab. It should be assigned in the editor prefab if the available, but is not required ")]
    public CustomXRInteractable m_component;
    public Inspector.ExposureLevel interactableExposureLevel;
    public bool IsInteractable = true;

    private void Awake()
    {
        //objectInfo = GetComponent<ObjectInfo>();
        //if (objectInfo == null) objectInfo = GetComponentInParent<ObjectInfo>();
        //if (objectInfo != null)
        //{
        //    if (!objectInfo.componentInfo_Interactables.Contains(this)) objectInfo.componentInfo_Interactables.Add(this);
        //}

        if(m_component == null)
        {
            m_component = GetComponentInParent<CustomXRInteractable>();
        }
    }
    public string[] SaveInfo()
    {
        return new string[] { "IsInteractable|" + IsInteractable };
    }
    public string SaveName()
    {
        return componentName;
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
        {
            Debug.Log("Failed to load interactable component info. Saved component is null for " + gameObject.name); return;
        }
        componentName = component.GetComponentName();
        //float.TryParse(component.GetParamValueAsStringByName("Mass"), out mass);
        bool.TryParse(component.GetParamValueAsStringByName("IsInteractable"), out IsInteractable);
        SetInteractable();
        
    }

    public void SetInteractable()
    {
        int count = UnityEngine.SceneManagement.SceneManager.sceneCount;
        bool isEditor = false;
        for (int i = 0; i < count; i++)
        {
            string name = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name;
            if (name == "BAH_ScenarioEditor")
            {
                isEditor = true;
                break;
            }
        }

        if (isEditor)
        {
            Debug.Log($"Setting interactable: {IsInteractable}");
            if (m_component == null)
            {
                m_component = GetComponentInParent<CustomXRInteractable>();
            }

            if (m_component != null)
                m_component.enabled = IsInteractable;
        }
        else
        {
            if (m_component == null)
            {
                m_component = GetComponentInParent<CustomXRInteractable>();
            }

            if (!IsInteractable && m_component != null)
            {
                Destroy(m_component);
            }
        }
    }
}
