using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scenario Editor/CableHangerData")]
public class CableHangerData : ScriptableObject
{
    public string CableHangerID;
    public string Name;
    public GameObject Prefab;
    public float CableHangerWidth = 0.03f;
}
