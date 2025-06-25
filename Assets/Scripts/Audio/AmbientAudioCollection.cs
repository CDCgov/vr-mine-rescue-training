using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AmbientAudioCollection", menuName = "VRMine/AmbientAudioCollection", order = 1)]
public class AmbientAudioCollection : ScriptableObject
{
    public AmbientAudioClip[] AmbientAudios;
}
