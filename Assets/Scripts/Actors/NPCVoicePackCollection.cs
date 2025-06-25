using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "NPCVoiceCollection", menuName = "VRMine/VoicePackCollection", order = 2)]
public class NPCVoicePackCollection : ScriptableObject
{
    public NPCVoicePack[] NPCVoicePacks;

    public NPCVoicePack GetCollectionByString(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return NPCVoicePacks[0];
        }
        else
        {
            return NPCVoicePacks.Where(sound => sound.VoiceName == name).First();
        }
    }
}
