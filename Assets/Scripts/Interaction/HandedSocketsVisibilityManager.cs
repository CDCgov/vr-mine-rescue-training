using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandedSocketsVisibilityManager : MonoBehaviour
{
    public GameObject[] RightHandedSocketArt;
    public GameObject[] LeftHandedSocketArt;

    public GameObject RightHandedSocketCheck;
    public GameObject LeftHandedSocketCheck;

    //public PlayerManager PlayerManager;

    private void Start()
    {
        //PlayerManager = PlayerManager.GetDefault();

        //PlayerManager.RegisterPlayerMessageHandler(OnPlayerMessage);
    }


    //private void OnPlayerMessage(VRNPlayerMessageType messageType, VRNPlayerMessage msg)
    //{
    //    switch (messageType)
    //    {
    //        case VRNPlayerMessageType.PmSetDominantHand:
    //            if (msg.PlayerMessageDataCase == VRNPlayerMessage.PlayerMessageDataOneofCase.IntData)
    //            {
    //                PlayerDominantHand dominantHand = (PlayerDominantHand)msg.IntData;
    //                Debug.Log($"HandedSocketsVisibilityManager: PlayerMessage - Setting dominant hand to {dominantHand}");
    //                SetPlayerHandedness(dominantHand);
    //            }
    //            break;
    //    }
    //}

    private void Update()
    {
        if (LeftHandedSocketCheck != null)
        {
            if (LeftHandedSocketCheck.transform.childCount > 0)
            {
                SetPlayerHandedness(PlayerDominantHand.LeftHanded);
            }
            else
            {
                SetPlayerHandedness(PlayerDominantHand.RightHanded);
            }
        }
    }

    void SetPlayerHandedness(PlayerDominantHand playerHandedness)
    {
        switch (playerHandedness)
        {
            case PlayerDominantHand.RightHanded:
                SetArtVisibility(RightHandedSocketArt, true);
                SetArtVisibility(LeftHandedSocketArt, false);
                break;
            case PlayerDominantHand.LeftHanded:
                SetArtVisibility(RightHandedSocketArt, false);
                SetArtVisibility(LeftHandedSocketArt, true);
                break;
            default:
                break;
        }
    }

    void SetArtVisibility(GameObject[] arts, bool isVisible)
    {
        if(arts == null)
        {
            return;
        }
        if(arts.Length == 0)
        {
            return;
        }
        foreach (GameObject art in arts)
        {
            if(art == null)
            {
                continue;
            }
            art.SetActive(isVisible);
        }
    }
}
