using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class WindowSnappingZone : MonoBehaviour
{

    public RectTransform.Edge edge;
    [HideInInspector] public RectTransform rt;
    
    private ResizableWindow parentWindow;
    [HideInInspector] public RectTransform parentRt;
    [HideInInspector] public BoxCollider2D boxCollider;
    [SerializeField] float colliderWidth = 60;
    private Collider2D targetCollider;
    public WindowManipulationZone windowDragZone;
    public bool linked;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        parentWindow = GetComponentInParent<ResizableWindow>();
        if (parentWindow)
        {
            parentRt = parentWindow.gameObject.GetComponent<RectTransform>();
            parentWindow.snapZones.Add(edge,this);
            parentWindow.onBreakLinks.AddListener(BreakLinks);
        }
        // if no parent window, parent RT must be assigned in inspector

        boxCollider = GetComponent<BoxCollider2D>();
       
    }

    private void Start()
    {

        if(boxCollider)ResizeCollision();

    }
    private void OnDestroy()
    {
        if (parentWindow) parentWindow.onBreakLinks.RemoveListener(BreakLinks);

    }
    private void OnDisable()
    {
        boxCollider.enabled = false;
    }

    private void OnEnable()
    {
        boxCollider.enabled = true;
    }

    private void Update()
    {
        if (parentWindow)
        {
            if (parentWindow.performingAction )
            {
                ResizeCollision();
            }
        }
        if (linked) this.enabled = false;
    }

    #region Colliders

    private void OnTriggerEnter2D(Collider2D other)
    {
        //if (blocked || parentWindow == null || targetCollider) return;
        if (parentWindow == null || targetCollider) return;

        //  only window performing the action should do things - prevents duplicate actions
        if (parentWindow.performingAction) 
        {
            //prime zone for snapping
            if (parentWindow.primedZone == null)
            {
                targetCollider = other;

                //send target info to parent
                if(targetCollider.gameObject.TryGetComponent(out WindowSnappingZone _targetSnapZone))
                {
 
                    var _targetDragZone = _targetSnapZone.windowDragZone;
                    var _targetSnapZoneEdge = _targetSnapZone.edge;
                    var _targetSnapZoneRt = _targetSnapZone.rt;
                    var _targetWindow = _targetSnapZone.parentWindow;//targetCollider.gameObject.GetComponentInParent<ResizableWindow>();
                    var _targetRt = _targetSnapZone.parentRt;
                    // validate edge compatability before sending info and priming for snap
                    if (_targetSnapZoneEdge == RectTransformExtensions.GetOppositeEdge(edge))
                    {
                        //parent improvements
                        parentWindow.SetTargetSnapInfo(_targetRt, _targetWindow, _targetDragZone, _targetSnapZone, _targetSnapZoneRt);
                        parentWindow.PrimeZone(this);
                    }
                    else
                    {
                        // null to allow valid targets to take affect
                        targetCollider = null;
                    }
                }
                
                else
                {
                    // null to allow valid targets to take affect
                    targetCollider = null;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {       
        if(other = targetCollider)
        {
            if (parentWindow)
            {

                if (parentWindow.performingAction)
                {
                    parentWindow.targetRt = null;
                    parentWindow.UnprimeZone(this);
                }
            }
            targetCollider = null;
        }
    }

    public void ResizeCollision()
    {
        if (edge == RectTransform.Edge.Right || edge == RectTransform.Edge.Left) boxCollider.size = new Vector2(colliderWidth, parentRt.rect.height);
        else boxCollider.size = new Vector2(parentRt.rect.width, colliderWidth);
       // Debug.Log("ResizeCollsion");
    }

    #endregion Colliders
   
    void BreakLinks()
    {
        // remove snapping zone handle and renable for snapping
    }


}
