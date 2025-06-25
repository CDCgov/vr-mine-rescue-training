
using NIOSH_EditorLayers;
using System.Collections.Generic;
using UnityEngine;
using static NIOSH_EditorLayers.LayerManager;
using Transform = UnityEngine.Transform;

[System.Serializable]
public class GizmoPlacerLogic : PlacerLogic
{
    private Ray ray;
    private RaycastHit hit;
    PlacablePrefab hitPrefab;
    PlacerGizmo currentGizmo;
    Vector3 previousMousePos;
    Vector3 prevMosPosY;
    Vector3 offset;
    Vector3 startSelectObjPos;
    Quaternion startSelectObjRot;
    Vector3 startSelectObjScale;


    public GizmoPlacerLogic(List<GizmoOrder> kinds, Placer placer, CameraManager camManager) : base(kinds, placer, camManager)
    {
        //layerMask = LayerMask.GetMask("PanPlane");
        //placer.onGizmoReleased += OnGizmoReleased;
    }

    //public override void DoLogic()
    //{
    //    CheckForGizmoInput();
    //    CheckForKeyboardInput();
    //    //CheckForMouseInput();

    //}
    //public override void DoSupportLogic()
    //{
    //    CheckForFocus();
    //    CheckForDelete();
    //    CheckForDeselect();
    //}
    /// <summary>
    /// Forces a selected object, skipping the normal raycast select.
    /// For example if the UI wants to select an object.
    /// </summary>
    /// <param name="obj"></param>
    //public override void ForceSelectObject(GameObject obj)
    //{
    //    PlacablePrefab prefab = obj.GetComponent<PlacablePrefab>();
    //    if (prefab != null && prefab.GetPlacementLayer() == LayerManager.GetCurrentLayer())
    //    {
    //        if (!prefab.gameObject.activeSelf) { Debug.Log("PREFAB NOT ACTIVE"); prefab.gameObject.SetActive(true); }
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
    //    if (selectedObject != null && selectedObject.gameObject == obj)
    //    {
    //        DeselectObject();
    //    }
    //}

    //public override void ForceDeselect()
    //{
    //    DeselectObject();
    //}


    //void CheckForMouseInput()
    //{
    //    // Check to see if we've clicked on something new when we click the mouse
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //        if (Physics.Raycast(ray, out hit, Mathf.Infinity, scenePlacer.objectLayers))
    //        {

    //            if(scenePlacer.GetSelectedGizmo() != null) { return; }
    //            if (selectedObject == null || hit.transform.GetInstanceID() != selectedObject.GetInstanceID())
    //            {
    //                hitPrefab = hit.collider.gameObject.GetComponent<PlacablePrefab>();
    //                if(hitPrefab == null) { hitPrefab = hit.transform.gameObject.GetComponent<PlacablePrefab>(); }


    //                if (hitPrefab != null && hitPrefab.GetPlacementLayer() == LayerManager.GetCurrentLayer())
    //                {
    //                    SelectObject(hitPrefab);
    //                    return;
    //                }
    //                else if(hit.transform.parent != null)
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
    //    if (Input.GetMouseButtonDown(1))
    //    {
    //        DeselectObject();
    //    }*/
    //}

    //void CheckForKeyboardInput()
    //{
    //    if(Input.GetKeyDown(KeyCode.Alpha1))
    //    {
    //        if(selectedObject != null)
    //        {
    //            scenePlacer.SwitchPlacerLogic(GizmoKind.None, selectedObject.gameObject);
    //        }
    //        else
    //        {
    //            scenePlacer.SwitchPlacerLogic(GizmoKind.None);
    //        }
    //    }
        
    //    if(Input.GetKeyDown(KeyCode.Alpha2))
    //    {
    //        SwitchGizmoType(GizmoKind.Pan);
    //    }
    //    else if (Input.GetKeyDown(KeyCode.Alpha3))
    //    {
    //        SwitchGizmoType(GizmoKind.Rotate);
    //    }
    //    else if (Input.GetKeyDown(KeyCode.Alpha4))
    //    {
    //        SwitchGizmoType(GizmoKind.Scale);
    //    }
    //}

