using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MinerProfiles", menuName = "VRMine/Miner Profile List", order = 2)]
public class MineProfiles : ScriptableObject
{
    public List<MinerProfile> MineProfileList;
}
