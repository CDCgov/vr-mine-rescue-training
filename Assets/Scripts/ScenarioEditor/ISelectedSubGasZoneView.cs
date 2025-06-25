using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISelectedSubGasZoneView 
{
    public void SetSubGasZone(ComponentInfo_StaticGasZone gasZone, SubGasZoneData data);
}