    //void CheckForGizmoInput()
    //{
    //    if(selectedObject == null) { return; }
    //    AlignActiveGizmos();
    //    currentGizmo = scenePlacer.GetSelectedGizmo();
    //    if (currentGizmo != null)
    //    {
    //        if (Input.GetMouseButtonDown(0))
    //        {
    //            prevMosPosY = Input.mousePosition;
    //            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
    //            {
    //                previousMousePos = hit.point;
    //            }
    //            else
    //            {
    //                Debug.Log(hit.transform.gameObject.name);

    //            }
    //            offset = selectedObject.transform.position - currentGizmo.transform.position;
    //            startSelectObjPos = selectedObject.transform.localPosition;
    //            startSelectObjRot = selectedObject.transform.localRotation;
    //            startSelectObjScale = selectedObject.transform.localScale;
    //        }

    //        if (Input.GetMouseButton(0))
    //        {
    //            switch (currentGizmo.GetGizmoType())
    //            {
    //                case GizmoKind.Pan:
    //                    PanObject(currentGizmo);
    //                    break;
    //                case GizmoKind.Rotate:
    //                    RotateObject(currentGizmo);
    //                    break;
    //                case GizmoKind.Scale:
    //                    ScaleObject(currentGizmo);
    //                    break;
    //            }
    //        }

    //    }
        
    //}

    //void AlignActiveGizmos()
    //{
    //    if(selectedObject == null) { return; }
    //    foreach (PlacerGizmo activeGizmo in scenePlacer.GetActiveGizmos())
    //    {
    //        activeGizmo.transform.localPosition = selectedObject.transform.position;
    //        Collider col = selectedObject.GetComponent<Collider>(); // TODO might want to cache this in the future
    //        if (col == null) { col = selectedObject.GetComponentInChildren<Collider>(); }
    //        if(col == null) { Debug.LogError("Object does not have collider, cannot adjust gizmos."); return; }
    //        float distanceToCam = Vector3.Distance(selectedObject.transform.position, Camera.main.transform.position);
    //        float colliderSize = Mathf.Clamp(Mathf.Max(Mathf.Max(col.bounds.size.x, col.bounds.size.y), col.bounds.size.z), 0.5f, 3);
    //        float gizmoScale = Mathf.Clamp(distanceToCam * scenePlacer.gizmoSizeMod, scenePlacer.minimumGizmoSize, scenePlacer.maximumGizmoSize);

    //        activeGizmo.transform.localScale = new Vector3(gizmoScale, gizmoScale, gizmoScale);

    //        if (activeGizmo.GetGizmoType() == GizmoKind.Rotate)
    //        {
    //            activeGizmo.transform.rotation = selectedObject.transform.rotation;
    //        }
    //    }
    //}
    
    ///// <summary>
    ///// Run required logic for selecting a placeable object. Layer is changed to allow 
    ///// drag raycast to see past it more easily.
    ///// </summary>
    ///// <param name="obj"></param>
    //public void SelectObject(PlacablePrefab obj)
    //{
    //    if (selectedObject != null)
    //    {
    //        DeselectObject();
    //    }

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
    //    scenePlacer.ActivateGizmos(obj.gameObject);
    //    if (scenePlacer) 
    //        scenePlacer.onObjectSelected?.Invoke(obj.gameObject);
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


    //    }
    //    scenePlacer.DeactivateGizmos();
    //    if (scenePlacer) scenePlacer.onObjectDeselected?.Invoke();
    //}

