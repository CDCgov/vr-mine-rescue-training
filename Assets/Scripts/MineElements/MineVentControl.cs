using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MineVentControl : MineElement
{
    public double AddedResistance;

    [System.NonSerialized]
    public MineSegment AssociatedSegment;
}
