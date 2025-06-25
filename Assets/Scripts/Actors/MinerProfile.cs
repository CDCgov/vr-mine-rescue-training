using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MinerProfile", menuName = "VRMine/MinerProfile", order = 1)]
public class MinerProfile : ScriptableObject
{
    public string MinerProfileID;
    public string DisplayName;
    public GameObject ThirdPersonPrefab;
    public List<VRNPlayerEquipmentType> DisabledEquipment;
    public List<VRNPlayerEquipmentType> AddedEquipment;
    public bool EnableBG4 = true;
}
