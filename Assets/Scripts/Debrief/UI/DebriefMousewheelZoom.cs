using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DebriefMousewheelZoom : MonoBehaviour, IScrollHandler
{
    public DebriefOverviewUIController DebriefUIController;

    private InputAction _actionZoom;

    private void Start()
    {
        _actionZoom = InputSystem.actions.FindAction("Zoom");

        _actionZoom.performed += OnZoom;
    }

    private void OnDestroy()
    {
        _actionZoom.performed -= OnZoom;
    }

    private void OnZoom(InputAction.CallbackContext context)
    {
        RectTransform rt = transform as RectTransform;

        float wheel = _actionZoom.ReadValue<float>();
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
                pointer.position = Mouse.current.GetPositionVec3();
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
        if (RectTransformUtility.RectangleContainsScreenPoint(rt, Mouse.current.GetPositionVec3()))
        {
            //Debug.Log($"MOUSE INSIDE RECT");
            DebriefUIController.Zoom(wheel);
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        Debug.Log($"DebriefZoom: {eventData.delta}");
        if (DebriefUIController == null)
            return;

        float scrollDelta = Mathf.Clamp(eventData.delta.y, -1.0f, 1.0f);

        if (eventData.delta.y != 0)
        {
            DebriefUIController.Zoom(scrollDelta * 0.1f);
        }
    }

    //void Update()
    //{
    //    RectTransform rt = transform as RectTransform;

    //    float wheel = Input.GetAxis("Mouse ScrollWheel");
    //    if (Mathf.Abs(wheel) <= 0)
    //        return;

    //    //bool overUIObject = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() //NOTE: Check done couple lines down checking if it's within the overview rect space
    //    //if (overUIObject)
    //    //    return;
    //    if (EventSystem.current != null)
    //    {
    //        PointerEventData pointer = new PointerEventData(EventSystem.current);
    //        if (pointer != null) 
    //        {
    //            pointer.position = Mouse.current.GetPositionVec3();
    //            List<RaycastResult> raycastResults = new List<RaycastResult>();
    //            EventSystem.current.RaycastAll(pointer, raycastResults);
    //            if (raycastResults.Count > 0)
    //            {
    //                foreach (RaycastResult result in raycastResults)
    //                {
    //                    if (result.gameObject.TryGetComponent<UIMousePanAndZoom>(out var mousePanAndZoom))
    //                    {
    //                        return;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    if (RectTransformUtility.RectangleContainsScreenPoint(rt, Mouse.current.GetPositionVec3()))
    //    {
    //        //Debug.Log($"MOUSE INSIDE RECT");
    //        DebriefUIController.Zoom(wheel);
    //    }
    //}
}
