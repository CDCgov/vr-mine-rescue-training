using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GizmoSelectionBox : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] float cornerScaleModifier = 1;
    [SerializeField] float lineScaleModifier = .01f;
    [SerializeField] Transform corner_rightTopFront;
    [SerializeField] Transform corner_rightTopBack;
    [SerializeField] Transform corner_leftTopFront;
    [SerializeField] Transform corner_leftTopBack;
    [SerializeField] Transform corner_rightBottomFront;
    [SerializeField] Transform corner_rightBottomBack;
    [SerializeField] Transform corner_leftBottomFront;
    [SerializeField] Transform corner_leftBottomBack;
    
    Placer scenePlacer;
    GameObject cornerContainer;
    //BoxCollider targetCollider;

    private GameObject _target;
    private List<Collider> _targetColliders;

    private void Awake()
    {
        _targetColliders = new List<Collider>();

    }

    void Start()
    {
        cornerContainer = corner_rightTopFront.parent.gameObject;
        scenePlacer = FindObjectOfType<Placer>();
        //scenePlacer.onObjectSelected += NewTarget;
        //scenePlacer.onObjectDeselected += DisableCorners;
        scenePlacer.SelectedObjectChanged += OnSelectedObjectChanged;
        DisableCorners();
    }

    private void OnSelectedObjectChanged(GameObject obj)
    {
        PlacablePrefab placeable = null;
        if (obj != null)
            obj.TryGetComponent<PlacablePrefab>(out placeable);

        if (obj == null || (placeable != null && placeable.ShowSelectionBox == false))
            DisableCorners();
        else
            NewTarget(obj);
    }

    private void Update()
    {
        UpdateSelectionBox();
        ///object will move, so reposition in update
        //if (targetCollider != null) 
        //{
        //        SetToMeshBounds(); 
        //}
        //else if (cornerContainer.activeInHierarchy) cornerContainer.SetActive(false);
    }

    /// <summary>
    /// Assign new target
    /// </summary>
    /// <param name="obj"></param>
    void NewTarget(GameObject obj)
    {
        GameObject target = obj;
        if (obj.name.Equals("StaticGasZoneEditorOnly"))
        {
            Transform t = obj.transform.Find("Core");
            if (t != null) target = t.gameObject;
            
        }

        _target = obj;
        _targetColliders.Clear();
        _target.GetComponentsInChildren<Collider>(_targetColliders);

        UpdateSelectionBox();
        //if (target.TryGetComponent(out BoxCollider box))
        //{
        //    targetCollider = box;
        //    SetScale();
        //}
        //else
        //{
        //    Debug.Log("Selected Object is missing a Box Collider component necessary to display selection box");
        //    targetCollider = null;
        //}
    }

    void UpdateSelectionBox()
    {
        if (_target == null)
            return;

        if (GetTargetBounds(out var bounds))
        {
            SetScale(bounds);
            SetToMeshBounds(bounds);
        }
        else if (cornerContainer.activeInHierarchy) 
        {
            cornerContainer.SetActive(false);
        }
    }

    private bool GetTargetBounds(out Bounds bounds)
    {
        bounds = default(Bounds);
        if (_target == null || _targetColliders == null || _targetColliders.Count <= 0)
            return false;

        if (_targetColliders[0] == null)
            return false;

        bounds = _targetColliders[0].bounds;

        for (int i = 1; i < _targetColliders.Count; i++)
        {
            if (_targetColliders[i] == null)
                continue;

            bounds.Encapsulate(_targetColliders[i].bounds);
        }

        return true;
    }

    /// <summary>
    /// Set the scale of cornerns and line thickness to match the size of the object collider
    /// </summary>
    void SetScale(Bounds bounds)
    {
        //Bounds bounds = targetCollider.bounds;

        ///Calculate new scale from target size
        float smallestExtent = Mathf.Min(bounds.extents.x, bounds.extents.y, bounds.extents.z);
        float newScale = smallestExtent * cornerScaleModifier;
        Vector3 cornerScale = new Vector3(newScale, newScale, newScale);

        ///Set scale for each corner
        corner_rightTopFront.localScale = cornerScale;
        corner_rightTopBack.localScale = cornerScale;
        corner_leftTopFront.localScale = cornerScale;
        corner_leftTopBack.localScale = cornerScale;
        corner_rightBottomFront.localScale = cornerScale;
        corner_rightBottomBack.localScale = cornerScale;
        corner_leftBottomFront.localScale = cornerScale;
        corner_leftBottomBack.localScale = cornerScale;

        ///Set line thickness 
        float lineScale = smallestExtent * lineScaleModifier;
        lineRenderer.startWidth = lineScale;
        lineRenderer.endWidth = lineScale;
    }

    /// <summary>
    /// Set positioning of mesh corners and line corners to match the target collider bounds
    /// </summary>
    void SetToMeshBounds(Bounds bounds)
    {
        //if (targetCollider == null)
        //{
        //    //Debug.Log("Selected Object is missing a Box Collider component necessary to display selection box");
        //    return;
        //}
        cornerContainer.SetActive(true);

        ///Calculate new positions
        //Bounds bounds = targetCollider.bounds;
        float top = bounds.max.y;
        float bottom = bounds.min.y;
        float right = bounds.max.x;
        float left = bounds.min.x;
        float front = bounds.max.z;
        float back = bounds.min.z;

        ///Set positions of corners
        corner_rightTopFront.position = new Vector3(right,top,front);
        corner_rightTopBack.position = new Vector3(right, top, back);
        corner_leftTopFront.position = new Vector3(left, top, front);
        corner_leftTopBack.position = new Vector3(left, top, back);
        corner_rightBottomFront.position = new Vector3(right, bottom, front);
        corner_rightBottomBack.position = new Vector3(right, bottom, back);
        corner_leftBottomFront.position = new Vector3(left, bottom, front);
        corner_leftBottomBack.position = new Vector3(left, bottom, back);

        ///Set Line Renderer corners 
        Vector3[] cornerPoints =  new Vector3[]
        {
            new Vector3(left, top, front),
            new Vector3(right, top, front), 
            new Vector3(right, top, back), 

            new Vector3(left, top, back),
            new Vector3(left, top, front),

            new Vector3(left, bottom, front),
            new Vector3(left, bottom, back),
            new Vector3(left, top, back),
            new Vector3(left, bottom, back),

            new Vector3(right, bottom, back),
            new Vector3(right, top, back),
            new Vector3(right, bottom, back),

            new Vector3(right, bottom, front),
            new Vector3(right, top, front),
            new Vector3(right, bottom, front),
            new Vector3(left, bottom, front),

        };
            
        lineRenderer.SetPositions(cornerPoints);
    }
    
    /// <summary>
    /// Turn box off when not in use 
    /// </summary>
    void DisableCorners()
    {
        //targetCollider = null;
        _target = null;
        _targetColliders.Clear();
        cornerContainer.SetActive(false);
    }

}
