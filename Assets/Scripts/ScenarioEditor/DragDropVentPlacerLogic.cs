using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.EventSystems;
using NIOSH_EditorLayers;
using static NIOSH_EditorLayers.LayerManager;
using NIOSH_MineCreation;
/// <summary>
/// A placerlogic that handles using the mouse to move placable assets 
/// as well as keyboard commands. No gizmos used.
/// </summary>

[System.Serializable]
public class DragDropVentPlacerLogic : PlacerLogic
{
    // Raycast references
    private Ray ray;
    private RaycastHit hit;

    bool dragging = false;
    PlacablePrefab hitPrefab;
    PlacerGizmo currentGizmo;
    VentilationLayerNode targetedNode;
    Vector3 previousMousePos;
    float previousHitDistance;
    Vector3 previousMouseInput;
    Vector3 previousGizmoPos;

    bool _staticGasZone;
    Renderer lastRenderer;
    //[SerializeField] float _gasZoneHeightOffset = 1.5f; //To Do: should be calculated from object Y scale after it is scalled to hallway

    const float gasZoneHeightMultiplier = 1f;
    private ComponentInfo_StaticGasZone _lastSelectedGasZone;
    private Vector3 _lastGasZoneSize;
        
    public DragDropVentPlacerLogic(List<GizmoOrder> kinds, Placer placer, CameraManager camManager) : base(kinds, placer, camManager)
    {

    }

    public override void StartLogic()
    {
        //float seamHeight = MineLayerTile.Settings.seamHeight;
        //_gasZoneHeightOffset = (seamHeight /2 ) * gasZoneHeightMultiplier;
        
    }
    /// <summary>
    /// Called by the placer every frame. Checks for mouse and keyboard input.
    /// </summary>
    public override void DoLogic() // TODO would love to use the new input system
    {
        CheckForGizmoInput();
        //if(currentGizmo == null)CheckForMouseInput();
        //CheckForMouseInput();
        //CheckForKeyboardInput();
        //CheckForDeselect(); 
    }

    public override void DoSupportLogic()
    {
        CheckForFocus();
        CheckForDelete();
        CheckForDeselect();

        if (_lastSelectedGasZone != null)
        {
            if (_lastSelectedGasZone.Size != _lastGasZoneSize)
            {
                RepositionGizmos();
            }
        }
    }

    /// <summary>
    /// Forces a selected object, skipping the normal raycast select.
    /// For example if the UI wants to select an object.
    ///// </summary>
    ///// <param name="obj"></param>
    //public override void ForceSelectObject(GameObject obj)
    //{
    //    PlacablePrefab prefab = obj.GetComponent<PlacablePrefab>();
    //    if (prefab != null)
    //    {
    //        if (!prefab.gameObject.activeSelf) { Debug.Log("PREFAB NOT ACTIVE"); prefab.gameObject.SetActive(true); }
    //        if (prefab.GetComponent<PlacablePrefab>().GetPlacementLayer() != EditorLayer.Ventilation)
    //            return;

    //        SelectObject(prefab);

    //        if (prefab.name.StartsWith("VentLayerNodeConnection"))
    //        {
    //            dragging = true;
    //        }
    //    }
    //}

    ///// <summary>
    ///// Forces the current selected object to be deselected. 
    ///// For example if UI input other than from mouse or keyboard merits deselection.
    ///// </summary>
    ///// <param name="obj"></param>
    //public override void ForceDeselectObject(GameObject obj)
    //{
    //    if (selectedObject != null && selectedObject.gameObject == obj)
    //    {
    //        DeselectObject();
    //    }
    //}

    void CheckForGizmoInput()
    {
        if (currentGizmo != null)
        {
            if (Input.GetMouseButton(0))
            {
                switch (currentGizmo.GetGizmoType())
                {
                    case GizmoKind.Resize:
                        GizmoDrag(currentGizmo);
                        RepositionZone();
                        RepositionGizmos();
                        break;
                }
                
            }

            if (Input.GetMouseButtonUp(0))
            {
                RepositionGizmos();
                currentGizmo.gameObject.GetComponent<Renderer>().material.color = scenePlacer.gizmoColor;
                currentGizmo = null;
            }
        }

        
    }

