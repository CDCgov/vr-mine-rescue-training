using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCKnockResponse", menuName = "VRMine/NPCKnockResponse", order = 2)]
public class NPCKnockResonses : ScriptableObject
{
    public SoundCollection SoloResponses;
    public SoundCollection DuoResponses;
    public SoundCollection GroupResponses;
    public SoundCollection GenericResponses;
    public SoundCollection AirtightResponses;
}
