using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropRegion : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public event Action<PointerEventData> BeginDrag;
    public event Action<PointerEventData> EndDrag;
    public event Action<PointerEventData> Drag;

    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDrag?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Drag?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDrag?.Invoke(eventData);
    }
}
