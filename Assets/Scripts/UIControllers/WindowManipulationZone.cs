using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowManipulationZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
{

    public enum ManipulationZone
    {
        None,
        Movement,
        Right,
        Left,
        Top,
        Bottom,
        TopRight,
        BottomRight,
        TopLeft,
        BottomLeft,
    }
    [HideInInspector] public ResizableWindow parentWindow;
    public ManipulationZone zone;
    [HideInInspector] public RectTransform rt;
    public List<WindowLink> links = new List<WindowLink>();
    public WindowLink adjacentLink;
    void Awake()
    {
        rt = GetComponent<RectTransform>();
        parentWindow = GetComponentInParent<ResizableWindow>();
        parentWindow.dragZones.Add(zone, this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //if (link) link.OnPointerEnterFromZone(zone);
        if (parentWindow) parentWindow.OnPointerEnterFromZone(zone);

    }
    public void OnPointerExit(PointerEventData eventData)
    {
        //if (link) link.OnPointerExitFromZone(zone);
        if (parentWindow) parentWindow.OnPointerExitFromZone(zone);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (links.Count > 0)
        {
            foreach (WindowLink link in links)
            {
                link.OnPointerDownFromZone(zone);
            }
        }
        if (parentWindow) parentWindow.OnPointerDownFromZone();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (links.Count > 0)
        {
            foreach (WindowLink link in links)
            {
                link.OnPointerUpFromZone(zone);
            }
        }
   
        if (parentWindow) parentWindow.OnPointerUpFromZone();
    }

    public void OnDrag(PointerEventData eventData)
    {
        //bypass parent and drag from link if available
        if (links.Count > 0)
        {
            foreach (WindowLink link in links)
            {
                link.OnDragFromZone(eventData.delta, zone);
            }
        }

        //if (link) link.OnDragFromZone(eventData.delta, zone);
        else
        {
            if (parentWindow)
            {
                parentWindow.OnDragFromZone(eventData.delta);
                //if (adjacentLink) adjacentLink.OnDragFromAdjacentZone(parentWindow);
            }
        } 
    }

}
