/*
 * NAME:LighSO.cs
 * DESC:Scriptable object containing the data of the SpotLights
*/
using UnityEngine;
[CreateAssetMenu(fileName ="lightSetting",menuName ="ScriptabaleObject/LightSO")]
public class LightSO : ScriptableObject {
    public float range;
    public float spotAngle;
    public Color color;
    public float intensity;
    public Flare flare;
}
