using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class WindowLink : MonoBehaviour
{
    public bool forceLink;
    public bool forceBreakLink;

    bool horizontalLink;

    public ResizableWindow window_A; // left or bottom
    public ResizableWindow window_B; // right or top
    
    public WindowManipulationZone manipulationZone_A; // left or bottom
    public WindowManipulationZone manipulationZone_B; // right or top
    //public List<WindowManipulationZone> adjacentZones = new List<WindowManipulationZone>();

    //cache these so we can turn them back on
    WindowSnappingZone snapZone_A;
    WindowSnappingZone snapZone_B;

    WindowManipulationZone.ManipulationZone zone_A;
    WindowManipulationZone.ManipulationZone zone_B;

    RectTransform rt;
    RectTransform windowRt_A;
    RectTransform windowRt_B;

    RectTransform zoneRt_A;
    RectTransform zoneRt_B;

    private bool dragging;

    Vector2 maxSize_A;
    Vector2 maxSize_B;

    [SerializeField] bool isResizableLink;
    bool changeStarted_A;
    bool changeStarted_B;

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            changeStarted_A = false;
            changeStarted_B = false;
        }
        if (forceLink)
        {
            Initialize();
            forceLink = false;
        }
        
    }

    public void Initialize()
    {
        
        rt = GetComponent<RectTransform>();
        rt.SetAsFirstSibling();

        manipulationZone_A.links.Add(this);
        manipulationZone_B.links.Add(this);
        snapZone_A = manipulationZone_A.gameObject.GetComponent<WindowSnappingZone>();
        snapZone_B = manipulationZone_B.gameObject.GetComponent<WindowSnappingZone>();
        snapZone_A.linked = true;
        snapZone_B.linked = true;
        snapZone_A.enabled = false;
        snapZone_B.enabled = false;


        window_A = manipulationZone_A.parentWindow;
        window_B = manipulationZone_B.parentWindow;
        if (isResizableLink) window_A.hasResizableLink = true;
        if (isResizableLink) window_B.hasResizableLink = true;

        //To D0 - replace with delegates
        window_A.onBreakLinks.AddListener(BreakLink);
        window_B.onBreakLinks.AddListener(BreakLink);


        windowRt_A = window_A.gameObject.GetComponent<RectTransform>();
        windowRt_B = window_B.gameObject.GetComponent<RectTransform>();

        zone_A = manipulationZone_A.zone;
        zone_B = manipulationZone_B.zone;

        horizontalLink = (zone_A == WindowManipulationZone.ManipulationZone.Left || zone_A == WindowManipulationZone.ManipulationZone.Right);

        zoneRt_A = manipulationZone_A.gameObject.GetComponent<RectTransform>();
        zoneRt_B = manipulationZone_B.gameObject.GetComponent<RectTransform>();


        /*SetAdjacentZones();
        foreach (WindowManipulationZone z in adjacentZones)
        {
            z.adjacentLink = this;
        }*/

        //forceLink = false;

        IEnumerator coroutine = LastUpdate();
        StartCoroutine(coroutine);

        SetPosition();
        SetSize();
    }
    IEnumerator LastUpdate()
    {
        while (true)
        {
            // wait for next frame to make sure dragging hasn't started
            yield return null;
            if (windowRt_A.hasChanged) changeStarted_A = true;
            if (windowRt_B.hasChanged) changeStarted_B = true;
            if (changeStarted_A || changeStarted_B)
            {
                SetPosition();

                if (isResizableLink && !dragging) SetSize();
            }
        }

    }

    
    public void BreakLink()
    {
        manipulationZone_A.links.Remove(this);
        manipulationZone_B.links.Remove(this);
        snapZone_A.enabled = true;
        snapZone_B.enabled = true;
        snapZone_A.linked = false;
        snapZone_B.linked = false;
        if (isResizableLink)window_A.hasResizableLink = false;
        if(isResizableLink)window_B.hasResizableLink = false;
        window_B.hasResizableLink = false;
        // destroyself
        Destroy(this.gameObject);
    }

    public void OnDestroy()
    {
        window_A.onBreakLinks.RemoveListener(BreakLink);
        window_B.onBreakLinks.RemoveListener(BreakLink);
    }

    public void OnPointerDownFromZone(WindowManipulationZone.ManipulationZone zone)
    {
        if (zone == zone_A) window_B.OnPointerDownFromLink();
        else window_A.OnPointerDownFromLink();
    }
    public void OnPointerUpFromZone(WindowManipulationZone.ManipulationZone zone)
    {
        if (zone == zone_A) window_B.OnPointerUpFromLink();
        else window_A.OnPointerUpFromLink();
        dragging = false;
    }

    public void OnDragFromZone(Vector2 delta, WindowManipulationZone.ManipulationZone zone)
    {
        // precalulcate size limits to prevent overlap or gap from floating point errors
        if (!dragging)
        {
            Vector2 startSize_A = windowRt_A.rect.size;
            Vector2 startSize_B = windowRt_B.rect.size;

            Vector2 sizeChange_A = startSize_A - window_A.minimumSize;
            Vector2 sizeChange_B = startSize_B - window_B.minimumSize;

            maxSize_A = startSize_A + sizeChange_B;
            maxSize_B = startSize_B + sizeChange_A;

            dragging = true;
            Debug.Log("StartDrag");
        }

        //Vector2 delta = eventData.delta;
        
        // dragging from link can reconcile conflict by forcing which zone to drag from locally
        window_A.OnDragFromLink(delta, zone_A);
        window_B.OnDragFromLink(delta, zone_B);

        if (window_A.Limited())
        {
            if (horizontalLink) windowRt_B.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxSize_B.x);
            else windowRt_B.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxSize_A.y);

        }
        if (window_B.Limited())
        {

            if (horizontalLink) windowRt_A.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxSize_A.x);
            else windowRt_A.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxSize_A.y);
        }

        //SetPosition();
    }
    
    /// <summary>
    /// When changing a window using an adjacent zone, this carries over changes made in size/position to the other window
    /// </summary>
    /// <param name="window"></param>
    public void SetSize()
    {
        if (changeStarted_A)
        {
            windowRt_B.SetAnchoringFixedWithSize(true);
            windowRt_B.SetPivot(windowRt_A.pivot);
            if (horizontalLink)
            {
                windowRt_B.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, windowRt_A.rect.height);
                windowRt_B.position = new Vector3(windowRt_B.position.x, windowRt_A.position.y, windowRt_B.position.z);
            }
            else
            {
                windowRt_B.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, windowRt_A.rect.width);
                windowRt_B.position = new Vector3(windowRt_A.position.x, windowRt_B.position.y, windowRt_B.position.z);
            }
        }

        else if (changeStarted_B)
        {
            windowRt_A.SetAnchoringFixedWithSize(true);
            windowRt_A.SetPivot(windowRt_B.pivot);
            if (horizontalLink)
            {
                windowRt_A.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, windowRt_B.rect.height);
                windowRt_A.position = new Vector3(windowRt_A.position.x, windowRt_B.position.y, windowRt_A.position.z);
            }
            else
            {
                windowRt_A.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, windowRt_B.rect.width);
                windowRt_A.position = new Vector3(windowRt_B.position.x, windowRt_A.position.y, windowRt_A.position.z);
            }
        }

        if(horizontalLink) rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, zoneRt_A.rect.height);
        else rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, zoneRt_A.rect.width);
    }
        
    void SetPosition()
    {
        rt.position = (zoneRt_A.position + zoneRt_B.position) / 2;
    }
    /*
    void SetAdjacentZones()
    {
        if (horizontalLink)
        {
            adjacentZones.Add(window_A.dragZones[WindowManipulationZone.ManipulationZone.Top]);
            adjacentZones.Add(window_A.dragZones[WindowManipulationZone.ManipulationZone.Bottom]);
            adjacentZones.Add(window_B.dragZones[WindowManipulationZone.ManipulationZone.Top]);
            adjacentZones.Add(window_B.dragZones[WindowManipulationZone.ManipulationZone.Bottom]);
        }
        else
        {
            adjacentZones.Add(window_A.dragZones[WindowManipulationZone.ManipulationZone.Left]);
            adjacentZones.Add(window_A.dragZones[WindowManipulationZone.ManipulationZone.Right]);
            adjacentZones.Add(window_B.dragZones[WindowManipulationZone.ManipulationZone.Left]);
            adjacentZones.Add(window_B.dragZones[WindowManipulationZone.ManipulationZone.Right]);
        }
    }*/


}
