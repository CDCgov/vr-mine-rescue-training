using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DustingHelper
{
    None,
    Medium,
    Full
}

public enum TileType
{
    FourWay,
    ThreeWay,
    Corner,
    Straight,
    EndCap
}


public class CreateMinePlacementHelper : MonoBehaviour {
    public DustingHelper DustLevel;
}
