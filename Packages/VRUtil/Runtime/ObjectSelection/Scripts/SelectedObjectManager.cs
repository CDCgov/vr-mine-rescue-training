using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedObjectManager : SceneManagerBase
{
    public static SelectedObjectManager GetDefault(GameObject self)
    {
        return self.GetDefaultManager<SelectedObjectManager>("SelectedObjectManager");
    }

    public GameObject CornerHighlightPrefab;

    public GameObject SelectedObject
    {
        get { return _selectedObject; }
        set { SetSelectedObject(value); }
    }

    public event Action<GameObject> SelectionChanged;

    private GameObject _selectedObject;
    private GameObject _highlightObj;
    private Transform[] _highlightCorners;
    private Renderer[] _renderers;
    private bool _objectSelected = false;

    private void Start()
    {
        Util.DontDestroyOnLoad(gameObject);

        if (CornerHighlightPrefab == null)
            CornerHighlightPrefab = Resources.Load<GameObject>("CornerSelectionBracket");
    }

    private void LateUpdate()
    {
        if (!_objectSelected)
            return;

        if (_selectedObject == null)
        {
            SetSelectedObject(null);
            return;
        }

        RepositionCorners();
    }

    public void SetSelectedObject(GameObject obj)
    {
        ClearSelectionHighlight();

        _selectedObject = obj;
        if (obj != null)
            _objectSelected = true;
        else
            _objectSelected = false;

        ShowSelectionHighlight(obj);
        RaiseSelectionChanged(obj);
    }

    public SelectableObject GetSelectableObjectComponent()
    {
        if (_selectedObject == null)
            return null;

        return _selectedObject.GetComponent<SelectableObject>();
    }

    public void GetSelectedObjectInfo(StringBuilder sb)
    {
        if (_selectedObject == null)
            return;

        var infoComponents = _selectedObject.GetComponents<ISelectableObjectInfo>();
        foreach (var comp in infoComponents)
        {
            comp.GetObjectInfo(sb);
        }
    }

    public IEnumerable<ISelectableObjectAction> GetSelectedObjectActions()
    {
        if (_selectedObject == null)
            yield break;

        var actions = _selectedObject.GetComponents<ISelectableObjectAction>();
        foreach (var action in actions)
        {
            yield return action;
        }
    }

    private void ShowSelectionHighlight(GameObject obj)
    {
        if (CornerHighlightPrefab == null || obj == null)
            return;

        Debug.Log($"SelectedObjectManager: Showing highlight for {obj.name}");


        _highlightObj = new GameObject("ObjectHighlight");
        var parent = _highlightObj.transform;

        _renderers = obj.GetComponentsInChildren<Renderer>();

        if (_highlightCorners == null)
            _highlightCorners = new Transform[8];

        for (int i = 0; i < _highlightCorners.Length; i++)
        {
            _highlightCorners[i] = CreateCorner(parent).transform;
        }

        RepositionCorners();

        //CreateCorner(parent, min, Quaternion.identity);
        //CreateCorner(parent, new Vector3(max.x, min.y, min.z), Quaternion.Euler(0, -90, 0));
        //CreateCorner(parent, new Vector3(max.x, min.y, max.z), Quaternion.Euler(0, 180, 0));
        //CreateCorner(parent, new Vector3(min.x, min.y, max.z), Quaternion.Euler(0, 90, 0));

        //CreateCorner(parent, new Vector3(min.x, max.y, min.z), Quaternion.Euler(0, 90,180));
        //CreateCorner(parent, new Vector3(max.x, max.y, min.z), Quaternion.Euler(0, 0, 180));
        //CreateCorner(parent, new Vector3(max.x, max.y, max.z), Quaternion.Euler(0, -90, 180));
        //CreateCorner(parent, new Vector3(min.x, max.y, max.z), Quaternion.Euler(0, 180, 180));
    }

    private void RepositionCorners()
    {
        if (_highlightCorners == null || _highlightCorners.Length != 8)
            return;

        if (_renderers == null || _renderers.Length <= 0)
            return;


        Bounds b = _renderers[0].bounds;
        foreach (var r in _renderers)
        {
            if (r == null || r.gameObject == null || !r.enabled || !r.gameObject.activeInHierarchy)
                continue;

            if (r is MeshRenderer || r is SkinnedMeshRenderer)
                b.Encapsulate(r.bounds);
        }

        var min = b.min;
        var max = b.max;

        UpdateCornerPosition(_highlightCorners[0], new Vector3(min.x, min.y, min.z), Quaternion.identity);
        UpdateCornerPosition(_highlightCorners[1], new Vector3(max.x, min.y, min.z), Quaternion.Euler(0, -90, 0));
        UpdateCornerPosition(_highlightCorners[2], new Vector3(max.x, min.y, max.z), Quaternion.Euler(0, 180, 0));
        UpdateCornerPosition(_highlightCorners[3], new Vector3(min.x, min.y, max.z), Quaternion.Euler(0, 90, 0));

        UpdateCornerPosition(_highlightCorners[4], new Vector3(min.x, max.y, min.z), Quaternion.Euler(0, 90, 180));
        UpdateCornerPosition(_highlightCorners[5], new Vector3(max.x, max.y, min.z), Quaternion.Euler(0, 0, 180));
        UpdateCornerPosition(_highlightCorners[6], new Vector3(max.x, max.y, max.z), Quaternion.Euler(0, -90, 180));
        UpdateCornerPosition(_highlightCorners[7], new Vector3(min.x, max.y, max.z), Quaternion.Euler(0, 180, 180));
    }

    private GameObject CreateCorner(Transform parent)
    {
        var c = Instantiate<GameObject>(CornerHighlightPrefab);
        c.transform.SetParent(parent, false);
        //c.transform.position = pos;
        //c.transform.localRotation = rot;
        c.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        return c;
    }

    private void UpdateCornerPosition(Transform corner, Vector3 pos, Quaternion rot)
    {
        corner.transform.position = pos;
        corner.transform.localRotation = rot;
    }

    private void ClearSelectionHighlight()
    {
        if (_highlightCorners != null)
        {
            for (int i = 0; i < _highlightCorners.Length; i++)
                _highlightCorners[i] = null;
        }

        _renderers = null;

        if (_highlightObj == null)
            return;

        Destroy(_highlightObj);
    }

    private void RaiseSelectionChanged(GameObject obj)
    {
        try
        {
            SelectionChanged?.Invoke(obj);
        }
        catch (Exception ex)
        {
            Debug.LogError($"SelectedObjectManager: Error in selection changed handler {ex.Message} {ex.StackTrace}");
        }
    }
}
