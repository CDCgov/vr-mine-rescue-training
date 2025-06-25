using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    Default,
    Wood,
    Metal,
    Rock,
    Plastic,
    CrumblingRock,
    GoodRoof,
    BadRoof
}
public class SoundingType : MonoBehaviour 
{
    public SoundType SoundMaterial = SoundType.Default;
    public SoundingStickSounds MaterialSounds;
    public int Priority = 10;
}
