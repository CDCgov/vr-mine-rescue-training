using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

//public enum MinerEquipmentType
//{
//    GasMeter,
//    SoundingStick,
//    Sentinel,
//    Chalk,
//    Radio,
//    Map,
//}

[System.Serializable]
public class PlayerRoleData
{
    public VRNPlayerRole PlayerRole;
    public List<VRNPlayerEquipmentType> Equipment;
    public GameObject ThirdPersonPrefabOverride;
    public bool BlockAdditionalEquipment;
}

[System.Serializable]
public class PlayerEquipmentData
{
    public VRNPlayerEquipmentType EquipmentType;
    public AssetReference Asset;
}


[CreateAssetMenu(fileName = "PlayerRoleManager", menuName = "VRMine/Managers/PlayerRoleManager", order = 0)]
public class PlayerRoleManager : Manager
{

    public const string DefaultResourcePath = "Managers/PlayerRoleManager";
    public static PlayerRoleManager GetDefault() { return Resources.Load<PlayerRoleManager>(DefaultResourcePath); }

    public List<PlayerRoleData> PlayerRoles;

    public List<PlayerEquipmentData> PlayerEquipment;

    private Dictionary<VRNPlayerRole, PlayerRoleData> _playerRoleMap;
    private Dictionary<VRNPlayerEquipmentType, PlayerEquipmentData> _playerEquipMap;

    public string GetEquipmentAsset(VRNPlayerEquipmentType equipment)
    {
        if (_playerEquipMap == null)
            BuildMaps();

        PlayerEquipmentData data;
        if (!_playerEquipMap.TryGetValue(equipment, out data))
            return null;

        return data.Asset.AssetGUID;
    }

    public PlayerRoleData GetPlayerRoleData(VRNPlayerRole role)
    {
        if (_playerRoleMap == null)
            BuildMaps();

        PlayerRoleData data;
        if (!_playerRoleMap.TryGetValue(role, out data))
            return null;

        return data;
    }

    

    private void BuildMaps()
    {
        _playerRoleMap = new Dictionary<VRNPlayerRole, PlayerRoleData>();
        _playerEquipMap = new Dictionary<VRNPlayerEquipmentType, PlayerEquipmentData>();
        

        foreach (var roleData in PlayerRoles)
        {
            _playerRoleMap.Add(roleData.PlayerRole, roleData);
        }

        foreach (var equipData in PlayerEquipment)
        {
            _playerEquipMap.Add(equipData.EquipmentType, equipData);
        }
    }
}
