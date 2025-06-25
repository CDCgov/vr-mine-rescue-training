using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDeafenAllButton : MonoBehaviour
{
    public PlayerManager PlayerManager;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
    }

    public void DeafenAll()
    {
        foreach (KeyValuePair<int, PlayerRepresentation> kvp in PlayerManager.PlayerList)
        {
            PlayerManager.SendPlayerMessage(kvp.Key, VRNPlayerMessageType.PmDeafenPlayer, true);
        }
    }

    public void UndeafenAll()
    {
        foreach (KeyValuePair<int, PlayerRepresentation> kvp in PlayerManager.PlayerList)
        {
            PlayerManager.SendPlayerMessage(kvp.Key, VRNPlayerMessageType.PmDeafenPlayer, false);
        }
    }
}
