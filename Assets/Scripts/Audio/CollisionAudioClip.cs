using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CollisionAudioClip", menuName = "VRMine/CollisionAudioClip", order = 1)]
public class CollisionAudioClip : ScriptableObject
{
    public AudioClip CollisionClip;
    public Range PitchRange = new Range(0.5f, 1.5f);
}
