using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LODValues
{
    public List<LODLevelData> DefaultValues;
}

[CreateAssetMenu(menuName = "Scenario Editor/LOD Level Defaults")]
public class LODLevelDefaults : ScriptableObject
{
    public List<LODValues> DefaultValues;
}
