using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ComponentInfo_AudioSource : ModularComponentInfo, ISaveableComponent
{
    [Tooltip("The name of the component as it appears in the inspector, also used to reference this information so the name should be unique for the prefab. This should be assigned in both the editor and scenario prefab")]
    public string componentName = "Audio Source";
    public string AudioSourceDetail;
    //protected ObjectInfo objectInfo;
    [Tooltip("The assigned component that lives in the prefab. The reference to the component does not save and must be assigned in the scenario prefab. It should be assigned in the editor prefab if the available, but is not required ")]
    public AudioSource m_component;
    public Inspector.ExposureLevel volumeExposureLevel;
    public float volume = 1f;
    public bool IsAmbientSound = false;
    //private void Awake()
    //{
    //    objectInfo = GetComponent<ObjectInfo>();
    //    if (objectInfo == null) objectInfo = GetComponentInParent<ObjectInfo>();
    //    if (objectInfo != null)
    //    {
    //        if (!objectInfo.componentInfo_AudioSources.Contains(this)) objectInfo.componentInfo_AudioSources.Add(this);
    //    }
    //}
    public string[] SaveInfo()
    {
        Debug.Log("Save Volume At: " + volume);
        return new string[] { "volume|" + volume };
    }
    public string SaveName()
    {
        return componentName;
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null) 
        {
            Debug.Log("Failed to load audio component info. Saved component is null for " + gameObject.name); return;
        }
        componentName = component.GetComponentName();
        float.TryParse(component.GetParamValueAsStringByName("Volume"), out volume);
        Debug.Log("Load Volume At: " + volume);
        if (m_component) 
            m_component.volume = volume / 100.0f;
    }

}
