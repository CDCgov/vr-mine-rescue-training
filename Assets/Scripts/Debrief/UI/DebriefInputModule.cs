using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DebriefInputModule : StandaloneInputModule
{
    public void GetHoveredObject()
    {
        var pointerData = this.GetLastPointerEventData(0);
        if (pointerData != null && pointerData.pointerCurrentRaycast.gameObject != null)
        { 
            var obj = pointerData.pointerCurrentRaycast.gameObject;
            Debug.Log($"Hover Object: {obj.name}");
        }

    }
}