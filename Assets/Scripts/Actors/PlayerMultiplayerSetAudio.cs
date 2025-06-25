using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerMultiplayerSetAudio : MonoBehaviour
{
    public PlayerManager PlayerManager;
    public AudioMixer Mixer;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        PlayerManager.RegisterPlayerMessageHandler(OnPlayerMessage);
    }

    private void OnDestroy()
    {
        if (PlayerManager != null)
        {
            PlayerManager.UnregisterPlayerMessageHandler(OnPlayerMessage);
        }
    }

    void OnPlayerMessage(VRNPlayerMessageType messageType, VRNPlayerMessage msg)
    {
        string property = "";
        switch (messageType)
        {
            case VRNPlayerMessageType.PmSetMasterVolume:
                property = "masterVol";
                break;
            case VRNPlayerMessageType.PmSetCollisionMasterVolume:
                property = "collisionsMasterVol";
                break;
            case VRNPlayerMessageType.PmSetFireMasterVolume:
                property = "fireMasterVol";
                break;
            case VRNPlayerMessageType.PmSetFootfallsVolume:
                property = "footfallsVol";
                break;
            case VRNPlayerMessageType.PmSetFireBurningVolume:
                property = "fireBurningVol";
                break;
            case VRNPlayerMessageType.PmSetFireExtinguished:
                property = "fireExtinguishedVol";
                break;
            case VRNPlayerMessageType.PmSetFireExtinguisher:
                property = "fireExtinguisherVol";
                break;
            case VRNPlayerMessageType.PmSetGeneralCollisionsVolume:
                property = "generalCollisionVol";
                break;
            case VRNPlayerMessageType.PmSetStretcherCollisionVolume:
                property = "stretcherCollisionVol";
                break;            
        }

        if(property != "" && Mixer != null)
        {
            Mixer.SetFloat(property, msg.FloatData);
        }
    }
}
