using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
///  Extends moveable window class to allow for resizing of window on edge drag.
/// </summary>
public class ResizableWindow : MonoBehaviour
{
    //RectTransform limits and margins
    [HideInInspector] public RectTransform rt;
    public Vector2 minimumSize;
    [SerializeField] Vector2 moveSize;
    [SerializeField] float correctionMargin = 15;
    [SerializeField] float headerTopMargin = 35;

    [SerializeField] GameObject windowLinkPrefab;
    //Manipulation

    // [SerializeField] ManipulationZone CursorImageController.instance.currentManipulationZone;
    // ManipulationZone CursorImageController.instance.lastManipulationZone;
    // ManipulationZone CursorImageController.instance.queuedManipulationZone;

    public bool linkedDrag;
    public Dictionary<WindowManipulationZone.ManipulationZone, WindowManipulationZone> dragZones = new Dictionary<WindowManipulationZone.ManipulationZone, WindowManipulationZone>();
    public WindowManipulationZone.ManipulationZone currentDragZone;
    public Dictionary<WindowManipulationZone.ManipulationZone, CursorImageController.CursorImage> ZoneCursor = new Dictionary<WindowManipulationZone.ManipulationZone, CursorImageController.CursorImage>
    {
        {WindowManipulationZone.ManipulationZone.Movement, CursorImageController.CursorImage.HandOpen},
        {WindowManipulationZone.ManipulationZone.Top, CursorImageController.CursorImage.Resize_NS},
        {WindowManipulationZone.ManipulationZone.Bottom, CursorImageController.CursorImage.Resize_NS},
        {WindowManipulationZone.ManipulationZone.Left, CursorImageController.CursorImage.Resize_EW},
        {WindowManipulationZone.ManipulationZone.Right, CursorImageController.CursorImage.Resize_EW},
        {WindowManipulationZone.ManipulationZone.TopRight, CursorImageController.CursorImage.Resize_NE_SW},
        {WindowManipulationZone.ManipulationZone.BottomLeft, CursorImageController.CursorImage.Resize_NE_SW},
        {WindowManipulationZone.ManipulationZone.TopLeft, CursorImageController.CursorImage.Resize_NW_SE},
        {WindowManipulationZone.ManipulationZone.BottomRight, CursorImageController.CursorImage.Resize_NW_SE},
        {WindowManipulationZone.ManipulationZone.None, CursorImageController.CursorImage.Arrow},
    };


    //[SerializeField] Camera uiCamera;
    [SerializeField]RectTransform canvasRt;
    Vector2 screenBounds;

    //Priming
    public RectTransform.Edge primedEdge;

    GameObject snapPreview;
    Image snapPreviewImage;
    RectTransform snapPreviewRt;

    //Snapping
    List<WindowSnappingZone> movingSnapZones = new List<WindowSnappingZone>();
    public Dictionary<RectTransform.Edge, WindowSnappingZone> snapZones = new Dictionary<RectTransform.Edge, WindowSnappingZone>();
    public WindowSnappingZone primedZone;
    public float resizeOnSnapDistance;
    public bool resizeOnSnap;
    public RectTransform targetRt;
    public RectTransform targetSnapZoneRt;
    public RectTransform.Edge targetSnapZoneEdge;
    public WindowSnappingZone targetSnapZone;
    public WindowManipulationZone targetDragZone;
    public ResizableWindow targetWindow;
    public float targetSnapDistance;
 

    //Actions
    public bool performingAction;
    bool dragging;
    bool _limitReached;
    
    public bool hitLimitX;
    public bool hitLimitY;

   // public bool forceSetEdge;
    bool isViewport;
    public bool hasResizableLink;


    //events
    public UnityEvent onBreakLinks;
    public UnityEvent<bool> onChangePerformingAction;

    InputTargetController _inputTargetController;

    #region Unity Methods

