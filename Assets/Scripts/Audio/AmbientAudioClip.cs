using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AmbientAudioClip", menuName = "VRMine/AmbientAudioClip", order = 1)]
public class AmbientAudioClip : ScriptableObject
{
    public string ClipName;
    public AudioClip Clip;
    public Range PitchRange;
    public float DefaultVolume = 1;
}
