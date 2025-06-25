using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/ScenarioEditor/ScenarioEditorCameraSettings")]
public class ScenarioEditorCameraSettingsObject : ScriptableObject
{
    public string settingsName;

    public float maxPitch, minPitch, startPitch, pitchSpeed;
    public bool invertPitch = false;
    
    public float startYaw, yawSpeed;
    public bool invertYaw = false;
    
    public float minZoom, maxZoom, startZoom, focusZoom, zoomSpeed;

    public float minOrthoSize = 1;
    public float maxOrthoSize = 100;
    
    public Vector3 pivotStartPoint;
    public float moveSpeed = 20;




}
