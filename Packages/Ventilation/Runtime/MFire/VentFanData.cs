using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "VentFanData", menuName = "VRMine/VentFanData", order = 0)]
public class VentFanData : ScriptableObject
{
    public double[] AirflowData;
    public double[] PressureData;
}
