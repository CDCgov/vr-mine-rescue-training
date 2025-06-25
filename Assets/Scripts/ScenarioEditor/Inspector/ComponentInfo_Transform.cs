using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class ComponentInfo_Transform
{
    public Transform transformComponent;
    
    public Inspector.ExposureLevel positionExposureLevel;
    public Vector3 position;
    public Inspector.ExposureLevel rotationExposureLevel;
    public Quaternion rotation;
    public Inspector.ExposureLevel scaleExposureLevel;
    public Vector3 localScale;
}