    void Start()
    {
        _inputTargetController = FindObjectOfType<InputTargetController>();
        var viewportController = GetComponent<ScenarioEditorViewportController>();
        isViewport = (viewportController);
        rt = GetComponent<RectTransform>();
        //_originalSize = rt.sizeDelta;

        SetScreenBounds();
        ResizeSnapZoneCollision();
        //rt.SetAnchoringFixedWithSize(false);
        snapPreview = GameObject.Find("SnapPreview");
        snapPreviewImage = snapPreview.GetComponent<Image>();
        snapPreviewRt = snapPreview.GetComponent<RectTransform>();
    }

    public void Update()
    {

        if (rt.hasChanged)
        {
            SetScreenBounds();
            if(!performingAction) ValidateTransform();

            ResizeSnapZoneCollision();
            //rt.hasChanged = false;
        }

        //check distance to snap target to determine type of snap
        if (primedZone != null)
        {
            SetSnapType();
            ControlSnapPreview();
        }
    }
    private void LateUpdate()
    {
        if (rt.hasChanged)
        {
            rt.hasChanged = false;
        }
    }

    private void OnDisable()
    {
        onBreakLinks?.Invoke();
    }

    #endregion

    #region Pointer Zone Methods
    public void OnPointerDownFromLink()
    {
        rt.SetAnchoringFixedWithSize(true);
        //Debug.Log("Pointer Down From Link " + this);
    }

    public void OnPointerDownFromZone()
    {
        rt.SetAnchoringFixedWithSize(true);
        performingAction = true;
        onChangePerformingAction?.Invoke(true);
        _inputTargetController.ManipulatingWindow = true;
        if(!isViewport)rt.SetAsLastSibling();
        CursorImageController.instance.queuedManipulationZone = CursorImageController.instance.currentManipulationZone;
    }
    public void OnPointerUpFromLink()
    {
        dragging = false;
        //Debug.Log("Pointer Up From Link " + this);
    }
    public void OnPointerUpFromZone()
    {
        if (primedZone != null) SnapWindow();
        CancelAction();
        
        //set manipulation zone from queue
        CursorImageController.instance.currentManipulationZone = CursorImageController.instance.queuedManipulationZone;
        CursorImageController.instance.queuedManipulationZone = CursorImageController.instance.currentManipulationZone;
        CursorImageController.instance.ChangeCursorImage(ZoneCursor[CursorImageController.instance.currentManipulationZone]);

        rt.SetPivot( new Vector2(0.5f, 0.5f));
        rt.SetAnchoringFixedWithSize(false);
        onChangePerformingAction?.Invoke(false);
        _inputTargetController.ManipulatingWindow = false;
        ControlNonMovingZones(true);
        movingSnapZones.Clear();
    }

    /// <summary>
    /// Drag started from child pointer zone
    /// </summary>
    /// <param name="delta"></param>
    public void OnDragFromZone(Vector2 delta)
    {
        if (!performingAction) return;
       
        if (!dragging)
        {
            linkedDrag = false;
            StartManipulation();
            dragging = true;
        }
        ManipulateWindow(delta);
        return;
    }
   
    public void OnDragFromLink(Vector2 delta, WindowManipulationZone.ManipulationZone zone)
    {
        
        currentDragZone = zone;


        if (!dragging)
        {
            linkedDrag = true;
            //Debug.Log("Start Manipulation From Link " + currentDragZone);
            StartManipulation();
            dragging = true;
        }
        ////Debug.Log("Run Manipulation From Link" + currentDragZone);
        ManipulateWindow(delta);
    }
    
    public void OnPointerEnterFromZone(WindowManipulationZone.ManipulationZone zone)
    {
        ////Debug.Log("Cursor Enter " + zone + "Zone");
        if (!performingAction) CursorImageController.instance.currentManipulationZone = zone;
        else CursorImageController.instance.queuedManipulationZone = zone;

        CursorImageController.instance.ChangeCursorImage(ZoneCursor[CursorImageController.instance.currentManipulationZone]);
    }

