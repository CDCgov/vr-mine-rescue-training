using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NIOSH_MineCreation;
using static NIOSH_EditorLayers.LayerManager;
using NIOSH_EditorLayers;

public class DragSnapPlacerLogic : PlacerLogic
{
    // Raycast references
    private Ray ray;
    private RaycastHit hit;

    Vector3 startSelectObjPos;
    Vector3 startSelectObjScale;
    Quaternion startSelectObjRot;

    bool dragging = false;
    PlacablePrefab hitPrefab;

    bool isSnapping = false;

    Vector3 _cachedMousePosition = Vector3.negativeInfinity;
    public DragSnapPlacerLogic(List<GizmoOrder> kinds, Placer placer, CameraManager camManager) : base(kinds, placer, camManager)
    {


    }

    /// <summary>
    /// Called by the placer every frame. Checks for mouse and keyboard input.
    /// </summary>
    public override void DoLogic() // TODO would love to use the new input system
    {
        //CheckForMouseInput();
        //CheckForKeyboardInput();
        CheckForDeselect();
    }
    public override void DoSupportLogic()
    {
        CheckForFocus();
        CheckForDelete();
        CheckForDeselect();
    }

    ///// <summary>
    ///// Forces a selected object, skipping the normal raycast select.
    ///// For example if the UI wants to select an object.
    ///// </summary>
    ///// <param name="obj"></param>
    //public override void ForceSelectObject(GameObject obj)
    //{
    //    PlacablePrefab prefab = obj.GetComponent<PlacablePrefab>();
    //    if (prefab != null && prefab.GetPlacementLayer() == LayerManager.GetCurrentLayer())
    //    {
    //        if (!prefab.gameObject.activeSelf) { Debug.Log("PREFAB NOT ACTIVE"); prefab.gameObject.SetActive(true); }
    //        if(prefab.gameObject.TryGetComponent(out MineLayerTile tile))
    //        {
    //            tile.ChangeModeToEdit(false);
    //            tile.ScaleToSettings();
    //        }

    //        SelectObject(prefab);
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

    ///// <summary>
    ///// Intended to run each frame, checks for current left mouse button status
    ///// as well as whether an object is selected and something is currently being dragged.
    ///// </summary>
    //void CheckForMouseInput()
    //{
    //    // End drag if we release the mouse
    //    if (selectedObject != null && dragging && Input.GetMouseButtonUp(0))
    //    {
    //        dragging = false;
    //        _cachedMousePosition = Vector3.negativeInfinity;
    //        if (isSnapping)
    //            selectedObject.SetPlaced();
    //        else if(selectedObject.TryGetComponent(out MineLayerTile tile))
    //        {
    //            tile.HandlePlacedNoSnap();
    //        }
                

    //        isSnapping = false;
    //        //UndoManager.Insert(new SpawnPlaceableCommand(selectedObject.gameObject, selectedObject.transform.localPosition,
    //        //                selectedObject.transform.localRotation, selectedObject.transform.localScale));

    //    }

    //    // If we are dragging something run drag logic
    //    if (dragging && selectedObject != null)
    //    {
    //        MouseDrag(selectedObject);
    //    }

    //    // Check to see if we've clicked on something new when we click the mouse
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //       // LayerMask mask = ~LayerMask.GetMask("Roof","Gizmo","IgnoreRaycast");
    //        LayerMask mask = LayerMask.GetMask("Floor");
    //        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask, QueryTriggerInteraction.Ignore))
    //        {
    //            Debug.Log("HIT: " + LayerMask.LayerToName(hit.collider.gameObject.layer));
    //            if (selectedObject == null || hit.transform.GetInstanceID() != selectedObject.GetInstanceID())
    //            {
    //                hitPrefab = hit.collider.transform.gameObject.GetComponent<PlacablePrefab>();
    //                if(hitPrefab == null)
    //                {
    //                    hitPrefab = hit.collider.transform.gameObject.GetComponentInParent<PlacablePrefab>();

    //                }
    //                if (hitPrefab != null)
    //                {
    //                    SelectObject(hitPrefab);
    //                    return;
    //                }

    //            }
    //            // FIXME don't like how this logic tree is set up
    //            if (selectedObject != null && hit.collider.GetInstanceID() != selectedObject.GetInstanceID())
    //            {
    //                DeselectObject();
    //            }
    //        }
    //        else
    //        {
    //            DeselectObject();
    //        }
    //    }
        

