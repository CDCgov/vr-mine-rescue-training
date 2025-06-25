using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ComponentInfo_Name : ISaveableComponent
{
    public string instanceDisplayName;
    public string instanceUserSuppliedName;
    public Inspector.ExposureLevel exposureLevel = Inspector.ExposureLevel.Editable;
    public string componentName = "Name";
    public ObjectInfo objectInfo;
    public string SaveName()
    {
        return componentName;
    }
    public string[] SaveInfo()
    {
        string useName = "";
        if (string.IsNullOrEmpty(instanceUserSuppliedName))
        {
            useName = instanceDisplayName;
        }
        else
        {
            useName = instanceUserSuppliedName;
        }
        return new string[] { "DisplayName|" + instanceDisplayName, "UserSuppliedName|" + useName };
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null) 
        { 
            Debug.Log("Failed to load audio component info."); 
            return; 
        }

        componentName = component.GetComponentName();
        //Debug.Log("DisplayName : " + instanceDisplayName);
        instanceDisplayName = component.GetParamValueAsStringByName("DisplayName");
        instanceUserSuppliedName = component.GetParamValueAsStringByName("UserSuppliedName");

        //objectInfo.StartCoroutine(objectInfo.SetDisplayName(instanceDisplayName));
        objectInfo.SetDisplayName(instanceDisplayName);

        string useName = instanceUserSuppliedName;
        if (string.IsNullOrEmpty(useName))
        {
            useName = instanceDisplayName;
        }
        //objectInfo.StartCoroutine(objectInfo.SetUserSuppliedName(useName));
        objectInfo.InstanceName = useName;
        objectInfo.gameObject.name = useName;
    }
}
