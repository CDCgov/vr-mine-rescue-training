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
public class DragDropPlacerLogic : PlacerLogic
{
    // Raycast references
    private Ray ray;
    private RaycastHit hit;

    float panSensitivity = 0.01f;
    float maxScaleSize = 8f;
    float minScaleSize = 0.2f;
    float rotationSensitivity = .5f;
    private Vector3 dragOffset;
    bool dragging = false;
    PlacablePrefab hitPrefab;

    private Vector3 _cachedMousePos = Vector3.negativeInfinity;

    public DragDropPlacerLogic(List<GizmoOrder> kinds, Placer placer, CameraManager camManager) : base(kinds, placer, camManager)
    {
    }

    /// <summary>
    /// Constructor that changes the default parameters for control sensitivities and limits.
    /// </summary>
    /// <param name="kinds"></param>
    /// <param name="placer"></param>
    /// <param name="pan"></param>
    /// <param name="maxScale"></param>
    /// <param name="minScale"></param>
    /// <param name="rot"></param>
    public DragDropPlacerLogic(List<GizmoOrder> kinds, Placer placer, CameraManager camManager, float pan, float maxScale, float minScale,float rot) : base(kinds, placer, camManager)
    {
        panSensitivity = pan;
        maxScaleSize = maxScale;
        minScaleSize = minScale;
        rotationSensitivity = rot;
    }

    /// <summary>
    /// Called by the placer every frame. Checks for mouse and keyboard input.
    /// </summary>
    public override void DoLogic() // TODO would love to use the new input system
    {
        //CheckForMouseInput();
        //CheckForKeyboardInput();
        CheckForNearbyMate();
        //CheckForDeselect();
    }

    /// <summary>
    /// Forces a selected object, skipping the normal raycast select.
    /// For example if the UI wants to select an object.
    /// </summary>
    /// <param name="obj"></param>
    //public override void ForceSelectObject(GameObject obj)
    //{
    //    if (!obj) return;
    //    PlacablePrefab prefab = obj.GetComponent<PlacablePrefab>();

    //    if (prefab != null && prefab.GetPlacementLayer() == LayerManager.GetCurrentLayer())
    //    {
    //        if(!prefab.gameObject.activeSelf) { Debug.Log("PREFAB NOT ACTIVE"); prefab.gameObject.SetActive(true); }
    //        SelectObject(prefab);
    //    }
    //}

    /// <summary>
    /// Forces the current selected object to be deselected. 
    /// For example if UI input other than from mouse or keyboard merits deselection.
    /// </summary>
    /// <param name="obj"></param>
    //public override void ForceDeselectObject(GameObject obj)
    //{
    //    if(selectedObject != null && selectedObject.gameObject == obj)
    //    {
    //        DeselectObject();
    //    }
    //}

    //public override void ForceDeselect()
    //{
    //    DeselectObject();
    //}

    ///// <summary>
    ///// Intended to run each frame, checks for current left mouse button status
    ///// as well as whether an object is selected and something is currently being dragged.
    ///// </summary>
    //void CheckForMouseInput()
    //{
    //    // End drag if we release the mouse
    //    if(dragging && Input.GetMouseButtonUp(0))
    //    {
    //        isActive = false;
    //        dragging = false;
    //    }

    //    // If we are dragging something run drag logic
    //    // using hit prefab rather than selected to prevent sudden teleporting of selected objects (user must drag directly from object itself)
    //    if (dragging && hitPrefab != null)
    //    {
    //        if(isActive == false) { isActive = true; }
    //        MouseDrag(hitPrefab);
    //    }

    //    // Check to see if we've clicked on something new when we click the mouse
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //        if (Physics.Raycast(ray, out hit, Mathf.Infinity, scenePlacer.objectLayers))

    //        {
    //            if (selectedObject == null || hit.transform.GetInstanceID() != selectedObject.GetInstanceID() )
    //            {
    //                hitPrefab = hit.collider.gameObject.GetComponent<PlacablePrefab>();
    //                if (hitPrefab != null && hitPrefab.GetPlacementLayer() == LayerManager.GetCurrentLayer())
    //                {
    //                    SelectObject(hitPrefab);
    //                    return;
    //                }
    //                else if (hit.transform.parent != null)
    //                {
    //                    hitPrefab = hit.transform.parent.gameObject.GetComponent<PlacablePrefab>();
    //                    if (hitPrefab != null && hitPrefab.GetPlacementLayer() == LayerManager.GetCurrentLayer())
    //                    {
    //                        SelectObject(hitPrefab);
    //                    }
    //                }
    //            }
    //        }
    //        else
    //        {
    //            DeselectObject();
    //        }

    //    }
    //    /*
    //    if(Input.GetMouseButtonDown(1))
    //    {
    //        DeselectObject();
    //    }
    //    */
    //    if (Input.GetMouseButtonUp(0))
    //    {
    //        hitPrefab = null;
    //    }