    /*
    /// <summary>
    /// Intended to run each frame, checks for current left mouse button status
    /// as well as whether an object is selected and something is currently being dragged.
    /// </summary>
    void CheckForMouseInput()
    {
        //Attempt to ignore mouse input on this function if we're not in vent layer for vent placer logic.
        if(LayerManager.GetCurrentLayer() != EditorLayer.Ventilation)
        {
            return;
        }
        // End drag if we release the mouse
        if (selectedObject != null && dragging && Input.GetMouseButtonUp(0))
        {
            dragging = false;
            bool isInitPlace = !selectedObject.GetPlacedOnce();
            selectedObject.SetPlaced();

            VentilationNodeConnection connection;
            
            if (selectedObject.gameObject.TryGetComponent(out connection))
            {
                if (targetedNode)
                {
                    connection.transform.SetParent(targetedNode.transform);
                    connection.AttachToNode(targetedNode.GetComponent<VentilationLayerNode>());
                    ForceSelectObject(Object.Instantiate(selectedObject.gameObject));
                    dragging = true;

                    connection.MakeConnection(selectedObject.gameObject);
                }
                else
                {
                    GameObject obj = selectedObject.gameObject;
                    DeselectObject();
                    dragging = false;
                    Object.Destroy(obj);
                }
            }
            else if (selectedObject.gameObject.name.StartsWith("VentilationLayerNode") && isInitPlace)
            {
                
                ForceSelectObject(Object.Instantiate(selectedObject.gameObject));
                dragging = true;
            }

        }

        // If we are dragging something run drag logic
        if (dragging && selectedObject != null)
        {
            if (currentGizmo != null) {  }
          
            else if (_staticGasZone){ MouseDrag(selectedObject); RepositionGizmos(); }
         
            else { MouseDrag(selectedObject); }
            

        }

        // Check to see if we've clicked on something new when we click the mouse
        if (Input.GetMouseButtonDown(0) && !dragging)
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Gizmo")))
            {
                if (selectedObject == null || hit.collider.GetInstanceID() != selectedObject.GetInstanceID())
                {
                    //hitPrefab = hit.collider.gameObject.GetComponent<PlacablePrefab>();
                    currentGizmo = hit.collider.gameObject.GetComponent<PlacerGizmo>();
                    previousMousePos = hit.point;
                    previousHitDistance = hit.distance;


                    if (hit.collider.TryGetComponent(out Renderer renderer))
                    {
                        renderer.material.color = scenePlacer.selectedColor;
                        //Debug.Log("Hit Gizmo " + hit.collider.gameObject.name);

                    }


                }
            }
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("VentVisualization")))
            {

                if (selectedObject == null || hit.collider.GetInstanceID() != selectedObject.GetInstanceID())
                {
                    //Debug.Log("New Selection : " + hit.collider.gameObject.name);
                    hitPrefab = hit.collider.gameObject.GetComponent<PlacablePrefab>();

                    if (hitPrefab != null && hitPrefab.GetPlacementLayer() == EditorLayer.Ventilation)
                    {
                        //Debug.Log("New Vent");
                        SelectObject(hitPrefab);
                        CheckForPlacementOverride(hitPrefab);
                        
                        return;
                    }
                }
                // FIXME don't like how this logic tree is set up
                if (selectedObject != null && hit.collider.GetInstanceID() != selectedObject.GetInstanceID())
                {
                    DeselectObject();
                }
            }
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Default")))
            {
                if (selectedObject == null || hit.collider.GetInstanceID() != selectedObject.GetInstanceID())
                {
                    Debug.Log("New Selection : " + hit.collider.gameObject.name);
                    hitPrefab = hit.collider.gameObject.GetComponent<PlacablePrefab>();

                    if (hitPrefab != null && hitPrefab.GetPlacementLayer() == EditorLayer.Ventilation)
                    {
                        Debug.Log("Select stopping placer");
                        //ForceDeselectObject();
                        SelectObject(hitPrefab);
                        CheckForPlacementOverride(hitPrefab);
                        
                        return;
                    }
                }
                // FIXME don't like how this logic tree is set up
                if (selectedObject != null && hit.collider.GetInstanceID() != selectedObject.GetInstanceID())
                {
                    DeselectObject();
                }


            }
            else
            {
                DeselectObject();
                dragging = false;
            }
        }

        else
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Gizmo")))
            {
                if (selectedObject == null || hit.collider.GetInstanceID() != selectedObject.GetInstanceID())
                {
                    var nextGizmo = hit.collider.gameObject.GetComponent<PlacerGizmo>();
                    if (nextGizmo != null && nextGizmo.transform.TryGetComponent(out lastRenderer))
                    {
                        lastRenderer.material.color = scenePlacer.hoveredColor;
                        //Debug.Log("Hit Gizmo " + hit.collider.gameObject.name);
                    }
                }
            }
            else if (lastRenderer != null)
            {
                lastRenderer.material.color = scenePlacer.gizmoColor;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && dragging)
        {
            GameObject obj = selectedObject.gameObject;
            DeselectObject();
            dragging = false;
            Object.Destroy(obj);
        }

        // If we hold the mouse down and are not already dragging turn on dragging
        if (!dragging && Input.GetMouseButton(0))
        {
            if (selectedObject)
            {
                dragging = true;
                selectedObject.UnPlace();
            }
        }

    }

    */

