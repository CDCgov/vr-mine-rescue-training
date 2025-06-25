using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/ScenarioEditor/OverheadCameraSettings", order =1)]
public class OverheadCameraSettingsObject : ScriptableObject
{
    public string settingsName;
    public Vector3 pivotStartPoint;
    public float minZoom, maxZoom;
    public float startHeight;
    public float zoomSpeed;
    public float maxPitchX;
    public float minPitchX;
}