    //    // If we hold the mouse down and are not already dragging turn on dragging
    //    if (!dragging && Input.GetMouseButton(0))
    //    {
    //        if (Input.mousePosition != _cachedMousePos && _cachedMousePos != Vector3.negativeInfinity)
    //        {
    //            dragging = true;
    //        }
    //        else
    //        {
    //            _cachedMousePos = Input.mousePosition;
    //        }
    //    }

    //}

    /// <summary>
    /// Check for nearby asset connections if this asset has asset connection points
    /// </summary>
    void CheckForNearbyMate()
    {
        Debug.Log($"Checking for nearby mates a whole lot");
        if (selectedObject == null)
        {
            return;
        }
        AssetConnectionPoints connectionSource = selectedObject.GetComponent<AssetConnectionPoints>();
        if(connectionSource == null)
        {
            return;
        }
        
        foreach (var point in connectionSource.ConnectionPoints)
        {
            Collider[] objs = Physics.OverlapSphere(point.Value.position, 1);
            foreach (var col in objs)
            {
                if (col.TryGetComponent<AssetConnectionPoints>(out var targetConnectionPoints))
                {
                    if (targetConnectionPoints.ConnectionPoints.ContainsKey(point.Key))
                    {
                        //Debug drawing the line for now
                        Debug.Log($"Placing at point: {point.Value.name}");
                        //connectionSource.ConnectionVisualizationLine.SetPosition(0, connectionSource.transform.TransformPoint(point.Value));
                        Transform dest;
                        if (targetConnectionPoints.ConnectionPoints.TryGetValue(point.Key, out dest))
                        {
                            //connectionSource.ConnectionVisualizationLine.enabled = true;
                            //connectionSource.ConnectionVisualizationLine.SetPosition(1, targetConnectionPoints.transform.TransformPoint(dest));
                            selectedObject.transform.rotation = dest.parent.rotation;
                            Vector3 worldConnectionPointPos = dest.position;
                            Vector3 worldPoint = worldConnectionPointPos - point.Value.localPosition;
                            selectedObject.transform.position = worldPoint;
                            return;
                        }
                    }
                }
            }
        }
        //if(connectionSource.ConnectionVisualizationLine != null)
        //{
        //    connectionSource.ConnectionVisualizationLine.enabled = false;
        //}
    }

    ///// <summary>
    ///// Run required logic for selecting a placeable object. Layer is changed to allow 
    ///// drag raycast to see past it more easily.
    ///// </summary>
    ///// <param name="obj"></param>
    //void SelectObject(PlacablePrefab obj)
    //{
    //    if(selectedObject != null)
    //    {
    //        DeselectObject();
    //    }

    //    selectedObject = obj;
    //    camManager.SetCameraTarget(obj.transform);
    //    selectedObjCollider = selectedObject.gameObject.GetComponent<Collider>();  
    //    obj.gameObject.layer = LayerMask.NameToLayer("SelectedObject");
    //    int count = obj.transform.childCount;
    //    if(count > 0)
    //    {
    //        for (int i = 0; i < count; i++)
    //        {
    //            Transform objC = obj.transform.GetChild(i);
    //            objC.gameObject.layer = LayerMask.NameToLayer("SelectedObject");
    //            int subCount = objC.transform.childCount;
                
    //            if (subCount > 0)
    //            {
    //                foreach(Transform child in objC.transform)
    //                {
    //                    child.gameObject.layer = LayerMask.NameToLayer("SelectedObject");
    //                }
    //            }
    //        }
    //    }
    //    if(scenePlacer)scenePlacer.onObjectSelected?.Invoke(obj.gameObject);
    //}

    ///// <summary>
    ///// Deselects any currently selected placable object
    ///// </summary>
    //void DeselectObject()
    //{
    //    if (selectedObject != null)
    //    {
    //        selectedObject.gameObject.layer = LayerMask.NameToLayer("Default");
    //        int count = selectedObject.transform.childCount;
    //        if (count > 0)
    //        {
    //            for (int i = 0; i < count; i++)
    //            {
    //                Transform objC = selectedObject.transform.GetChild(i);
    //                objC.gameObject.layer = LayerMask.NameToLayer("Default");
    //                int subCount = objC.transform.childCount;

    //                if (subCount > 0)
    //                {
    //                    foreach (Transform child in objC.transform)
    //                    {
    //                        child.gameObject.layer = LayerMask.NameToLayer("Default");
    //                    }
    //                }
    //            }
    //        }
    //        selectedObjCollider = null;
    //        selectedObject = null;
    //        if (scenePlacer) scenePlacer.onObjectDeselected?.Invoke();
    //    }

    //}

