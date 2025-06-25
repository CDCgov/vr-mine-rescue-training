using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenEdge : MonoBehaviour
{
    [SerializeField] RectTransform anchorTargetRt;
    [SerializeField] RectTransform windowSpaceRt;
    [SerializeField] Vector2 windowSpaceBounds;
    [SerializeField] bool horizontal;
    [SerializeField] bool positionToTarget;
    [SerializeField] bool scaleToTarget;

    RectTransform rt;
    WindowSnappingZone snappingZone;
 
    private void Start()
    {
        rt = GetComponent<RectTransform>();
        snappingZone = GetComponentInChildren<WindowSnappingZone>();
    }

    private void Update()
    {
        if (rt.hasChanged)
        {
            snappingZone.ResizeCollision();
            RepositionEdge();
            rt.hasChanged = false;
        }
    }
     
    private void RepositionEdge()
    {
        //Debug.Log("Recieve Event: OnScreenChanged");
        SetWindowSpaceBounds();
        
        if (positionToTarget) rt.position = anchorTargetRt.position;

        if (scaleToTarget)
        {
            if (horizontal) rt.sizeDelta = new Vector2(windowSpaceBounds.x, rt.sizeDelta.y);
            else rt.sizeDelta = new Vector2(rt.sizeDelta.x, windowSpaceBounds.y);
        }        
    }
    private void SetWindowSpaceBounds()
    {
        windowSpaceBounds = new Vector2(windowSpaceRt.rect.width, windowSpaceRt.rect.height);
    }
}
