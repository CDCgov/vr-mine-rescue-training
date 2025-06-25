using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public enum GizmoKind { Rotate, Pan, Scale, Resize, None }
public enum GizmoAxis { X, Y, Z, XY, XZ, ZY, XYZ }   
public class PlacerGizmo : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    /*
     Different gizmo objects that
    the placer has an array of
    associated to an enum to
    select which gizmo should
    be active (translate, rotate,
    etc.). 
    */

    // Needs to:
    /*
     * - Have an associated prefab scene object
     * - Handle input calls and send back to placer
     * 
     */
    //Action<PointerEventData, PlacerGizmo> onPointerDown;
    //Action<PointerEventData, PlacerGizmo> onPointerUp;

    
    [FormerlySerializedAs("axis")]
    public GizmoAxis Axis;
    [FormerlySerializedAs("type")]
    public GizmoKind GizmoType;
    public Vector3 VectorAxis;

    public float GizmoSizeMod = 1.5f;
    public float MinGizmoSize = 5f;
    public float MaxGizmoSize = 100f;

    Material gizmoMat;
    Color originalColor;

    private Collider _collider;
    private Vector3 _initialScale;

    public void Start()
    {
        _collider = GetComponent<Collider>();

        gizmoMat = GetComponent<Renderer>().material;
        originalColor = gizmoMat.color;
        _initialScale = transform.localScale;
    }

    public GizmoAxis GetAxis()
    {
        return Axis;
    }

    public GizmoKind GetGizmoType()
    {
        return GizmoType;
    }

    public void OnGizmoActive()
    {
        gizmoMat.color = Color.yellow;
    }

    public void OnGizmoInactive()
    {
        gizmoMat.color = originalColor;
    }

    //public void SubscribeToGizmoActions(Action<PointerEventData,PlacerGizmo> placerOnDown, Action<PointerEventData, PlacerGizmo> placerOnUp)
    //{
    //    onPointerDown += placerOnDown;
    //    onPointerUp += placerOnUp; 
    //}

    //public void UnsubscribeFromGizmoActions(Action<PointerEventData, PlacerGizmo> placerOnDown, Action<PointerEventData, PlacerGizmo> placerOnUp)
    //{
    //    onPointerDown -= placerOnDown;
    //    onPointerUp -= placerOnUp;
    //}

    public void OnPointerDown(PointerEventData eventData)
    {
        //onPointerDown?.Invoke(eventData, this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //onPointerUp?.Invoke(eventData, this);
    }

    public void ProcessMouseDrag(Transform obj, ScenarioCursorData oldCursor, ScenarioCursorData newCursor)
    {
        switch (GizmoType)
        {
            case GizmoKind.Pan:
                TranslateObject(obj, oldCursor.SceneRay, newCursor.SceneRay);
                break;

            case GizmoKind.Rotate:
                RotateObject(obj, oldCursor.SceneRay, newCursor.SceneRay);
                break;

            case GizmoKind.Scale:
                ScaleObject(obj, oldCursor, newCursor);
                break;

            case GizmoKind.Resize:
                ResizeObject(obj, oldCursor, newCursor);
                break;
        }
    }

    public void AlignGizmo(GameObject selectedObject)
    {
        if (selectedObject == null || _collider == null)
            return;

        if (GizmoType == GizmoKind.Resize)
        {
            AlignResizeGizmo(selectedObject);
            return;
        }

        transform.position = selectedObject.transform.position;

        float distanceToCam = Vector3.Distance(selectedObject.transform.position, Camera.main.transform.position);
        float colliderSize = Mathf.Clamp(Mathf.Max(_collider.bounds.size.x, _collider.bounds.size.y, _collider.bounds.size.z), 0.5f, 3);
        float gizmoScale = Mathf.Clamp(distanceToCam * GizmoSizeMod, MinGizmoSize, MaxGizmoSize);

        //transform.localScale = new Vector3(gizmoScale, gizmoScale, gizmoScale);
        transform.localScale = _initialScale * gizmoScale;

        if (GizmoType == GizmoKind.Rotate || GizmoType == GizmoKind.Scale)
        {
            transform.rotation = selectedObject.transform.rotation;
        }

    }

    private void AlignResizeGizmo(GameObject selectedObject)
    {
        if (!selectedObject.TryGetComponent<IScenarioEditorResizable>(out var resize))
            return;

        var extents = resize.Size * 0.5f;
        var offset = Vector3.Scale(extents, VectorAxis);

        //by convention the transform point is at the bottom center, compute 3D center
        //var center = selectedObject.transform.position;
        //center.y += extents.y;

        var center = selectedObject.transform.position + selectedObject.transform.TransformDirection(resize.LocalCenter);
        transform.position = center + selectedObject.transform.TransformDirection(offset);

        //var center = resize.LocalCenter + offset;
        //center = selectedObject.transform.TransformDirection(center);
        //transform.position = center;
    }

    private void ComputeGizmoPlane(out Plane gizmoPlane, out Vector3 gizmoMask, Ray camRay, Vector3 pos)
    {
        gizmoPlane = new Plane(Vector3.up, 0);
        gizmoMask = Vector3.zero;

        switch (Axis)
        {
            case GizmoAxis.X:
                //gizmoPlane = new Plane(new Vector3(0, 1, 0), oldPos);
                gizmoMask = new Vector3(1, 0, 0);
                gizmoPlane = ComputeGizmoPlane(gizmoMask, camRay, pos);
                break;
            case GizmoAxis.Y:
                //gizmoPlane = new Plane(new Vector3(1, 0, 0), oldPos);
                gizmoMask = new Vector3(0, 1, 0);
                gizmoPlane = ComputeGizmoPlane(gizmoMask, camRay, pos);
                break;
            case GizmoAxis.Z:
                //gizmoPlane = new Plane(new Vector3(0, 1, 0), oldPos);
                gizmoMask = new Vector3(0, 0, 1);
                gizmoPlane = ComputeGizmoPlane(gizmoMask, camRay, pos);
                break;
            case GizmoAxis.XY:
                gizmoPlane = new Plane(new Vector3(0, 0, 1), pos);
                gizmoMask = new Vector3(1, 1, 0);
                break;
            case GizmoAxis.XZ:
                gizmoPlane = new Plane(new Vector3(0, 1, 0), pos);
                gizmoMask = new Vector3(1, 0, 1);
                break;
            case GizmoAxis.ZY:
                gizmoPlane = new Plane(new Vector3(1, 0, 0), pos);
                gizmoMask = new Vector3(0, 1, 1);
                break;
        }
    }

    void TranslateObject(Transform obj, Ray oldRay, Ray newRay)
    {        
        var camRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);       

        Vector3 oldPos = obj.position;

        ComputeGizmoPlane(out var gizmoPlane, out var gizmoMask, camRay, oldPos);

        if (!RaycastGizmoPlane(gizmoPlane, newRay, out var newPt))
            return;
        if (!RaycastGizmoPlane(gizmoPlane, oldRay, out var oldPt))
            return;

        var delta = newPt - oldPt;
        delta.Scale(gizmoMask);
        obj.position += delta;
    }

    void ResizeObject(Transform obj, ScenarioCursorData oldCursor, ScenarioCursorData newCursor)
    {
        if (!obj.TryGetComponent<IScenarioEditorResizable>(out var resize))
            return;

        //compute mouse delta in world space

        var vectorAxisGlobalSpace = obj.transform.TransformDirection(VectorAxis).normalized;
        //var gizmoPlane = new Plane(Vector3.up, transform.position);
        var gizmoPlane = ComputeGizmoPlane(vectorAxisGlobalSpace, newCursor.SceneRay, transform.position);
        //var gizmoPlane = ComputeGizmoPlane(vectorAxisGlobalSpace, Camera.main.transform.up, transform.position);
        //var gizmoPlane = new Plane(newCursor.SceneRay.direction * -1.0f, obj.transform.position);

        if (!RaycastGizmoPlane(gizmoPlane, newCursor.SceneRay, out var newPt))
            return;
        if (!RaycastGizmoPlane(gizmoPlane, oldCursor.SceneRay, out var oldPt))
            return;

        var delta = newPt - oldPt;
        //var delta = newPt - transform.position;
        //delta.Scale(vectorAxisGlobalSpace);

        //keep only the component of delta in the direction of the vector axis
        delta = Vector3.Project(delta, vectorAxisGlobalSpace);

        
        Debug.Log($"Delta: {delta} {delta.magnitude}");


        //convert to local space to compute size change
        delta = obj.transform.InverseTransformDirection(delta);

        //correct sign
        delta.Scale(VectorAxis);

        var newSize = resize.Size + delta;
        if (newSize.x < 0.1f)
            newSize.x = 0.1f;
        if (newSize.y < 0.1f)
            newSize.y = 0.1f;
        if (newSize.z < 0.1f)
            newSize.z = 0.1f;

        //resize.Size = newSize;

        //account for local origin location
        var relCenter = resize.LocalCenter.ComponentDivide(resize.Size);

        //vector axis (-1 ... 1) needs to be shifted so the delta is proportional to where the local center (0 ... 1) is
        var scaleFactor = (VectorAxis * 0.5f) - relCenter;

        //scale proportional to relative center
        delta.Scale(scaleFactor);

        //convert delta back to global space
        delta = obj.transform.TransformDirection(delta);

        //delta.Scale(new Vector3(0.5f, 1.0f, 0.5f));
        //delta = delta / 2.0f;
        resize.SetSize(newSize, obj.transform.position + delta);
    }

    private Plane ComputeGizmoPlane(Vector3 axis, Vector3 camUp, Vector3 ptOnPlane)
    {
        var normal = Vector3.Cross(axis, camUp);

        return new Plane(normal, ptOnPlane);
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

    void ScaleObject(Transform obj, ScenarioCursorData oldCursor, ScenarioCursorData newCursor)
    {
        var localAxis = obj.TransformDirection(VectorAxis);
        var gizmoPlane = ComputeGizmoPlane(localAxis, newCursor.SceneRay, transform.position);

        if (!RaycastGizmoPlane(gizmoPlane, newCursor.SceneRay, out var newPt))
            return;
        if (!RaycastGizmoPlane(gizmoPlane, oldCursor.SceneRay, out var oldPt))
            return;

        oldPt = obj.InverseTransformPoint(oldPt);
        newPt = obj.InverseTransformPoint(newPt);

        var delta = newPt - oldPt;
        delta.Scale(VectorAxis);

        //oldPt.Scale(VectorAxis);
        //newPt.Scale(VectorAxis);

        var scale = obj.localScale;
        delta.x = newPt.x / oldPt.x * scale.x - scale.x;
        delta.y = newPt.y / oldPt.y * scale.y - scale.y;
        delta.z = newPt.z / oldPt.z * scale.z - scale.z;
        delta.Scale(VectorAxis);

        //delta.Scale(new Vector3(2.0f, 1.0f, 2.0f));

        scale = obj.localScale + delta;
        scale = scale.ClampComponents(0.1f, Mathf.Infinity);

        obj.localScale = scale;


        //Vector3 mousePos = Vector3.zero;
        //Vector3 mouseDelta = Vector3.zero;
        //Vector3 newScale = Vector3.zero;

        //Vector3 mosInput = Input.mousePosition;
        //Vector3 mouseYOffset = mosInput - prevMosPosY;
        //Vector3 startObjScale = selectedObject.transform.localScale;


        //ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        //{
        //    mousePos = hit.point;
        //    mouseDelta = mousePos - previousMousePos;
        //}

        //Vector3 oldPos = selectedObject.transform.localScale;
        //switch (gizmo.GetAxis())
        //{
        //    case GizmoAxis.X:
        //        newScale = selectedObject.transform.localScale + mouseDelta;
        //        selectedObject.transform.localScale = new Vector3(newScale.x, oldPos.y, oldPos.z);
        //        break;
        //    case GizmoAxis.Y:
        //        selectedObject.transform.localScale = startObjScale + (mouseYOffset.y * selectedObject.transform.up) * Time.deltaTime;
        //        break;
        //    case GizmoAxis.Z:
        //        newScale = selectedObject.transform.localScale + mouseDelta;
        //        selectedObject.transform.localScale = new Vector3(oldPos.x, oldPos.y, newScale.z);
        //        break;
        //    case GizmoAxis.XYZ:
        //        float changeInDelta = Mathf.Abs(mouseDelta.x) > Mathf.Abs(mouseDelta.z) ? mouseDelta.x : mouseDelta.z;
        //        newScale = selectedObject.transform.localScale + new Vector3(changeInDelta, changeInDelta, changeInDelta);
        //        selectedObject.transform.localScale = newScale;
        //        break;
        //}

        //Vector3 clampedScale = new Vector3(Mathf.Clamp(selectedObject.transform.localScale.x, .1f, 500),
        //                                   Mathf.Clamp(selectedObject.transform.localScale.y, .1f, 500),
        //                                   Mathf.Clamp(selectedObject.transform.localScale.z, .1f, 500));
        //selectedObject.transform.localScale = clampedScale;

        //previousMousePos = mousePos;
        //prevMosPosY = mosInput;
    }

    void RotateObject(Transform obj, Ray oldRay, Ray newRay)
    {

        Vector3 axis = Vector3.up;

        switch (Axis)
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
            var worldAxis = obj.TransformDirection(axis);
            var rot = ComputeObjectRotation(Camera.main, obj.position, worldAxis, axis, oldRay, newRay);
            //selectedObject.transform.Rotate(axis, rotateAmount, Space.Self);
            obj.localRotation *= rot;
        }

        transform.rotation = obj.rotation;        
    }


    private Quaternion ComputeObjectRotation(Camera camera, Vector3 pos, Vector3 axis, Vector3 localAxis, 
        Ray prevRay, Ray currentRay)
    {
        var plane = new Plane(axis, pos);

        //var prevRay = camera.ScreenPointToRay(prevMosPosY);
        //var currentRay = camera.ScreenPointToRay(Input.mousePosition);
        if (!plane.Raycast(currentRay, out var dist))
            return Quaternion.identity;
        var p1 = currentRay.origin + currentRay.direction * dist;

        if (!plane.Raycast(prevRay, out dist))
            return Quaternion.identity;

        var p2 = prevRay.origin + prevRay.direction * dist;
        var v1 = pos - p1;
        var v2 = pos - p2;

        var xproduct = Vector3.Cross(v2, v1);

        var angle = Vector3.Angle(v1, v2);
        if (Vector3.Dot(xproduct, axis) < 0)
            angle *= -1.0f;

        return Quaternion.AngleAxis(angle, localAxis);
    }

}