    //    // If we hold the mouse down and are not already dragging turn on dragging
    //    if (!dragging && Input.GetMouseButton(0))
    //    {
    //        if (_cachedMousePosition == Vector3.negativeInfinity)
    //        {
    //            _cachedMousePosition = Input.mousePosition;
    //        }
    //        else
    //        {
    //            if (_cachedMousePosition != Input.mousePosition)
    //            {
    //                dragging = true;
    //                if (selectedObject)
    //                {
    //                    selectedObject.UnPlace();
    //                    startSelectObjPos = selectedObject.transform.localPosition;
    //                    startSelectObjRot = selectedObject.transform.localRotation;
    //                    startSelectObjScale = selectedObject.transform.localScale;
    //                    //UndoManager.Insert(new DestroyCommand(selectedObject.gameObject, selectedObject.transform.localPosition,
    //                    //selectedObject.transform.localRotation, selectedObject.transform.localScale));
    //                }
    //                _cachedMousePosition = Input.mousePosition;
    //            }
    //        }


    //    }

    //}

    ///// <summary>
    ///// Run required logic for selecting a placeable object. Layer is changed to allow 
    ///// drag raycast to see past it more easily.
    ///// </summary>
    ///// <param name="obj"></param>
    //void SelectObject(PlacablePrefab obj)
    //{
    //    if (selectedObject != null)
    //    {
    //        DeselectObject();
    //    }
    //    selectedObject = obj;
    //    selectedObjCollider = selectedObject.gameObject.GetComponent<Collider>();
    //    obj.gameObject.layer = LayerMask.NameToLayer("SelectedObject");
    //    //BDM: This threw a Null ref when selecting mine segments?
    //    if (scenePlacer) 
    //    { 
    //        scenePlacer.onObjectSelected?.Invoke(obj.gameObject); 
    //    }
    //}

    ///// <summary>
    ///// Deselects any currently selected placable object
    ///// </summary>
    //void DeselectObject()
    //{
    //    if (selectedObject != null)
    //    {
    //        selectedObject.gameObject.layer = LayerMask.NameToLayer("MineSegments");
    //        selectedObjCollider = null;
    //        selectedObject = null;
    //    }
    //    if(scenePlacer)scenePlacer.onObjectDeselected?.Invoke();

    //}

    /// <summary>
    /// Drags the currently selected placable object based on raycast hit location and normals.
    /// </summary>
    /// <param name="target"></param>
    void MouseDrag(PlacablePrefab target)
    {
        target.gameObject.SetActive(true);
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("PanPlane", "MineSegments")))
        {
            if (CheckValidPlacement(LayerMask.LayerToName(hit.transform.gameObject.layer)))
            {
                if(hit.transform.gameObject.layer == LayerMask.NameToLayer("MineSegments")) //TODO: maybe make a layer or something other than a string compare
                {
                    //Debug.Log("hit tile is " + hit.transform.name);
                    //MineLayerTile hitTile = hit.transform.GetComponent<MineLayerTile>();
                    MineLayerTile hitTile = hit.transform.GetComponentInParent<MineLayerTile>();
                    int connectionIndex = hitTile.GetConnectionIndexFromPosition(hit.point);

                    if (connectionIndex == -1)
                    {
                        isSnapping = false;
                        target.transform.position = hit.point;
                        return;
                    }
                    int matchingIndex = selectedObject.GetComponent<MineLayerTile>().GetConnectionIndexFromConnectionID(hitTile.GetConnection(connectionIndex).ConnectionID);

                    if (matchingIndex == -1)
                    {
                        isSnapping = false;
                        target.transform.position = hit.point;
                        return;
                    }

                    target.transform.position = selectedObject.GetComponent<MineLayerTile>().GetSnappingPositionForCentroid(matchingIndex, hitTile.GetCentroidWorldPosition(connectionIndex));
                    isSnapping = true;
                }
                else
                {
                    target.transform.position = hit.point;
                    isSnapping = false;

                    //target.transform.up = hit.normal;
                }
            }
        }
        
    }

    private void DetermineConnection()
    {

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

    /// <summary>
    /// Checks if the delete key is pressed while an object is selected, deleting the object.
    /// </summary>
    //public override void CheckForDelete()
    //{
    //    if (Input.GetKey(KeyCode.Delete))
    //    {
    //        if (selectedObject != null)
    //        {
    //            //UndoManager.Insert(new DestroyCommand(selectedObject, scenePlacer.assetContainer, selectedObject.transform.localPosition,
    //            //                      selectedObject.transform.localRotation, selectedObjCollider.transform.localScale));
    //            GameObject temp = selectedObject.gameObject;

    //            //selectedObject.SetIgnoreSave(true);
    //            DeselectObject();
    //            //temp.SetActive(false);
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
    //public override void CheckForFocus()
    //{
    //    if (Input.GetKeyDown(KeyCode.F))
    //    {
    //        if (selectedObject != null)
    //        {
    //            camManager.GetActiveCameraLogic().FocusObject(selectedObject.gameObject);

    //        }
    //    }
    //}

    public void SetIsSnapping(bool state)
    {
        isSnapping = state;
    }
}