    void PanObject(PlacerGizmo gizmo)
    {
        Vector3 mouseDelta = Vector3.zero;
        Vector3 newPos = Vector3.zero;

        Vector3 mosInput = Input.mousePosition;
        //Vector3 mouseYOffset = mosInput - prevMosPosY;
        //Vector3 startObjPos = selectedObject.transform.position;

        var mousePos = Input.mousePosition;
        if (Vector3.Distance(mousePos, previousMousePos) > 50)
        {
            previousMousePos = mousePos;
            return;
        }

        var camRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        var newRay = Camera.main.ScreenPointToRay(mousePos);
        var oldRay = Camera.main.ScreenPointToRay(previousMousePos);
        //if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        //{
        //    mousePos = hit.point;
        //    mouseDelta = mousePos - previousMousePos;
        //}

        Vector3 oldPos = selectedObject.transform.position;
        Plane gizmoPlane = new Plane(Vector3.up, 0);
        Vector3 gizmoMask = Vector3.zero;

        switch(gizmo.GetAxis())
        {
            case GizmoAxis.X:
                //gizmoPlane = new Plane(new Vector3(0, 1, 0), oldPos);
                gizmoMask = new Vector3(1, 0, 0);
                gizmoPlane = ComputeGizmoPlane(gizmoMask, camRay, oldPos);
                break;
            case GizmoAxis.Y:
                //gizmoPlane = new Plane(new Vector3(1, 0, 0), oldPos);
                gizmoMask = new Vector3(0, 1, 0);
                gizmoPlane = ComputeGizmoPlane(gizmoMask, camRay, oldPos);
                break;
            case GizmoAxis.Z:
                //gizmoPlane = new Plane(new Vector3(0, 1, 0), oldPos);
                gizmoMask = new Vector3(0, 0, 1);
                gizmoPlane = ComputeGizmoPlane(gizmoMask, camRay, oldPos);
                break;
            case GizmoAxis.XY:
                gizmoPlane = new Plane(new Vector3(0, 0, 1), oldPos);
                gizmoMask = new Vector3(1, 1, 0);
                break;
            case GizmoAxis.XZ:
                gizmoPlane = new Plane(new Vector3(0, 1, 0), oldPos);
                gizmoMask = new Vector3(1, 0, 1);
                break;
            case GizmoAxis.ZY:
                gizmoPlane = new Plane(new Vector3(1, 0, 0), oldPos);
                gizmoMask = new Vector3(0, 1, 1);
                break;

            //case GizmoAxis.X:
            //    newPos = selectedObject.transform.position + mouseDelta;
            //    selectedObject.transform.position = new Vector3(newPos.x, oldPos.y, oldPos.z);
            //    break;
            //case GizmoAxis.Y:
            //    selectedObject.transform.position = startObjPos + (mouseYOffset.y * selectedObject.transform.up) * Time.deltaTime;
            //    break;
            //case GizmoAxis.Z:
            //    newPos = selectedObject.transform.position + mouseDelta;
            //    selectedObject.transform.position = new Vector3(oldPos.x, oldPos.y, newPos.z);
            //    break;
            //case GizmoAxis.XY:
            //    newPos = selectedObject.transform.position + mouseDelta;
            //    selectedObject.transform.position = new Vector3(newPos.x, (startObjPos + (mouseYOffset.y * selectedObject.transform.up) * Time.deltaTime).y, oldPos.z);
            //    break;
            //case GizmoAxis.XZ:
            //    newPos = selectedObject.transform.position + mouseDelta;
            //    selectedObject.transform.position = new Vector3(newPos.x, oldPos.y, newPos.z);
            //    break;
            //case GizmoAxis.ZY:
            //    newPos = selectedObject.transform.position + mouseDelta;
            //    selectedObject.transform.position = new Vector3(oldPos.x, (startObjPos + (mouseYOffset.y * selectedObject.transform.up) * Time.deltaTime).y, newPos.z);
            //    break;

        }

        if (!RaycastGizmoPlane(gizmoPlane, newRay, out var newPt))
            return;
        if (!RaycastGizmoPlane(gizmoPlane, oldRay, out var oldPt))
            return;

        var delta = newPt - oldPt;
        delta.Scale(gizmoMask);
        selectedObject.transform.position += delta;

        previousMousePos = mousePos;
        prevMosPosY = mosInput; 
    }

