using NIOSH_MineCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[Serializable]
public class ComponentInfo_Airway : ModularComponentInfo
{
    [Tooltip("The name of the component as it appears in the inspector, also used to reference this information so the name should be unique for the prefab. This should be assigned in both the editor and scenario prefab")]
    public string componentName = "Airway Properties";
    //protected ObjectInfo objectInfo;

    public Inspector.ExposureLevel AddedResistanceExposureLevel;
    public float AddedResistance;
    public Inspector.ExposureLevel OverrideResistanceExposureLevel;
    public float OverrideResistance ;
    public Inspector.ExposureLevel CH4RateExposureLevel;
    public float CH4EmissionRate ;
    public Inspector.ExposureLevel FricitonFactorExposureLevel;
    public float FrictionFactor ;

    public VentilationConnectionAirway AirwayComponent;

    //private void Awake()
    //{
    //    objectInfo = GetComponent<ObjectInfo>();
    //    if (objectInfo == null) objectInfo = GetComponentInParent<ObjectInfo>();
    //    if (objectInfo != null)
    //    {
    //        if (!objectInfo.componentInfo_Airway.Contains(this)) objectInfo.componentInfo_Airway.Add(this);
    //    }
    //}
}
