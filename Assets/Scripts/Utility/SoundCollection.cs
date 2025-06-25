using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Sounds", menuName = "VRMine/Sounds", order = 1)]
public class SoundCollection : ScriptableObject
{
    public AudioClip[] Sounds;
}
