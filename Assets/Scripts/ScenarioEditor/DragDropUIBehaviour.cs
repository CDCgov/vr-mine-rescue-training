using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDropUIBehaviour : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector2 lastMousePosition = Vector3.zero;
    private Vector2 lastObjectPosition = Vector3.zero;
    RectTransform rect;
    //RectTransform lastRect;
    public Action onObjectDropped;
    public Action onObjectGrabbed;
    public Action onObjectMoved;
    public PointerEventData currentPointerEvent;
    public DragDropUIBehaviour otherDrag;
    
    private void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    /// <summary>
    /// This method will be called on the start of the mouse drag
    /// </summary>
    /// <param name="eventData">mouse pointer event data</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if(otherDrag)otherDrag.OnBeginDrag(eventData);
        else
        {
            lastMousePosition = eventData.position;
            lastObjectPosition = rect.position;
            onObjectGrabbed?.Invoke();
        }
    }

    /// <summary>
    /// This method will be called during the mouse drag
    /// </summary>
    /// <param name="eventData">mouse pointer event data</param>
    public void OnDrag(PointerEventData eventData)
    {
        if (otherDrag) otherDrag.OnDrag(eventData);
        else
        {
            Vector2 currentMousePosition = eventData.position;
            Vector2 diff = currentMousePosition - lastMousePosition;
            Vector3 newPosition = rect.position + new Vector3(diff.x, diff.y, transform.position.z);
            Vector3 oldPos = rect.position;
            rect.position = newPosition;
            if (!IsRectTransformInsideSreen(rect))
            {
                rect.position = oldPos;
            }
            lastMousePosition = currentMousePosition;
            onObjectMoved?.Invoke();
            //Debug.Log("Dragging " + this.gameObject);
        }
    }

    /// <summary>
    /// This method will be called at the end of mouse drag
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (otherDrag) otherDrag.OnEndDrag(eventData);
        else
        {
            rect.position = lastObjectPosition;
            onObjectDropped?.Invoke();
            //Debug.Log("Dropped");
        }
        otherDrag = null;
    }

    /// <summary>
    /// This methods will check is the rect transform is inside the screen or not
    /// </summary>
    /// <param name="rectTransform">Rect Trasform</param>
    /// <returns></returns>
    private bool IsRectTransformInsideSreen(RectTransform rectTransform)
    {
        bool isInside = false;
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        int visibleCorners = 0;
        Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        foreach (Vector3 corner in corners)
        {
            if (rect.Contains(corner))
            {
                visibleCorners++;
            }
        }
        if (visibleCorners == 4)
        {
            isInside = true;
        }
        return isInside;
    }
    
    /// <summary>
    /// Allows us to transfer a drag to another object. Useful for dragging a UI object to another window
    /// </summary>
    /// <param name="_otherDrag"></param>
    /// <param name="eventData"></param>
    public void StartDragOther(DragDropUIBehaviour _otherDrag, PointerEventData eventData)
    {
        _otherDrag.InitializeFromOther(rect, eventData);
        otherDrag = _otherDrag;
        rect.position = lastObjectPosition;
    }
    
    /// <summary>
    /// Initialize from other drag script in use with position and on drag
    /// </summary>
    /// <param name="parentRect"></param>
    /// <param name="eventData"></param>
    public void InitializeFromOther(RectTransform parentRect, PointerEventData eventData)
    {
        rect = GetComponent<RectTransform>();
        rect.position = parentRect.position;
        OnBeginDrag(eventData);
    }

}