    ///// <summary>
    ///// Run required logic for selecting a placeable object. Layer is changed to allow 
    ///// drag raycast to see past it more easily.
    ///// </summary>
    ///// <param name="obj"></param>
    //void SelectObject(PlacablePrefab obj)
    //{

       
    //    /*if (obj.CheckIfPlaced())
    //    {
    //        scenePlacer.SwitchPlacerLogic(GizmoKind.Pan, obj.gameObject);
    //    }*/

    //    if (selectedObject != null)
    //    {
    //        DeselectObject();
    //    }
    //   // Debug.Log("SelectVent");

    //    selectedObject = obj;
    //    selectedObjCollider = selectedObject.gameObject.GetComponent<Collider>();
    //    obj.gameObject.layer = LayerMask.NameToLayer("SelectedObject");
        
    //    if (selectedObject.TryGetComponent(out StaticVentilationZoneStratified gasZone))
    //    {
    //        _staticGasZone = true;
    //        scenePlacer.ActivateResizeGizmos();
    //    }
    //    else { _staticGasZone = false; scenePlacer.DeactivateResizeGizmos(); }
    //    if (scenePlacer) scenePlacer.onObjectSelected?.Invoke(obj.gameObject);

    //    //if (_lastSelectedGasZone != null)
    //    //{
    //    //    _lastSelectedGasZone.SizeChanged -= OnGasZoneSizeChanged;
    //    //    _lastSelectedGasZone = null;
    //    //}

    //    selectedObject.TryGetComponent(out _lastSelectedGasZone);
    //    _lastGasZoneSize = Vector3.zero;

    //    //if (_lastSelectedGasZone != null)
    //    //{
    //    //    _lastSelectedGasZone.SizeChanged += OnGasZoneSizeChanged;
    //    //}

    //    RepositionGizmos();
        
    //}

    //private void OnGasZoneSizeChanged()
    //{

    //}

    /// <summary>
    /// Deselects any currently selected placable object
    /// </summary>
    //void DeselectObject()
    //{
    //    if (selectedObject != null)
    //    {
    //        selectedObject.gameObject.layer = LayerMask.NameToLayer("VentVisualization");
    //        selectedObjCollider = null;
    //        selectedObject = null;
    //    }

    //}

