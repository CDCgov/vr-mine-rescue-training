using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "VRMine/ScenarioData")]
public class ScenarioData : ScriptableObject
{
    public string ScenarioName;
    public bool AddressableScene = false;
    public string SceneName;
}
