using NIOSH_MineCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[Serializable]
public class ComponentInfo_JunctionProperties : ModularComponentInfo
{
    [Tooltip("The name of the component as it appears in the inspector, also used to reference this information so the name should be unique for the prefab. This should be assigned in both the editor and scenario prefab")]
    public string componentName = "Junction Properties";
    //protected ObjectInfo objectInfo;

    public Inspector.ExposureLevel IsStartExposureLevel;
    public bool isStartJunction;
    public Inspector.ExposureLevel InAtmosphereExposureLevel;
    public bool isInAtmosphere ;
    public Inspector.ExposureLevel CH4ExposureLevel;
    public float ch4Concentration ;
    public Inspector.ExposureLevel ContaminantExposureLevel;
    public float contaminantConcentration ;

    public VentilationLayerNode nodeComponent;

    //private void Awake()
    //{
    //    objectInfo = GetComponent<ObjectInfo>();
    //    if (objectInfo == null) objectInfo = GetComponentInParent<ObjectInfo>();
    //    if (objectInfo != null)
    //    {
    //        if (!objectInfo.componentInfo_JunctionProperties.Contains(this)) objectInfo.componentInfo_JunctionProperties.Add(this);
    //    }
    //}
}
