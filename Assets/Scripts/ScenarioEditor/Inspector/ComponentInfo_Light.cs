using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[Serializable]
public class ComponentInfo_Light : ModularComponentInfo, ISaveableComponent
{
    [Tooltip("The name of the component as it appears in the inspector, also used to reference this information so the name should be unique for the prefab. This should be assigned in both the editor and scenario prefab")]
    public string componentName = "Light";
    //protected ObjectInfo objectInfo;

//    [Tooltip("The assigned component that lives in the prefab. The reference to the component does not save and must be assigned in the scenario prefab. It should be assigned in the editor prefab if the available, but is not required ")]
    [Header("OBSOLETE")]
    public HDAdditionalLightData m_component;
    [Header("OBSOLETE")]
    public List<HDAdditionalLightData> m_AdditionalLights;
    [Header("Add all lights to this list")]
    public List<Light> m_Lights;
    public List<Material> emissiveMaterials;
    public List<MeshRenderer> emissiveMeshRenderers;
    public Inspector.ExposureLevel IntensityExposureLevel;

    public float intensity;
    public Inspector.ExposureLevel RangeExposureLevel;
    public float range;
    public Inspector.ExposureLevel ColorExposureLevel;
    public Color color;
    public bool isOn = true;
    //public float Hue = 1;
    public float Temperature = 6500;
    public string RGBValue = "CFCFCF";
    public bool CastShadows = false;

    public float VolumetricIntensity = 1.0f;

    public bool UsesSeperateModelWhenOff = false;

    //private void Awake()
    //{
    //    objectInfo = GetComponent<ObjectInfo>();
    //    if (objectInfo == null) objectInfo = GetComponentInParent<ObjectInfo>();
    //    if (objectInfo != null)
    //    {
    //        if (!objectInfo.componentInfo_Lights.Contains(this)) objectInfo.componentInfo_Lights.Add(this);
    //    }
    //}

    void Awake()
    {
        //override alpha to one
        color.a = 1;
    }

    public string[] SaveInfo()
    {
        Debug.Log("Save Intensity At: " + intensity);
        Debug.Log("Save Range At: " + range);
        RGBValue = ColorUtility.ToHtmlStringRGB(color);
        return new string[] 
        { 
            "Intensity|" + intensity, 
            "Range|" + range, 
            "IsOn|" + isOn, 
            "Temperature|" + Temperature, 
            "RGBValue|#" + RGBValue,
            "CastShadows|" + CastShadows,
            "VolumetricIntensity|" + VolumetricIntensity.ToString("F3"),
        };
    }
    public string SaveName()
    {
        return componentName;
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
        {
            Debug.Log("Failed to load light component info. Saved component is null for " + gameObject.name); return;
        }
        componentName = component.GetComponentName();
        float.TryParse(component.GetParamValueAsStringByName("Intensity"), out intensity);
        float.TryParse(component.GetParamValueAsStringByName("Range"), out range);
        bool.TryParse(component.GetParamValueAsStringByName("IsOn"), out isOn);
        float.TryParse(component.GetParamValueAsStringByName("Temperature"), out Temperature);
        ColorUtility.TryParseHtmlString(component.GetParamValueAsStringByName("RGBValue"), out color);
        color.a = 1;
        Debug.Log($"Color loaded: {color.ToString()} but should be {component.GetParamValueAsStringByName("RGBValue")}");

        CastShadows = component.GetParamValueBool("CastShadows", false);
        VolumetricIntensity = component.GetParamValueFloat("VolumetricIntensity", 1.0f);

        bool volumetricEnabled = VolumetricIntensity > 0 ? true : false;

        foreach (var light in GetLights())
        {
            light.enabled = isOn;
        }

        foreach (var hdrpLight in GetLightsHDRP())
        {
            hdrpLight.lightUnit = LightUnit.Lumen;
            hdrpLight.SetIntensity(intensity, LightUnit.Lumen);
            hdrpLight.SetRange(range);
            hdrpLight.EnableColorTemperature(true);
            hdrpLight.SetColor(color, Temperature);
            hdrpLight.EnableShadows(CastShadows);
            hdrpLight.affectsVolumetric = volumetricEnabled;
            hdrpLight.volumetricDimmer = VolumetricIntensity;
        }

        if(emissiveMeshRenderers != null)
        {
            Color col = color * Mathf.CorrelatedColorTemperatureToRGB(Temperature);
            foreach (var meshRen in emissiveMeshRenderers)
            {
                Material mat = new Material(meshRen.material);
                float alpha = mat.color.a;
                mat.SetColor("_EmissiveColor", col);//swapping this around to try and preserve manually set alpha values ( allow for emssive "glass")
                col.a = alpha;
                mat.color = col;                
                meshRen.material = mat;

                if(intensity <= 0 || !isOn)
                {
                    //meshRen.enabled = false;
                    if (UsesSeperateModelWhenOff)
                    {
                        meshRen.enabled = false;
                    }
                    else
                    {
                        mat.SetColor("_EmissiveColor", Color.black);
                    }
                }
            }
            Debug.Log($"Setting emissive color from Component Info? {color.ToString()}");
        }
    }


    public IEnumerable<Light> GetLights()
    {
        //if (TargetComponentInfo == null || TargetComponentInfo.m_Lights == null || TargetComponentInfo.m_Lights.Count <= 0)
        //    yield break;

        if (m_Lights == null || m_Lights.Count <= 0)
            yield break;

        foreach (var light in m_Lights)
            yield return light;
    }

    public IEnumerable<HDAdditionalLightData> GetLightsHDRP()
    {
        foreach (var light in GetLights())
        {
            if (light.TryGetComponent<HDAdditionalLightData>(out var hdrpLight))
            {
                yield return hdrpLight;
            }
        }
    }

}
