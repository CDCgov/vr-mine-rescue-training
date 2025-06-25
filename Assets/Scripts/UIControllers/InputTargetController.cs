using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using NIOSH_EditorLayers;
using static NIOSH_EditorLayers.LayerManager;
using NIOSH_MineCreation;

public class InputTargetController : MonoBehaviour
{
    public enum InputTarget
    {
        Viewport,
        Hierarchy,
        Library,
        Inspector,
        Toolbar,
        OtherUI,
        PartialViewport,// allows movement of camera, but not placer manipulation
    }

    InputTarget inputTarget;
    InputTarget nextInputTarget;
    InputTarget lastInputTarget;

    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    GameObject hierarchy;
    GameObject inspector;
    GameObject library;
    GameObject viewport;
    GameObject toolbar;

    private List<RaycastResult> _raycastResults = new List<RaycastResult>();

    public bool ManipulatingWindow;


    public delegate void OnNewInputTarget(InputTarget inputTarget);
    public OnNewInputTarget onNewInputTarget;
    EventSystem eventSystem;
    Placer placer;


    private Camera sceneCamera;

    private void Awake()
    {
        m_Raycaster = FindObjectOfType<GraphicRaycaster>();
        m_EventSystem = FindObjectOfType<EventSystem>();

        hierarchy = GameObject.Find("HierarchyWindow");
        inspector = GameObject.Find("InspectorWindow");
        library = GameObject.Find("LibraryWindow");
        viewport = GameObject.Find("ViewportWindow");
        toolbar = GameObject.Find("ToolBar");
        placer = GameObject.Find("Placer").GetComponent<Placer>();
        eventSystem = FindObjectOfType<EventSystem>();
        sceneCamera = Camera.main;
    }
    /// <summary>
    /// Attempting to set the default input target to the viewport upon starting the editor mode
    /// </summary>
    private void Start()
    {
        
        nextInputTarget = InputTarget.Viewport;
        ChangeInputTarget();
    }

    public void SetInputTargetToViewPort()
    {
        nextInputTarget = InputTarget.Viewport;
        ChangeInputTarget();
    }

    private void Update()
    {
        RaycastCheck();// To Do: move to coroutine for better performance

        if (nextInputTarget != inputTarget)
        {
            if ((nextInputTarget == InputTarget.Viewport || nextInputTarget == InputTarget.PartialViewport) && (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))) // if hovering viewport, switch on mouse click events
            {
                ChangeInputTarget();
            }
            else if  (nextInputTarget != InputTarget.Viewport && nextInputTarget != InputTarget.PartialViewport) //if hovering UI, switch when eventSystem has selected object
            {
                ChangeInputTarget();
            }

        }

        
    }

    void ChangeInputTarget()
    {
        lastInputTarget = inputTarget;
        inputTarget = nextInputTarget;

        onNewInputTarget?.Invoke(inputTarget);
        ControlUITarget();
        ///Debug.Log("New Input Target : " + inputTarget);
    }
    
    void RaycastCheck()
    {
        if (m_PointerEventData == null)
            m_PointerEventData = new PointerEventData(m_EventSystem);

        m_PointerEventData.position = Input.mousePosition;

        //List<RaycastResult> results = new List<RaycastResult>();
        _raycastResults.Clear();
        m_Raycaster.Raycast(m_PointerEventData, _raycastResults);

        //Automatically set to viewport if no GUI detected
        if (_raycastResults.Count <= 0)
        {
            Vector2 mousePos = sceneCamera.ScreenToViewportPoint(Input.mousePosition);
           
            if (mousePos.x < 0 || mousePos.y < 0 || mousePos.x > 1 || mousePos.y > 1 )
            {
               if(nextInputTarget != InputTarget.PartialViewport) nextInputTarget = InputTarget.PartialViewport;
            }
            else if (nextInputTarget != InputTarget.Viewport)
            {
                nextInputTarget = InputTarget.Viewport;
            }
        }
        else if(eventSystem.currentSelectedGameObject != null)
        {
            foreach (RaycastResult r in _raycastResults)
            {
                Transform t = r.gameObject.transform;
                
                if (t.IsChildOf(hierarchy.transform)) nextInputTarget = InputTarget.Hierarchy;
                else if (t.IsChildOf(inspector.transform)) nextInputTarget = InputTarget.Inspector;
                else if (t.IsChildOf(library.transform)) nextInputTarget = InputTarget.Library;
                else if (t.IsChildOf(toolbar.transform)) nextInputTarget = InputTarget.Toolbar;
                else if (t.IsChildOf(viewport.transform)) nextInputTarget = InputTarget.Viewport;
                else nextInputTarget = InputTarget.OtherUI;
                    
            }
        }
        else
        {
            nextInputTarget = InputTarget.PartialViewport;
        }
    }
    

    
    void ControlUITarget()
    {
        if (inputTarget == InputTarget.Viewport) 
        { 
            eventSystem.sendNavigationEvents = false; 
            eventSystem.SetSelectedGameObject(null);
            if (ManipulatingWindow) 
            {
                nextInputTarget = InputTarget.PartialViewport;
                ChangeInputTarget();
            }
        }
        else if (inputTarget == InputTarget.PartialViewport)
        { 
            eventSystem.sendNavigationEvents = false;
        }
        else 
        {  
            eventSystem.sendNavigationEvents = true;
        }
    }
}