    public void OnPointerExitFromZone(WindowManipulationZone.ManipulationZone zone)
    {
        CursorImageController.instance.lastManipulationZone = zone;
        IEnumerator coroutine = ValidateZoneExit();
        StartCoroutine(coroutine);
    }

    // called from pointer exit to make sure that the exit does not overwrite another pointer enter
    IEnumerator ValidateZoneExit()
    {
        // wait for next frame to give other zones a chance to set themselves
        yield return null;
        
        //check current zone against zone just exited
        if (!performingAction && CursorImageController.instance.lastManipulationZone == CursorImageController.instance.currentManipulationZone) CursorImageController.instance.currentManipulationZone = WindowManipulationZone.ManipulationZone.None;
        else if (performingAction && CursorImageController.instance.lastManipulationZone == CursorImageController.instance.queuedManipulationZone) CursorImageController.instance.queuedManipulationZone = WindowManipulationZone.ManipulationZone.None;
        
        CursorImageController.instance.ChangeCursorImage(ZoneCursor[CursorImageController.instance.currentManipulationZone]);

        yield break;
    }

    #endregion Pointer Methods

    #region Window Manipulation
    
    /// <summary>
    /// Assign pivot and vars at start of window manipulation
    /// </summary>
    void StartManipulation()
    {
        if (!linkedDrag) currentDragZone = CursorImageController.instance.currentManipulationZone;
        //set pivots and assign moving edges
        switch (currentDragZone)
        {
            case WindowManipulationZone.ManipulationZone.Movement:

                //set pivot 
                rt.SetPivot(new Vector2(0.5f, 1f));

                //set size to movement size if too big
                float width = 0;
                float height = 0;
                if (rt.sizeDelta.x > moveSize.x) width = moveSize.x;
                else width = rt.sizeDelta.x;
                if (rt.sizeDelta.y > moveSize.y) height = moveSize.y;
                else height = rt.sizeDelta.y;
                rt.sizeDelta = new Vector2(width, height);

                //set cursor to grabbing
                CursorImageController.instance.ChangeCursorImage(CursorImageController.CursorImage.HandClosed);
                onBreakLinks?.Invoke();

                //assign moving snap zones
                movingSnapZones.Add(snapZones[RectTransform.Edge.Right]);
                movingSnapZones.Add(snapZones[RectTransform.Edge.Top]);
                movingSnapZones.Add(snapZones[RectTransform.Edge.Left]);
                movingSnapZones.Add(snapZones[RectTransform.Edge.Bottom]);

                break;

            case WindowManipulationZone.ManipulationZone.Left:
                rt.SetPivot(new Vector2(1f, 0.5f));
                movingSnapZones.Add(snapZones[RectTransform.Edge.Left]);
                break;

            case WindowManipulationZone.ManipulationZone.BottomLeft:
                rt.SetPivot(new Vector2(1f, 1f));
                movingSnapZones.Add(snapZones[RectTransform.Edge.Left]);
                movingSnapZones.Add(snapZones[RectTransform.Edge.Bottom]);
                onBreakLinks?.Invoke();
                break;

            case WindowManipulationZone.ManipulationZone.TopLeft:
                rt.SetPivot(new Vector2(1f, 0f));
                movingSnapZones.Add(snapZones[RectTransform.Edge.Left]);
                movingSnapZones.Add(snapZones[RectTransform.Edge.Top]);
                onBreakLinks?.Invoke();
                break;

            case WindowManipulationZone.ManipulationZone.Right:
                rt.SetPivot(new Vector2(0f, 0.5f));
                movingSnapZones.Add(snapZones[RectTransform.Edge.Right]);
                break;

            case WindowManipulationZone.ManipulationZone.BottomRight:
                rt.SetPivot(new Vector2(0f, 1f));
                movingSnapZones.Add(snapZones[RectTransform.Edge.Right]);
                movingSnapZones.Add(snapZones[RectTransform.Edge.Bottom]);
                onBreakLinks?.Invoke();
                break;

            case WindowManipulationZone.ManipulationZone.TopRight:
                rt.SetPivot(new Vector2(0f, 0f));
                movingSnapZones.Add(snapZones[RectTransform.Edge.Right]);
                movingSnapZones.Add(snapZones[RectTransform.Edge.Top]);
                onBreakLinks?.Invoke();
                break;

            case WindowManipulationZone.ManipulationZone.Bottom:
                rt.SetPivot(new Vector2(0.5f, 1f));
                movingSnapZones.Add(snapZones[RectTransform.Edge.Bottom]);
                break;

            case WindowManipulationZone.ManipulationZone.Top:
                rt.SetPivot(new Vector2(0.5f, 0f));
                movingSnapZones.Add(snapZones[RectTransform.Edge.Top]);
                break;
        }
        if (!linkedDrag) ControlNonMovingZones(false);
    }
    
