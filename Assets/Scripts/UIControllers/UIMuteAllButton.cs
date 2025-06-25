using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMuteAllButton : MonoBehaviour
{
    public PlayerManager PlayerManager;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
    }

    public void MuteAll()
    {
        foreach (KeyValuePair<int, PlayerRepresentation> kvp in PlayerManager.PlayerList)
        {
            PlayerManager.SendPlayerMessage(kvp.Key, VRNPlayerMessageType.PmMutePlayer, true);
        }
    }

    public void UnmuteAll()
    {
        foreach (KeyValuePair<int, PlayerRepresentation> kvp in PlayerManager.PlayerList)
        {
            PlayerManager.SendPlayerMessage(kvp.Key, VRNPlayerMessageType.PmMutePlayer, false);
        }
    }
}
