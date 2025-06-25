using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.EventSystems;
using NIOSH_EditorLayers;
using static NIOSH_EditorLayers.LayerManager;
using NIOSH_MineCreation;
/// <summary>

public class SnapCurtainPlacerLogic : PlacerLogic
{
    PlacablePrefab hitPrefab;
    ScaleSpawnedCurtain scaler;
    Transform _snapZone;
    private Ray ray;
    private RaycastHit hit;

    bool dragging = false;

    bool _flipAxis;
    bool _isNorthSouth;

    public SnapCurtainPlacerLogic(List<GizmoOrder> kinds, Placer placer, CameraManager camManager) : base(kinds, placer, camManager)
    {
    }

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
    ///// Intended to run each frame, checks for current left mouse button status
    ///// as well as whether an object is selected and something is currently being dragged.
    ///// </summary>
    //void CheckForMouseInput()
    //{
    //    // End drag if we release the mouse
    //    if (selectedObject != null && dragging && Input.GetMouseButtonUp(0))
    //    {
    //        dragging = false;
    //        if (_snapZone != null)
    //            selectedObject.SetPlaced();
    //        _snapZone = null;
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
    //        // LayerMask mask = ~LayerMask.GetMask("Roof","Gizmo","IgnoreRaycast");
    //        LayerMask mask = scenePlacer.objectLayers;
    //        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask, QueryTriggerInteraction.Ignore))
    //        {
    //           // Debug.Log("HIT: " + LayerMask.LayerToName(hit.collider.gameObject.layer));
    //            if (selectedObject == null || hit.transform.GetInstanceID() != selectedObject.GetInstanceID())
    //            {
    //                hitPrefab = hit.collider.transform.gameObject.GetComponent<PlacablePrefab>();
    //                if (hitPrefab == null)
    //                {
    //                    hitPrefab = hit.collider.transform.gameObject.GetComponentInParent<PlacablePrefab>();

    //                }
    //                if (hitPrefab != null)
    //                {
    //                    SelectObject(hitPrefab);
    //                    CheckForPlacementOverride(hitPrefab);
                        
    //                    return;
    //                }

    //            }
    //            // FIXME don't like how this logic tree is set up
    //            if (selectedObject != null && hit.collider.GetInstanceID() != selectedObject.GetInstanceID())
    //            {
    //                DeselectObject();
    //            }
    //        }
    //        else if(Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("VentVisualization")))
    //        {
    //            hitPrefab = hit.collider.transform.gameObject.GetComponent<PlacablePrefab>();
    //            if (hitPrefab == null) {  hitPrefab = hit.collider.transform.gameObject.GetComponentInParent<PlacablePrefab>(); }

    //            if (hitPrefab != null)
    //            {

    //                Debug.Log("select vent placer");
    //                //DeselectObject();
    //                SelectObject(hitPrefab);
    //                CheckForPlacementOverride(hitPrefab);
                    
    //                return;
    //            }
    //        }
    //        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Gizmo")))
    //        {
    //            hitPrefab = hit.collider.transform.gameObject.GetComponent<PlacablePrefab>();
    //            if (hitPrefab == null) { hitPrefab = hit.collider.transform.gameObject.GetComponentInParent<PlacablePrefab>(); }

    //            if (hitPrefab != null)
    //            {
    //                SelectObject(hitPrefab);
    //                CheckForPlacementOverride(hitPrefab);

    //                return;
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
    //        dragging = true;
    //        if (selectedObject)
    //            selectedObject.UnPlace();
    //    }

    //}

    //void CheckForKeyboardInput()    // TODO would really love to use new input system try to figure out error that happened last time
    //{
    //    if (selectedObject != null)
    //    {
    //        RotateObject();
    //        //CheckForDelete();
    //    }
    //}

    ///// <summary>
    ///// Added the check for delete function that was mising in this placer logic.
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

    ///// <summary>
    ///// Rotates selected object around its local Y axis
    ///// </summary>
    //void RotateObject()
    //{
    //    if (!dragging)
    //        return;
    //    if(_snapZone != null)
    //    {
    //        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))
    //        {
    //            _flipAxis = !_flipAxis;
    //        }
    //    }
    //    else
    //    {
    //        if ( Input.GetKeyDown(KeyCode.E))
    //        {
    //            selectedObject.transform.eulerAngles = new Vector3(selectedObject.transform.eulerAngles.x, selectedObject.transform.eulerAngles.y + 90, selectedObject.transform.eulerAngles.z);
    //        }
    //        if (Input.GetKeyDown(KeyCode.Q))
    //        {
    //            selectedObject.transform.eulerAngles = new Vector3(selectedObject.transform.eulerAngles.x, selectedObject.transform.eulerAngles.y - 90, selectedObject.transform.eulerAngles.z);
    //        }
    //    }