    /// <summary>
    /// Drags the currently selected placable object based on raycast hit location and normals.
    /// </summary>
    /// <param name="target"></param>
    void MouseDrag(PlacablePrefab target)
    {
        if(target == null) { return; }
        target.gameObject.SetActive(true);
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("PanPlane", "Floor", "VentVisualization")))
        {
            if (selectedObject.gameObject.name.StartsWith("VentilationLayerNode"))
            {
                target.transform.position = new Vector3(hit.point.x, 0, hit.point.z);
            }
            else if (selectedObject.gameObject.name.StartsWith("Static Gas Zone"))
            {
                target.transform.position = new Vector3(hit.point.x, 0, hit.point.z);
                //Debug.Log("Drag Gas Zone at : " + 0);
            }
            else
            {
                if (hit.collider.gameObject.TryGetComponent(out targetedNode))
                {
                    target.transform.position = new Vector3(targetedNode.transform.position.x,
                                                            targetedNode.transform.position.y + .5f,
                                                            targetedNode.transform.position.z);
                }
                else
                {
                    target.transform.position = hit.point;
                    targetedNode = null;
                }
            }
        }
    }

    void GizmoDrag(PlacerGizmo gizmo)
    {
        if (gizmo == null) { return; }
        gizmo.gameObject.SetActive(true);
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Vector3 mousePos = Vector3.zero;
        Vector3 mouseDelta = Vector3.zero;
        Vector3 mosInput = Input.mousePosition;

        

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        mousePos = ray.GetPoint(previousHitDistance);
        mouseDelta = mousePos - previousMousePos;



        Vector3 newPos = Vector3.zero;
        Vector3 oldPos = gizmo.transform.position;
        switch (gizmo.GetAxis())
        {
            case GizmoAxis.X:
                newPos = gizmo.transform.position + mouseDelta;
                gizmo.transform.position = new Vector3(newPos.x, oldPos.y, oldPos.z);
                break;

            case GizmoAxis.Z:
                newPos = gizmo.transform.position + mouseDelta;
                gizmo.transform.position = new Vector3(oldPos.x, oldPos.y, newPos.z);
                break;
        }
        previousMousePos = mousePos;
        
    }

    /// <summary>
    /// Check if the object to place can be placed on the layer the placer object is on
    /// </summary>
    /// <param name="placementLayer">The layer the placer object is on</param>
    private bool CheckValidPlacement(string placementLayer)
    {
        return true;
    }

    /// <summary>
    /// Intended to run each frame to check keyboard inputs for different functionalities.
    /// </summary>
    //void CheckForKeyboardInput()    // TODO would really love to use new input system try to figure out error that happened last time
    //{
    //    if (selectedObject != null)
    //    {
    //        RotateObject();
    //        //CheckForDelete();
    //    }

    //}

    /// <summary>
    /// Rotates selected object around its local Y axis
    /// </summary>
    //void RotateObject()
    //{
    //    if (!dragging)
    //        return;

    //    //Vector3 eulers = Vector3.zero;
    //    if (Input.GetKeyDown(KeyCode.Q))
    //    {
    //        //eulers += new Vector3(0, 1, 0) * 90;
    //        SelectObject(selectedObject.GetComponent<RotatorSetAccess>().GetRotateToObject(false).GetComponent<PlacablePrefab>());
    //    }
    //    if (Input.GetKeyDown(KeyCode.E))
    //    {
    //        //eulers += new Vector3(0, -1, 0) * 90;
    //        SelectObject(selectedObject.GetComponent<RotatorSetAccess>().GetRotateToObject(true).GetComponent<PlacablePrefab>());
    //    }
    //    //selectedObject.transform.Rotate(eulers);

    //}

    ///// <summary>
    ///// Checks if the delete key is pressed while an object is selected, deleting the object.
    ///// </summary>
    //public override void CheckForDelete()
    //{
    //    if (Input.GetKey(KeyCode.Delete))
    //    {
    //        if (selectedObject != null)
    //        {
    //            GameObject temp = selectedObject.gameObject;
    //            DeselectObject();
    //            GameObject.Destroy(temp);
    //        }
    //    }
    //}
    //public override void CheckForDeselect()
    //{
    //    if (Input.GetKey(KeyCode.Escape))
    //    {
    //        DeselectObject();
    //    }
    //}

    public override void CheckForFocus()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (selectedObject != null)
            {
                camManager.GetActiveCameraLogic().FocusObject(selectedObject.gameObject);
            }
        }
    }
    
    void RepositionGizmos()
    {
        if (_lastSelectedGasZone == null) return;

        _lastGasZoneSize = _lastSelectedGasZone.Size;

        var zonePos = _lastSelectedGasZone.transform.position;
        var extent = _lastSelectedGasZone.Size / 2.0f;

        scenePlacer.resizeGizmos[0].transform.position = new Vector3(zonePos.x + extent.x, zonePos.y, zonePos.z);
        scenePlacer.resizeGizmos[1].transform.position = new Vector3(zonePos.x - extent.x, zonePos.y, zonePos.z);

        scenePlacer.resizeGizmos[2].transform.position = new Vector3(zonePos.x, zonePos.y, zonePos.z + extent.z);
        scenePlacer.resizeGizmos[3].transform.position = new Vector3(zonePos.x, zonePos.y, zonePos.z - extent.z);
    }

    void RepositionZone()
    {
        if (_lastSelectedGasZone == null) return;
        
        float centroidX = (scenePlacer.resizeGizmos[0].transform.position.x + scenePlacer.resizeGizmos[1].transform.position.x) /2;

        float centroidZ = (scenePlacer.resizeGizmos[2].transform.position.z + scenePlacer.resizeGizmos[3].transform.position.z) / 2;
        Vector3 centroid = new Vector3(centroidX, _lastSelectedGasZone.transform.position.y, centroidZ);


        _lastSelectedGasZone.transform.position = centroid;

        //scale zone to distance between placer gizmos
        float xDist = Vector3.Distance(scenePlacer.resizeGizmos[0].transform.position, scenePlacer.resizeGizmos[1].transform.position);
        float zDist = Vector3.Distance(scenePlacer.resizeGizmos[2].transform.position, scenePlacer.resizeGizmos[3].transform.position);
        //_lastSelectedGasZone.transform.localScale = new Vector3(xDist, _lastSelectedGasZone.transform.localScale.y, zDist);
        _lastSelectedGasZone.Size = new Vector3(xDist, _lastSelectedGasZone.Size.y, zDist);

    }



}