    private Plane ComputeGizmoPlane(Vector3 axis, Ray ray, Vector3 ptOnPlane)
    {
        var normal = ray.direction - Vector3.Dot(axis, ray.direction) * axis;

        //Debug.DrawLine(ptOnPlane, ptOnPlane + normal * 10, Color.cyan, 4);
        return new Plane(normal, ptOnPlane);
    }

    private bool RaycastGizmoPlane(Plane plane, Ray ray, out Vector3 pt)
    {
        pt = Vector3.zero;

        if (Mathf.Abs(Vector3.Dot(ray.direction, plane.normal)) < 0.1f)
            return false; // ray is too parallel to the plane

        if (!plane.Raycast(ray, out float enter))
            return false;


        pt = ray.origin + ray.direction * enter;
        return true;
    }

    void ScaleObject(PlacerGizmo gizmo)
    {
        Vector3 mousePos = Vector3.zero;
        Vector3 mouseDelta = Vector3.zero;
        Vector3 newScale = Vector3.zero;

        Vector3 mosInput = Input.mousePosition;
        Vector3 mouseYOffset = mosInput - prevMosPosY;
        Vector3 startObjScale = selectedObject.transform.localScale;


        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            mousePos = hit.point;
            mouseDelta = mousePos - previousMousePos;
        }

        Vector3 oldPos = selectedObject.transform.localScale;
        switch (gizmo.GetAxis())
        {
            case GizmoAxis.X:
                newScale = selectedObject.transform.localScale + mouseDelta;
                selectedObject.transform.localScale = new Vector3(newScale.x, oldPos.y, oldPos.z);
                break;
            case GizmoAxis.Y:
                selectedObject.transform.localScale = startObjScale + (mouseYOffset.y * selectedObject.transform.up) * Time.deltaTime;
                break;
            case GizmoAxis.Z:
                newScale = selectedObject.transform.localScale + mouseDelta;
                selectedObject.transform.localScale = new Vector3(oldPos.x, oldPos.y, newScale.z);
                break;
            case GizmoAxis.XYZ:
                float changeInDelta = Mathf.Abs(mouseDelta.x) > Mathf.Abs(mouseDelta.z) ? mouseDelta.x : mouseDelta.z;
                newScale = selectedObject.transform.localScale + new Vector3( changeInDelta,changeInDelta,changeInDelta);
                selectedObject.transform.localScale = newScale;
                break;
        }

        Vector3 clampedScale = new Vector3(Mathf.Clamp(selectedObject.transform.localScale.x, .1f,500),
                                           Mathf.Clamp(selectedObject.transform.localScale.y, .1f, 500),
                                           Mathf.Clamp(selectedObject.transform.localScale.z, .1f, 500));
        selectedObject.transform.localScale = clampedScale;