    //}


    /// <summary>
    /// Drags the currently selected placable object based on raycast hit location and normals.
    /// </summary>
    /// <param name="target"></param>
    //public void MouseDrag(PlacablePrefab target)
    //{
    //    ObjectInfo targetInfo = target.GetComponent<ObjectInfo>();
    //        //Debug.Log("Mouse Drag Curtain");
    //        //target.gameObject.SetActive(true);
    //        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //    if (Physics.Raycast(ray, out hit, Mathf.Infinity, scenePlacer.surfaceLayers))
    //    {
    //        if (CheckValidPlacement(LayerMask.LayerToName(hit.transform.gameObject.layer)))
    //        {
    //            Transform placementTarget = hit.transform;
    //            var mineTile = placementTarget.GetComponentInParent<MineLayerTile>();
    //            if (mineTile != null) placementTarget = mineTile.transform;

    //            Vector3 placementVector = Vector3.zero;
    //            switch (target.GetPlacementMode())
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

    //            if (_snapZone != null)
    //            {
    //                //Set Rotation
    //                if (!_flipAxis) { target.transform.rotation = Quaternion.LookRotation(_snapZone.forward); }
    //                else { target.transform.rotation = Quaternion.LookRotation(-_snapZone.forward); }

    //                //Set Position
    //                if (_isNorthSouth)
    //                {
    //                    target.transform.position = new Vector3(_snapZone.position.x, _snapZone.position.y, placementVector.z);
    //                    target.transform.localScale = new Vector3(_snapZone.parent.lossyScale.x, _snapZone.parent.lossyScale.y, target.transform.localScale.z);
                        
    //                }
    //                else
    //                {
    //                    target.transform.position = new Vector3(placementVector.x, _snapZone.position.y, _snapZone.position.z);
    //                    target.transform.localScale = new Vector3(_snapZone.parent.lossyScale.z, _snapZone.parent.lossyScale.y, target.transform.localScale.z);
    //                }

    //                //if (targetInfo.ScaledMaterial != null) targetInfo.ScaledMaterial.mainTexture.
    //            }
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
    /// Forces the current selected object to be deselected. 
    /// For example if UI input other than from mouse or keyboard merits deselection.
    /// </summary>
    /// <param name="obj"></param>
    
    //public override void ForceDeselectObject(GameObject obj)
    //{
    //    if (selectedObject != null && selectedObject.gameObject == obj)
    //    {
    //        DeselectObject();
    //    }
    //}
    
    //void DeselectObject()
    //{
    //    //Debug.Log("Deselect Curtain");
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

    //public override void ForceSelectObject(GameObject obj)
    //{
    //    if (!obj) return;
    //    PlacablePrefab prefab = obj.GetComponent<PlacablePrefab>();
       
    //    //Debug.Log("force select object");//

    //    if (prefab != null && prefab.GetPlacementLayer() == LayerManager.GetCurrentLayer())
    //    {
    //        if (!prefab.gameObject.activeSelf) { Debug.Log("PREFAB NOT ACTIVE"); prefab.gameObject.SetActive(true); }
    //        SelectObject(prefab);
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
    //    //Debug.Log("select object");
    //    selectedObject = obj;
    //    camManager.SetCameraTarget(obj.transform);
    //    selectedObjCollider = selectedObject.gameObject.GetComponent<Collider>();
    //    obj.gameObject.layer = LayerMask.NameToLayer("SelectedObject");
    //    int count = obj.transform.childCount;
    //    if (count > 0)
    //    {
    //        for (int i = 0; i < count; i++)
    //        {
    //            Transform objC = obj.transform.GetChild(i);
    //            objC.gameObject.layer = LayerMask.NameToLayer("SelectedObject");
    //            int subCount = objC.transform.childCount;

    //            if (subCount > 0)
    //            {
    //                foreach (Transform child in objC.transform)
    //                {
    //                    child.gameObject.layer = LayerMask.NameToLayer("SelectedObject");
    //                }
    //            }
    //        }
    //    }
    //    if (scenePlacer) scenePlacer.onObjectSelected?.Invoke(obj.gameObject);
        
    //}

    public void SetSnapZone(Transform snapZone, bool isNorthSouth)
    {
        _snapZone = snapZone;
        _isNorthSouth = isNorthSouth;

    }
}
