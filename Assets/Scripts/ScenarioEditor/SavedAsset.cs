using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Newtonsoft.Json;

/// <summary>
/// JSON serializable data container for a placed asset or piece of geometry in a created scenario
/// </summary>
[Serializable]
public class SavedAsset
{
    public string AssetID;
    public string BasePrefabID;
    [FormerlySerializedAs("position")] 
    public Vector3 Position;
    [FormerlySerializedAs("scale")] 
    public Vector3 Scale;
    [FormerlySerializedAs("rotation")] 
    public Quaternion Rotation;
    [FormerlySerializedAs("assetName")] 
    public string AssetName;
    [FormerlySerializedAs("distinctName")] 
    public string DistinctName;
    [FormerlySerializedAs("savedComponents")] 
    public List<SavedComponent> SavedComponents;
    [FormerlySerializedAs("isNetworked")] 
    public bool IsNetworked;
    [FormerlySerializedAs("guidString")] 
    public string GUIDString;
    [FormerlySerializedAs("isMineTile")] 
    public bool IsMineTile;

    //public SavedAsset (Vector3 position, Vector3 scale, Quaternion rotation, string distinctName, string assetName, List<SavedComponent> componentList, bool isNet, string guid, bool isMineTile)
    //{ 
    //    this.Position = position;
    //    this.Scale = scale;
    //    this.Rotation = rotation;
    //    this.AssetName = assetName;
    //    this.SavedComponents = componentList;
    //    this.IsNetworked = isNet;
    //    this.GUIDString = guid;
    //    this.IsMineTile = isMineTile;
    //    this.DistinctName = distinctName;
    //}

    public SavedComponent GetSavedComponent(string componentName)
    {
        foreach (SavedComponent item in SavedComponents)
        {
            if(item.GetComponentName() == componentName)
            {
                return item;
            }
        }
        Debug.LogError("NO COMPONENT WITH NAME: " + componentName + " LOCATED ON OBJECT: " + AssetName);
        return null;
    }

    public List<SavedComponent> GetAllSavedComponents()
    {
        return SavedComponents;
    }
}

/// <summary>
/// JSON serializable data object to hold string data about unique components attached to saved gameobject
/// </summary>
[Serializable]
public class SavedComponent
{
    [SerializeField]
    [JsonProperty]
    string componentName;

    [SerializeField]
    [JsonProperty]
    string[] parameters;

    public SavedComponent(string componentName, string[] parameters)
    {
        this.componentName = componentName;
        this.parameters = parameters;
    }

    public string GetComponentName()
    {
        return componentName;
    }

    public void SetCustomParams(string[] paramsList)
    {
        parameters = paramsList;
    }

    public string GetParamValueAsStringByIndex(int index)
    {
        string paramStringValue = parameters[index].Substring(parameters[index].IndexOf("|") +1);
        return paramStringValue;
    }

    public string GetParamValueAsStringByName(string paramName)
    {
        string param;
        int index = parameters.Length;
        for (int i = 0; i < index; i++)
        {
            param = parameters[i].Substring(0, parameters[i].IndexOf("|"));
            if(paramName.ToLower().Equals(param.ToLower()))
            {
                return parameters[i].Substring(parameters[i].IndexOf("|") + 1);
            }
        }
        Debug.LogWarning("No parameter saved with this component by the name: " + paramName);
        return null;
    }

    public string GetParamValueString(string paramName, string defaultValue = null)
    {
        var strValue = GetParamValueAsStringByName(paramName);
        if (strValue == null)
            strValue = defaultValue;
     
        return strValue;
    }

    public float GetParamValueFloat(string paramName, float defaultValue = 0)
    {
        float value = defaultValue;
        var strValue = GetParamValueAsStringByName(paramName);
        if (strValue != null)
            float.TryParse(strValue, out value);

        return value;
    }

    public double GetParamValueDouble(string paramName, double defaultValue = 0)
    {
        double value = defaultValue;
        var strValue = GetParamValueAsStringByName(paramName);
        if (strValue != null)
            double.TryParse(strValue, out value);

        return value;
    }

    public bool GetParamValueBool(string paramName, bool defaultValue = true)
    {
        bool value = defaultValue;
        var strValue = GetParamValueAsStringByName(paramName);
        if (strValue != null)
            bool.TryParse(strValue, out value);

        return value;
    }

    public Color GetParamValueColor(string paramName, Color defaultValue)
    {
        Color value = defaultValue;
        var strValue = GetParamValueAsStringByName(paramName);
        if (strValue != null)
            ColorUtility.TryParseHtmlString(strValue, out value);

        return value;
        
    }
}
