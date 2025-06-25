using NIOSH_EditorLayers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIListSceneObjects : LayerControlledClass
{
    public Placer Placer;
    public GameObject ListItemPrefab;

    private System.Diagnostics.Stopwatch _stopwatch;

    private ScrollRect _scrollRect; 
    private List<GameObject> _listItemPool;
    private int _lastSearchIndex;

    private List<ObjectInfo> _tempItemList;

    private new void Start()
    {
        base.Start();

        _tempItemList = new List<ObjectInfo>();
        _listItemPool = new List<GameObject>();
        _lastSearchIndex = 0;

        _stopwatch = new System.Diagnostics.Stopwatch();

        _scrollRect = transform.GetComponentInParent<ScrollRect>();

        foreach (Transform obj in transform)
        {
            obj.gameObject.SetActive(false);
            Destroy(obj.gameObject);
        }

        RebuildList(Placer.CurrentLayer);
        //Placer.SceneObjectListChanged += OnSceneObjectListChanged;
    }

    private void OnEnable()
    {

        if (Placer == null)
            Placer = Placer.GetDefault();

        Placer.SceneObjectListChanged += OnSceneObjectListChanged;
        Placer.SelectedObjectChanged += OnSelectedObjectChanged;
    }

    private void OnDisable()
    {
        if (Placer != null)
        {
            Placer.SceneObjectListChanged -= OnSceneObjectListChanged;
            Placer.SelectedObjectChanged -= OnSelectedObjectChanged;
        }
    }

    private void OnSceneObjectListChanged()
    {
        ClearList();
        RebuildList(Placer.CurrentLayer);
    }

    private void OnSelectedObjectChanged(GameObject selectedObj)
    {
        if (selectedObj == null || _scrollRect == null)
            return;

        //Canvas.ForceUpdateCanvases();

        var contentRect = transform as RectTransform;
        if (contentRect == null)
            return;

        var viewportRect = transform.parent as RectTransform;
        if (viewportRect == null)
            return;

        var height = contentRect.rect.height;
        var viewHeight = viewportRect.rect.height;

        float minView = transform.localPosition.y + 12.0f;
        float maxView = transform.localPosition.y + viewHeight - 12.0f;
        
        foreach (var listItem in _listItemPool)
        {
            if (!listItem.TryGetComponent<UIBtnSelectSceneObject>(out var btn))
                continue;

            if (btn.SceneGameObject == selectedObj)
            {
                //var itemHeight = ((RectTransform)listItem.transform).rect.height;
                //var normalizedPos = 1.0f - Mathf.Abs(((float)listItem.transform.localPosition.y) / height);
                //normalizedPos += normView / 2.0f;
                //_scrollRect.verticalNormalizedPosition = normalizedPos;
                var itemPos = Mathf.Abs(listItem.transform.localPosition.y);

                if (itemPos < minView || itemPos > maxView)
                {
                    Debug.Log($"MinView: {minView:F2} MaxView: {maxView:F2} itemPos: {itemPos:F2}");
                    var pos = transform.localPosition;
                    pos.y = itemPos - viewHeight / 2.0f;
                    transform.localPosition = pos;
                }
                return;
            }
        }
    }

    protected override void OnLayerChanged(LayerManager.EditorLayer newLayer)
    {
        ClearList();
        RebuildList(newLayer);
    }

    private void ClearList()
    {
        foreach (var item in _listItemPool)
        {
            item.SetActive(false);
        }
    }

    private void RebuildList(LayerManager.EditorLayer activeLayer)
    {
        _stopwatch.Reset();
        _stopwatch.Start();

        _tempItemList.Clear();

        foreach (var sceneObj in Placer.AllSceneObjects)
        {
            if (sceneObj == null)
                continue;

            if (sceneObj.PlacementLayer != activeLayer)
                continue;

            if (!sceneObj.TryGetComponent<ObjectInfo>(out var objInfo))
                continue;

            _tempItemList.Add(objInfo);
            //CreateListItem(sceneObj);
        }

        _tempItemList.Sort((a, b) =>
        {
            return string.Compare(a.InstanceName, b.InstanceName);
        });

        foreach (var sceneObj in _tempItemList)
        {
            CreateListItem(sceneObj.GetComponent<PlacablePrefab>());
        }

        _stopwatch.Stop();
        Debug.Log($"UIListSceneObjects: Rebuild took {_stopwatch.ElapsedMilliseconds} ms");
    }

    private GameObject GetAvailableListItem()
    {
        if (_lastSearchIndex >= _listItemPool.Count)
            _lastSearchIndex = 0;

        for (int i = _lastSearchIndex; i < _listItemPool.Count; i++)
        {
            if (!_listItemPool[i].activeSelf)
            {
                _lastSearchIndex = i;
                return _listItemPool[i];
            }
        }

        for (int i = 0; i < _lastSearchIndex; i++)
        {
            if (!_listItemPool[i].activeSelf)
            {
                _lastSearchIndex = i;
                return _listItemPool[i];
            }
        }

        return null;
    }

    private void CreateListItem(PlacablePrefab sceneObj)
    {
        if (ListItemPrefab == null)
            return;

        var obj = GetAvailableListItem();

        if (obj == null)
        {
            obj = Instantiate<GameObject>(ListItemPrefab, transform);
            _listItemPool.Add(obj);
        }
        else
        {
            obj.SetActive(true);
        }

        if (obj.TryGetComponent<UIBtnSelectSceneObject>(out var sceneObjBtn))
        {
            sceneObjBtn.Placer = Placer;
            //sceneObjBtn._sceneObject = sceneObj;
            sceneObjBtn.SetSceneObject(sceneObj);
        }
    }
}
