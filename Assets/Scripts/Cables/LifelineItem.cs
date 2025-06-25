using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifelineItem : MonoBehaviour {

    [HideInInspector]
    public Vector3 SavedClosestPoint;
    
    public int ClosestPointIndex = 0;
    [HideInInspector]
    public LifelineMarkerGenerator LifeLineGenRef;

    public enum ItemType
    {
        Mandoor,
        SCSRCache,
        Refuge,
        Directional,
        Tag,
        Branchline,

    }
    public ItemType itemType;
    public bool flippedDirection;

}
