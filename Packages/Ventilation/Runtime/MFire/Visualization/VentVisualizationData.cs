using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VentVisualizationParameter
{
    Methane,
    Contaminant,
    Temperature,
}

/// <summary>
/// Supplemental data and parameters used for ventilation visualization
/// </summary>
[CreateAssetMenu(fileName = "VentVisualizationData", menuName = "VRMine/VentVisualizationData", order = 0)]
public class VentVisualizationData : ScriptableObject
{
    public VentVisualizationParameter VisualizationParameter;
    public Gradient ColorGradient;
    public float ParamMinValue = 0;
    public float ParamMaxValue = 1;
}
