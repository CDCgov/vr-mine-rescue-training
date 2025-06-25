using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PlayerRoleColor
{
    public VRNPlayerRole PlayerRole;
    public Color RoleColor;
}

[CreateAssetMenu(fileName = "PlayerColorManager", menuName = "VRMine/Managers/PlayerColorManager", order = 0)]
public class PlayerColorManager : Manager
{
    public const string DefaultResourcePath = "Managers/PlayerColorManager";
    public static PlayerColorManager GetDefault() { return Resources.Load<PlayerColorManager>(DefaultResourcePath); }

    //Recommended Player Colors
    //Red - R: 155 G: 0 B: 0
    //Gold - R: 212 G: 175 B: 55
    //Green - R: 0 G: 155 B: 0
    //Blue - R: 0 G: 0 B: 155
    //Purple - R: 155 G: 0 B: 155
    //Gray - R: 155 G: 155 B: 155

    public List<Color> PlayerColors;
    public List<PlayerRoleColor> PlayerRoleColors;

    //private Dictionary<VRNPlayerRole, Color> _roleColorCache;

    //public void OnEnable()
    //{
    //	Debug.Log($"PlayerColorManager Enabled: {PlayerRoleColors.Count} role colors found");

    //}

    public Color GetPlayerColor(VRNPlayerRole role)
    {
        foreach (var roleColor in PlayerRoleColors)
        {
            if (roleColor.PlayerRole == role)
            {
                return roleColor.RoleColor;
            }
        }

        return Color.magenta;
    }

    public Color GetPlayerColor(int playerId)
    {
        if (PlayerColors == null || PlayerColors.Count <= 0 || 
            playerId < 0 || playerId >= PlayerColors.Count)
        {
            return Color.white;
        }

        int index = playerId;
        while (index >= PlayerColors.Count)
            index -= PlayerColors.Count;

        return PlayerColors[index];
    }
}