    /// <summary>
    /// Move or resize window
    /// </summary>
    /// <param name="delta"></param>
    void ManipulateWindow(Vector2 delta)
    {
        if (!linkedDrag) currentDragZone = CursorImageController.instance.currentManipulationZone;

        delta = Vector2.Scale(delta, new Vector2(1.0f / transform.lossyScale.x, 1.0f / transform.lossyScale.y));

        float width = rt.sizeDelta.x;
        float height = rt.sizeDelta.y;

        switch (currentDragZone)
        {
            case WindowManipulationZone.ManipulationZone.Movement:
                Vector2 newPosition = rt.anchoredPosition + new Vector2(delta.x, delta.y);
                rt.anchoredPosition = newPosition;
                break;

            case WindowManipulationZone.ManipulationZone.Left:
                width = AdjustedWidth(-delta);
                break;

            case WindowManipulationZone.ManipulationZone.BottomLeft:
                width = AdjustedWidth(-delta);
                height = AdjustedHeight(-delta);
                break;

            case WindowManipulationZone.ManipulationZone.TopLeft:
                width = AdjustedWidth(-delta);
                height = AdjustedHeight(delta);
                break;

            case WindowManipulationZone.ManipulationZone.Right:
                width = AdjustedWidth(delta);
                break;

            case WindowManipulationZone.ManipulationZone.BottomRight:
                width = AdjustedWidth(delta);
                height = AdjustedHeight(-delta);
                break;

            case WindowManipulationZone.ManipulationZone.TopRight:
                width = AdjustedWidth(delta);
                height = AdjustedHeight(delta);
                break;

            case WindowManipulationZone.ManipulationZone.Bottom:
                height = AdjustedHeight(-delta);
                break;

            case WindowManipulationZone.ManipulationZone.Top:
                height = AdjustedHeight(delta);
                break;
        }
        rt.sizeDelta = new Vector2(width, height);

        return;
    }
    
    /// <summary>
    /// Correct windows rectTransform after they have exceed sizing or positioning limits
    /// </summary>
    void ValidateTransform()
    {
        CorrectSize();
        CorrectPosition();
    }
   
