using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DebriefMousewheelZoom : MonoBehaviour, IScrollHandler
{
    public DebriefOverviewUIController DebriefUIController;

    public void OnScroll(PointerEventData eventData)
    {
        Debug.Log($"DebriefZoom: {eventData.delta}");
        if (DebriefUIController == null)
            return;

        if (eventData.delta.y != 0)
        {
            DebriefUIController.Zoom(eventData.delta.y / 3.0f);
        }
    }

    void Update()
    {
        RectTransform rt = transform as RectTransform;

        float wheel = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(wheel) <= 0)
            return;

        //bool overUIObject = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() //NOTE: Check done couple lines down checking if it's within the overview rect space
        //if (overUIObject)
        //    return;
        if (EventSystem.current != null)
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            if (pointer != null) 
            {
                pointer.position = Input.mousePosition;
                List<RaycastResult> raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, raycastResults);
                if (raycastResults.Count > 0)
                {
                    foreach (RaycastResult result in raycastResults)
                    {
                        if (result.gameObject.TryGetComponent<UIMousePanAndZoom>(out var mousePanAndZoom))
                        {
                            return;
                        }
                    }
                }
            }
        }
        if (RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition))
        {
            //Debug.Log($"MOUSE INSIDE RECT");
            DebriefUIController.Zoom(wheel);
        }
    }
}
