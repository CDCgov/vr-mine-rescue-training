using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NIOSH_EditorLayers;


[System.Obsolete]
[RequireComponent(typeof(DragDropUIBehaviour))]
public class HierarchyItem : MonoBehaviour//, ISelectHandler
{
    /*
    //public HierarchyContainer.HierarchyTag parentTag;
    [HideInInspector]public RectTransform rt;
    protected Transform root;
    public GameObject prefabInScene;
    public GameObject prefabToLoad;
    protected DragDropUIBehaviour dragDrop;
    public HierarchyContainer currentContainer, lastContainer, nextContainer;
    protected int nextIndex;
    protected bool dragging;
    protected GameObject snapPreview;
    protected Image snapPreviewImage;
    protected RectTransform snapPreviewRt;
    [SerializeField] protected Vector3 snapPreviewSize;
    [SerializeField] protected float snapPreviewYOffset;
    protected HierarchyContainer myContainer;
    protected Placer scenePlacer;
    public LayerManager.EditorLayer editorLayer;
    TextMeshProUGUI tmp;
    [SerializeField] Vector2 pivotOnDrop;
    [SerializeField] bool reposition;
    [SerializeField] float repositionX;



    #region Initialization
    
    public virtual void InitializeFromPlacedObject(GameObject _prefabInScene, HierarchyContainer container)
    {
       // //Debug.Log("InitializeFromPlacedObject");
        if (_prefabInScene)
        {
            prefabInScene = _prefabInScene;
            SetDisplayName(prefabInScene);
        }

        currentContainer = container;
        rt = GetComponent<RectTransform>();
        rt.SetPivot(pivotOnDrop);
        Initialize();
    }

    public virtual void InitializeFromPrefab(GameObject _prefabToLoad, HierarchyContainer container)
    {
        if (_prefabToLoad)
        {
            prefabToLoad = _prefabToLoad;
            prefabInScene = Instantiate(prefabToLoad);
            prefabInScene.transform.position = new Vector3(0, -500, 0);
            prefabInScene.name = prefabToLoad.name;
            if (!prefabInScene.TryGetComponent<PlacablePrefab>(out var placeable))
                prefabInScene.AddComponent<PlacablePrefab>();

            prefabInScene.SetActive(true);
            SetDisplayName(prefabToLoad);
        }
        currentContainer = container;
        //lastContainer = container;
        Initialize();
    }

    public void Initialize()
    {
        scenePlacer = FindObjectOfType<Placer>();
        snapPreview = GameObject.Find("SnapPreview");
        snapPreviewImage = snapPreview.GetComponent<Image>();
        snapPreviewRt = snapPreview.GetComponent<RectTransform>();
        rt = GetComponent<RectTransform>();

        dragDrop = GetComponent<DragDropUIBehaviour>();
        VerticalLayoutGroup layout = GetComponentInParent<VerticalLayoutGroup>();
        root = layout.GetComponent<RectTransform>();
        if (LayerManager.Instance)
        {
            LayerManager.Instance.layerChanged += OnLayerChanged;
        }

        myContainer = GetComponent<HierarchyContainer>();
        dragDrop.onObjectGrabbed += Grabbed;
        dragDrop.onObjectDropped += Dropped;
        snapPreviewSize = new Vector2(300, 5);
        snapPreviewYOffset = -10;
        ObjectInfo info = prefabInScene.GetComponent<ObjectInfo>();
        if (info)
        {
            //info.hierarchyObjectInScene = this.gameObject;
            info.HierarchyItem = this;
            if (TryGetComponent(out HierarchyContainer container)) info.hierarchyContainer = container;
            editorLayer = info.editorLayer;
        }
        if (myContainer) myContainer.Initialize();
    }
    
    #endregion Initialization

    void OnLayerChanged(LayerManager.EditorLayer newLayer)
    {
        Selectable selectable = GetComponent<Selectable>();
        selectable.interactable = (editorLayer == newLayer);

    }
    protected void Grabbed()
    {
        Moving();
    }

    protected void Dropped()
    {
        Placed(true);
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (!dragging) return;
        
        
        HierarchyItem _nextSibling = other.GetComponent<HierarchyItem>();
        HierarchyContainer _nextContainer = other.GetComponent<HierarchyContainer>();


        if (_nextSibling && _nextSibling.parentTag == parentTag)
        {
            nextContainer = _nextSibling.lastContainer;
            nextIndex = _nextSibling.rt.GetSiblingIndex() + 1;
            //Debug.Log("parent from sibling +" + _nextSibling.gameObject);
            ControlSnapPreview(_nextSibling.rt);
        }
        else if(_nextContainer && _nextContainer.hierarchyTag == parentTag)
        {
            nextContainer = _nextContainer;
            nextIndex = 0;

            //Debug.Log("parent from container +" + _nextContainer.gameObject);
            ControlSnapPreview(_nextContainer.rt);
        }

    }
    
    protected void OnTriggerExit2D(Collider2D other)
    {
        if (!dragging) return;

        if (nextContainer != null && nextContainer.TryGetComponent(out Collider2D c) && c == other)
        {
            nextContainer = null;
            nextIndex = 0;
            snapPreviewImage.enabled = false;
        }
    }

    protected void ControlSnapPreview(RectTransform targetRt)
    {
        snapPreviewRt.position = new Vector2(targetRt.position.x,targetRt.position.y + snapPreviewYOffset);
        snapPreviewRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, snapPreviewSize.x);
        snapPreviewRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, snapPreviewSize.y);
        snapPreviewImage.enabled = true;
    }
    
    public void SetDisplayName(GameObject prefab)
   {

        tmp = GetComponentInChildren<TextMeshProUGUI>();
        ObjectInfo info = prefab.GetComponent<ObjectInfo>();
        
        if (info)
        {
            //string _displayName = info.displayName;
            //tmp.text = _displayName;
            string name = info.UserSuppliedName;
            if (string.IsNullOrEmpty(name) || name == "")
            {
                //string assetWindowName = LoadableAssetCollection.Get
                if (string.IsNullOrEmpty(info.AssetWindowName))
                {
                    Debug.Log($"In hierarchy item, it thinks user supplied name and asset window name are null: {info.AssetWindowName}");
                    name = info.DisplayName;
                }
                else
                {
                    name = info.AssetWindowName;
                }
            }
            string _displayName = name;
            tmp.text = _displayName;

        }
   }

    public void OnSelect(BaseEventData eventData)
    {
        scenePlacer.SelectObject(prefabInScene);
    }
    public void StartDestroy()
    {
        //rt.parent = null;
        if (lastContainer != null) lastContainer.RemoveItemFromContainer(this);
        Destroy(this.gameObject);
        ////Debug.Log("StartDestroy");
    }
    protected void OnDestroy()
    {
        dragDrop.onObjectGrabbed -= Grabbed;
        dragDrop.onObjectDropped -= Dropped;
        if(LayerManager.Instance)LayerManager.Instance.layerChanged -= OnLayerChanged;
        ////Debug.Log("Destroy");
    }

    public void Moving()
    {
        if (currentContainer && currentContainer.contentRt != root)
        {
            lastContainer = currentContainer;
            currentContainer = null;
        }

        dragging = true;
        nextIndex = 0;
    }
    public void Placed(bool repositionPrefab)
    {
        
        dragging = false;

        if (nextContainer)
        {

            //currentContainer = nextContainer;
            nextContainer.AddItemToContainer(this, nextIndex, repositionPrefab);
        }
        
        else if (lastContainer != null)
        {
            //currentContainer = lastContainer;
            lastContainer.AddItemToContainer(this, nextIndex, repositionPrefab);
        }
        // no last container, destroy
        else if (currentContainer.contentRt == root)
        {
            if (prefabInScene != null) Destroy(prefabInScene);
            else Destroy(this.gameObject);
        }

        snapPreviewImage.enabled = false;
        rt.SetPivot(pivotOnDrop);
        if (reposition) rt.anchoredPosition = new Vector2(repositionX, rt.anchoredPosition.y);
    }

    */
}