    /// <summary>
    /// Resize window dimensions when they go below minimum size threshold
    /// </summary>
    private void CorrectSize()
    {
        // if transform is too small, set to minimum
        if (rt.rect.width < minimumSize.x)
        {
            //Debug.Log("minimum X breached");
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minimumSize.x);
        }
        if (rt.rect.height < minimumSize.y)
        {
            //Debug.Log("minimum Y breached");
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minimumSize.y);
        }
    }
    
    /// <summary>
    /// Reposition the window if the header/move point goes out of bounds
    /// </summary>
    private void CorrectPosition()
    {
        // this should return the top center position of the rect
        float distanceToTop = rt.rect.yMax;
        float distanceToHeader = distanceToTop - headerTopMargin;

        Vector2 topEdgePoint = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y + distanceToTop);

        // if transform header is out of bounds, back into bounds without changing scale
        if (topEdgePoint.x > screenBounds.x)
        {
            //Debug.Log("header out of X bounds");
            // reposition back within bounds with some margin
            rt.anchoredPosition = new Vector2(screenBounds.x - correctionMargin, rt.anchoredPosition.y);
        }
        else if (topEdgePoint.x < -screenBounds.x)
        {
            //Debug.Log("header out of -X bounds");
            rt.anchoredPosition = new Vector2(-screenBounds.x + correctionMargin, rt.anchoredPosition.y);
        }

        if (topEdgePoint.y > screenBounds.y)
        {
            //Debug.Log("header out of y bounds");
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, screenBounds.y - correctionMargin - distanceToHeader);
        }
        else if (topEdgePoint.y < -screenBounds.y)
        {
            //Debug.Log("header out of -y bounds");
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -screenBounds.y + correctionMargin - distanceToHeader);
        }
    }
 
    float AdjustedWidth(Vector2 delta)
    {
        float nextSize = rt.sizeDelta.x + delta.x;
        if (nextSize < minimumSize.x) hitLimitX = true;
        else hitLimitX = false;
        return Mathf.Clamp(nextSize, minimumSize.x, Mathf.Infinity);
    }
    
    float AdjustedHeight(Vector2 delta)
    {
        float nextSize = rt.sizeDelta.y + delta.y;
        if (nextSize < minimumSize.y) hitLimitY = true;
        else hitLimitY = false;

        return Mathf.Clamp(nextSize, minimumSize.y, Mathf.Infinity);
    }
    
    /// <summary>
    /// control the colliders of edges not being manipulated to prevent interference.
    /// </summary>
    void ControlNonMovingZones(bool enabled)
    {
        foreach (WindowSnappingZone w in snapZones.Values)
        {
            w.enabled = (movingSnapZones.Contains(w) || (enabled && !w.linked));
        }
    }
    
    #endregion Window Manipulation

    #region Window Snapping

    /// <summary>
    /// Prime snap zone for snapping
    /// </summary>
    /// <param name="snappingZone"></param>
    public void PrimeZone(WindowSnappingZone snappingZone)
    {
        // prevent multiple
        if (primedZone!= null) return;
        
        primedZone = snappingZone;
        primedEdge = primedZone.edge;

        ControlNonPrimedZones(false);
    }
    
    /// <summary>
    /// Clear out primed zone vars
    /// </summary>
    /// <param name="zone"></param>
    public void UnprimeZone(WindowSnappingZone zone)
    {
        if (primedZone != zone) return;
        primedZone = null;
        ControlNonPrimedZones(true);
        ResetSnapPreview();
        ClearTargetSnap();
    }

    /// <summary>
    /// Recieve Target info from primed snap zone
    /// </summary>
    /// <param name="targetRt"></param>
    /// <param name="targetWindow"></param>
    /// <param name="targetSnapZone"></param>
    /// <param name="targetSnapZoneRt"></param>
    public void SetTargetSnapInfo(RectTransform _targetRt, ResizableWindow _targetWindow,  WindowManipulationZone _targetDragZone, WindowSnappingZone _targetSnapZone, RectTransform _targetSnapZoneRt)
    {
        targetRt = _targetRt;
        targetWindow = _targetWindow;
        targetDragZone = _targetDragZone;
        targetSnapZone = _targetSnapZone;
        targetSnapZoneRt = _targetSnapZoneRt;
    }

    /// <summary>
    /// Set Snap Type based on distance to edge
    /// </summary>
    void SetSnapType()
    {
        if (hasResizableLink) resizeOnSnap = false;

        else
        {
            // calculate snap distance
            switch (primedEdge)
            {
                case RectTransform.Edge.Right:
                    targetSnapDistance = targetSnapZoneRt.position.x - primedZone.rt.position.x;
                    break;
                case RectTransform.Edge.Left:
                    targetSnapDistance = primedZone.rt.position.x - targetSnapZoneRt.position.x;
                    break;
                case RectTransform.Edge.Top:
                    targetSnapDistance = targetSnapZoneRt.position.y - primedZone.rt.position.y;
                    break;
                case RectTransform.Edge.Bottom:
                    targetSnapDistance = primedZone.rt.position.y - targetSnapZoneRt.position.y;
                    break;

            }

            // set whether we resize on snap based on distance
            resizeOnSnap = (targetSnapDistance < resizeOnSnapDistance);
        }
    }

    /// <summary>
    /// Set preview based on snap type
    /// </summary>
    public void ControlSnapPreview()
    {
        // preview should match position and size of snap
        if (resizeOnSnap)
        {
            snapPreviewRt.position = targetSnapZoneRt.position;
            snapPreviewRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSnapZoneRt.rect.size.x);
            snapPreviewRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSnapZoneRt.rect.size.y);
        }
        else
        {
            //snapPreviewRt.sizeDelta = _originalSnapPreviewSize;
            snapPreviewRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, primedZone.rt.rect.size.x);
            snapPreviewRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, primedZone.rt.rect.size.y);
            if (primedEdge == RectTransform.Edge.Right || primedEdge == RectTransform.Edge.Left) snapPreviewRt.position = new Vector2(targetSnapZoneRt.position.x, primedZone.rt.position.y);
            else snapPreviewRt.position = new Vector2(primedZone.rt.position.x, targetSnapZoneRt.position.y);
        }

        // turn on if its off
        //if (!snapPreview.activeSelf)  snapPreview.SetActive(true);
        snapPreviewImage.enabled = true;
    }
    
    void ResetSnapPreview()
    {
        //snapPreview.SetActive(false);
        snapPreviewImage.enabled = false;
    }

    /// <summary>
    /// Control the colliders of non primed edges to prevent interference
    /// </summary>
    void ControlNonPrimedZones(bool enabled)
    {
        foreach (WindowSnappingZone w in movingSnapZones)
        {
            w.enabled = (w == primedZone || (enabled && !w.linked));
        }
    }

    /// <summary>
    /// Snap and/or Resize the window
    /// </summary>
    public void SnapWindow()
    {
        if (targetRt)
        {
            // if edges are overlaping, resize
            if (resizeOnSnap)
            {
                primedEdge.SnapAndResizeWindow(rt, targetRt);
            }

            //otherwise only snap edge to edge
            else primedEdge.SetEdgeToOppositeEdgeInCanvasSpace(rt, targetRt);

            //create object to managing edge link
            if (targetWindow != null)
            {
                var windowLinkObject = Instantiate(windowLinkPrefab, rt.parent);
                var windowLinkScript = windowLinkObject.GetComponent<WindowLink>();

                windowLinkScript.manipulationZone_A = primedZone.windowDragZone;
                windowLinkScript.manipulationZone_B = targetDragZone;
                windowLinkScript.Initialize();
            }
        }

        UnprimeZone(primedZone);
    }

    void ClearTargetSnap()
    {
        // clean up
        targetRt = null;
        targetWindow = null;
        targetSnapZone = null;
        targetSnapZoneRt = null;
        targetSnapDistance = 0;
    }
    
    #endregion Window Snapping

    private void SetScreenBounds()
    {
        //Debug.Log("set screen bounds");
        screenBounds = canvasRt.rect.size/2;
    }
    
    void CancelAction()
    {
       // //Debug.Log("Cancel Action");
        performingAction = false;
        dragging = false;
        hitLimitX = false;
        hitLimitY = false;
    }
    
    public void ResizeSnapZoneCollision()
    {
        // we need this for viewport in order initialize correctly since it changes To Do- remove the need to resize viewport
        foreach (WindowSnappingZone s in snapZones.Values)
        {
            s.ResizeCollision();
        }
    }

    public bool Limited()
    {
        return (hitLimitX || hitLimitY);
    }
}