        previousMousePos = mousePos;
        prevMosPosY = mosInput;
    }

    void RotateObject(PlacerGizmo gizmo)
    {
        Vector3 mousePos = Vector3.zero;
        Vector3 rotationAxis;
        Vector3 mosInput = Input.mousePosition;
        //float rotateAmount = 0;
  
        //switch (gizmo.GetAxis())
        //{
        //    case GizmoAxis.X:
        //        rotationAxis = Vector3.forward;
        //        break;
        //    case GizmoAxis.Y:
        //        rotationAxis = Vector3.up;
        //        break;
        //    case GizmoAxis.Z:
        //        rotationAxis = Vector3.right;
        //        break;
        //}
        
        //Vector3 rotation = selectedObject.transform.TransformDirection(new Vector3(Input.GetAxis("Mouse Y"), -Input.GetAxis("Mouse X"), 0));
        //Quaternion.Euler(rotation).ToAngleAxis(out rotateAmount, out rotationAxis);
        //rotateAmount *= scenePlacer.rotationSpeed;

        Vector3 axis = Vector3.up;
        
        switch (gizmo.GetAxis())
        {
            case GizmoAxis.X:
                axis = new Vector3(0, 0, 1);
                //selectedObject.transform.Rotate(new Vector3(0,0, -rotationAxis.z), rotateAmount, Space.Self);
                break;
            case GizmoAxis.Y:
                axis = new Vector3(0, 1, 0);
                //selectedObject.transform.Rotate(new Vector3(0, rotationAxis.y, 0), rotateAmount, Space.Self);
                break;
            case GizmoAxis.Z:
                axis = new Vector3(1, 0, 0);
                //selectedObject.transform.Rotate(new Vector3(-rotationAxis.x, 0, 0), rotateAmount, Space.Self);
                break;
        }

        //selectedObject.transform.Rotate(axis, rotateAmount, Space.Self);

        if (Camera.main != null)
        {
            var worldAxis = selectedObject.transform.TransformDirection(axis);
            var rot = ComputeObjectRotation(Camera.main, selectedObject.transform.position, worldAxis, axis);
            //selectedObject.transform.Rotate(axis, rotateAmount, Space.Self);
            selectedObject.transform.localRotation *= rot;
        }

        gizmo.transform.rotation = selectedObject.transform.rotation;
        previousMousePos = mousePos;
        prevMosPosY = mosInput;
    }


    private Quaternion ComputeObjectRotation(Camera camera, Vector3 pos, Vector3 axis, Vector3 localAxis)
    {
        //var inputVector = new Vector3(Input.GetAxis("Mouse Y"), -Input.GetAxis("Mouse X");

        var plane = new Plane(axis, pos);

        var prevRay = camera.ScreenPointToRay(prevMosPosY);
        var currentRay = camera.ScreenPointToRay(Input.mousePosition);
        if (!plane.Raycast(currentRay, out var dist))
            return Quaternion.identity;
        var p1 = currentRay.origin + currentRay.direction * dist;

        if (!plane.Raycast(prevRay, out dist))
            return Quaternion.identity;

        var p2 = prevRay.origin + prevRay.direction * dist;

        //Debug.DrawLine(p1, p2, Color.green, 2.0f);
        //Debug.DrawLine(plane.normal * plane.distance, plane.normal * plane.distance + plane.normal * 3.0f, Color.magenta, 2.0f);
        //Debug.DrawLine(pos, pos + axis * 3.0f, Color.cyan, 2.0f);

        var v1 = pos - p1;
        var v2 = pos - p2;

        var xproduct = Vector3.Cross(v2, v1);

        //return Quaternion.identity;

        //return Quaternion.FromToRotation(v2, v1);

        var angle = Vector3.Angle(v1, v2);
        if (Vector3.Dot(xproduct, axis) < 0)
            angle *= -1.0f;

        return Quaternion.AngleAxis(angle, localAxis);
    }


    //void SwitchGizmoType(GizmoKind kind)
    //{
    //    if(selectedObject != null)
    //    {
    //        scenePlacer.SwitchActiveGizmos(kind, selectedObject.gameObject);
    //    }
    //    else
    //    {
    //        scenePlacer.SwitchActiveGizmos(kind);
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

    //public override void CheckForDelete()
    //{
    //    if (Input.GetKey(KeyCode.Delete))
    //    {
    //        if (selectedObject != null)
    //        {
    //            //UndoManager.Insert(new DestroyCommand(selectedObject, scenePlacer.assetContainer, selectedObject.transform.localPosition, 
    //            //                                      selectedObject.transform.localRotation,selectedObjCollider.transform.localScale));
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
    //    if(selectedObject == null && scenePlacer.CheckAreGizmosActive()) { DeselectObject(); }
    //}

    void OnGizmoReleased()
    {
        if (selectedObject != null)
        {
            //UndoManager.Insert(new MoveCommand(startSelectObjPos, selectedObject.transform.localPosition,
            //                           startSelectObjRot, selectedObject.transform.localRotation,
            //                           startSelectObjScale, selectedObject.transform.localScale, selectedObject.transform));
        }
    }

}
