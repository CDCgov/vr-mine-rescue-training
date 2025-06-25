using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCVoicePack", menuName = "VRMine/VoicePack", order = 2)]
public class NPCVoicePack : ScriptableObject
{
    public string VoiceName;
    public SoundCollection FollowResponses;
    public SoundCollection WaitResponses;
    public SoundCollection IdleResponses;
    public SoundCollection KnockResponses;
    public NPCKnockResonses NPCResponses;
}
