using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Scenario Editor/SkyboxData")]
public class SkyboxData : ScriptableObject
{
    public string SkyboxID;
    public string Name;

    public VolumeProfile VolumeProfile;
    public GameObject LightPrefab;
    public int SortOrder = 0;
}