    /// <summary>
    /// Drags the currently selected placable object based on raycast hit location and normals.
    /// </summary>
    /// <param name="target"></param>
    //void MouseDrag(PlacablePrefab target)
    //{
    //    if (!dragging)
    //    {
    //        return;
    //    }
    //    target.gameObject.SetActive(true);
    //    ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //    if (Physics.Raycast(ray, out hit, Mathf.Infinity, scenePlacer.surfaceLayers))
    //    {
    //        if (CheckValidPlacement(LayerMask.LayerToName(hit.transform.gameObject.layer)))
    //        {
    //            Transform placementTarget = hit.transform;
    //            var mineTile = placementTarget.GetComponentInParent<MineLayerTile>();
    //            if (mineTile != null) placementTarget = mineTile.transform;   

    //            Vector3 placementVector = Vector3.zero;
    //            switch(target.GetPlacementMode())
    //            {
    //                case PlacementMode.Anchor:
    //                    placementVector = hit.point - (target.GetAnchor().transform.position - target.transform.position);
    //                    break;
    //                case PlacementMode.Collider:
    //                case PlacementMode.Pivot:
    //                    placementVector = hit.point;
    //                    break;
    //            }
    //            target.transform.position = placementVector;

    //            target.transform.up = hit.normal;

    //        }
    //    }
    //}

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
    void CheckForKeyboardInput()    // TODO would really love to use new input system try to figure out error that happened last time
    {
        if(selectedObject != null)
        {
            PanObject();
            RotateObject();
            ScaleObject();
            CheckForDelete();
            CheckForFocus();
            //CheckForStatusChange();
        }
        //CheckForStatusChange();
    }

    //void CheckForStatusChange()
    //{
    //    if (Input.GetKeyDown(KeyCode.Alpha2))
    //    {
    //        scenePlacer.SwitchPlacerLogic(GizmoKind.Pan);
    //        SwitchGizmoType(GizmoKind.Pan);
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha3))
    //    {
    //        scenePlacer.SwitchPlacerLogic(GizmoKind.Rotate);
    //        SwitchGizmoType(GizmoKind.Rotate);
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha4))
    //    {
    //        scenePlacer.SwitchPlacerLogic(GizmoKind.Scale,selectedObject.gameObject);
    //        SwitchGizmoType(GizmoKind.Scale);
    //    }
    //}

    void PanObject()
    {
        Vector3 panningInput = Vector3.zero;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            panningInput += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            panningInput += Vector3.left;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            panningInput += Vector3.right;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            panningInput += Vector3.back;
        }

        selectedObject.transform.position += panningInput * panSensitivity;

    }

    /// <summary>
    /// Scales object based on its local scale. Limited with a max and min set at PlacerLogic construction
    /// </summary>
    void ScaleObject()
    {
        Vector3 scaleInput = Vector3.zero;
        Vector3 currentScale = selectedObject.transform.localScale;
        if (Input.GetKey(KeyCode.Z))
        {
            scaleInput += new Vector3(0.1f, 0.1f, 0.1f);
        }
        if (Input.GetKey(KeyCode.C))
        {
            scaleInput += new Vector3(-0.1f, -0.1f, -0.1f);
        }
        selectedObject.transform.localScale = new Vector3(
                                                Mathf.Clamp(currentScale.x + scaleInput.x,minScaleSize,maxScaleSize),
                                                Mathf.Clamp(currentScale.y + scaleInput.y, minScaleSize, maxScaleSize),
                                                Mathf.Clamp(currentScale.z + scaleInput.z, minScaleSize, maxScaleSize)
                                                );
    }

    /// <summary>
    /// Rotates selected object around its local Y axis
    /// </summary>
    void RotateObject()
    {
        Vector3 eulers = Vector3.zero;
        if (Input.GetKey(KeyCode.Q))
        {
            eulers += new Vector3(0, 1, 0) * rotationSensitivity;
        }
        if (Input.GetKey(KeyCode.E))
        {
            eulers += new Vector3(0, -1,0) * rotationSensitivity;
        }
        selectedObject.transform.Rotate(eulers);

    }

    ///// <summary>
    ///// Checks if the delete key is pressed while an object is selected, deleting the object.
    ///// </summary>
    //public override void CheckForDelete()
    //{
    //    if(Input.GetKey(KeyCode.Delete))
    //    {
    //        if(selectedObject != null)
    //        {
    //            GameObject temp = selectedObject.gameObject;
    //            DeselectObject();
    //            GameObject.Destroy(temp);
    //        }
    //    }
    //}

    //void CheckForFocus()
    //{
    //    if (Input.GetKeyDown(KeyCode.F))
    //    {
    //        if (selectedObject != null)
    //        {
    //            camManager.GetActiveCameraLogic().FocusObject(selectedObject.gameObject);
    //        }
    //    }
    //}

    //void SwitchGizmoType(GizmoKind kind)
    //{
    //    if (selectedObject != null)
    //    {
    //        scenePlacer.SwitchActiveGizmos(kind, selectedObject.gameObject);
    //    }
    //    else
    //    {
    //        scenePlacer.SwitchActiveGizmos(kind);
    //    }

    //}
    //void CheckForDeselect()
    //{
    //    if (Input.GetKey(KeyCode.Escape))
    //    {
    //        DeselectObject();
    //    }
    //}
}
