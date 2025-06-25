using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using NIOSH_EditorLayers;
using static NIOSH_EditorLayers.LayerManager;
using NIOSH_MineCreation;

public class AssetUIObject : MonoBehaviour
{
    public LoadableAssetManager LoadableAssetManager;
    public LayerMask layerMask;
    public LoadableAsset Asset;
    //public GameObject prefabToLoad;                // The gameobject to spawn when we are over the scene ( TODO will be done at runtime eventually)
    public GameObject _hierarchyObj;

    private DragDropUIBehaviour dragDropUIBehaviour; // Drag/drop logic associated with this object
    private CanvasGroup uiImageCanvasGroup;          //  Canvas group used for hiding and showing this UI element  
    private GameObject prefabInScene;                // Reference to a spawned gameobject in the scene, in case we need to delete it if we go back off the scene
    private bool isOverScene = false;                // Are we over the scene or over the UI?
    private bool isOverHierarchy = false;
    private bool gateIsOverHierarchy = false;
    private GraphicRaycaster m_Raycaster;            // TODO need to offload the raycasting stuff to a seperate script                                          // That this script queries for raycast results rather than having every UI element deal with it
    private PointerEventData m_PointerEventData;
    private EventSystem m_EventSystem;
    private Placer _scenePlacer;
    private Transform assetContainer;
    private PlacablePrefab spawnedPrefab;
    private Ray ray;
    private RaycastHit hit; // TODO make a unique raycasting function so each one doesn't have its own raycaster
    //private HierarchyContainer hierarchyRoot;
    //private ObjectInfo objectInfo;
    private MenuTooltip _menuTooltip;
    private bool _isCable = false;
    private string _assetWindowName;

    public void Initialize(LoadableAsset asset, Sprite icon, string name, string tooltip, Placer placer, EventSystem eventSystem, GraphicRaycaster graphicRaycaster, GameObject hierarchyObj)
    {
        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);

        //_scenePlacer = FindObjectOfType<Placer>();
        //m_EventSystem = FindObjectOfType<EventSystem>();
        //m_Raycaster = FindObjectOfType<GraphicRaycaster>();
        //_hierarchyObj = GameObject.Find("HierarchyWindow");

        _scenePlacer = placer;
        m_EventSystem = eventSystem;
        m_Raycaster = graphicRaycaster;
        _hierarchyObj = hierarchyObj;

        Asset = asset;        
        assetContainer = _scenePlacer.assetContainer;
        
        //prefabToLoad = prefab;
        //objectInfo = prefabToLoad.GetComponent<ObjectInfo>();
        SetName(name);
        _assetWindowName = name;
        
        //hierarchyRoot = _hierarchyObj.GetComponent<HierarchyContainer>();        
        dragDropUIBehaviour = GetComponent<DragDropUIBehaviour>();

        //Set Tooltip info
        _menuTooltip = GetComponent<MenuTooltip>();
        _menuTooltip.SetTooltipText(tooltip);
        if (string.IsNullOrEmpty(tooltip))
        {
            _menuTooltip.enabled = false;
        }

        uiImageCanvasGroup = dragDropUIBehaviour.gameObject.GetComponent<CanvasGroup>();
        dragDropUIBehaviour.onObjectGrabbed += ObjectGrabbed;
        dragDropUIBehaviour.onObjectDropped += ObjectDropped;
        dragDropUIBehaviour.onObjectMoved += ObjectMoved;
        AddImageToUIAsset(icon);
    }

    void AddImageToUIAsset(Sprite sprite)
    {
        if (sprite == null) { Debug.Log("SPRITE NOT LOADED"); return; }
        this.GetComponent<Image>().sprite = sprite;
    }

    private void OnDestroy()
    {
        if(dragDropUIBehaviour == null)
        {
            dragDropUIBehaviour = GetComponent<DragDropUIBehaviour>();
        }

        dragDropUIBehaviour.onObjectGrabbed -= ObjectGrabbed;
        dragDropUIBehaviour.onObjectDropped -= ObjectDropped;
        dragDropUIBehaviour.onObjectMoved -= ObjectMoved;
    }

    void ObjectGrabbed()
    {
        //OverridePlacementType();
        //scenePlacer.SelectSpecificGameObject(prefabInScene);
    }

    void ObjectDropped()
    {
        isOverScene = false;

        uiImageCanvasGroup.alpha = 1f;

        if (prefabInScene)
        {

            //if (objectInfo.editorLayer == EditorLayer.Cables)
            //{
            //    if (objectInfo.TryGetComponent(out SphereCollider collider))
            //    {
            //        collider.enabled = false;
            //        prefabInScene.transform.rotation = Quaternion.identity;
            //        prefabInScene.transform.localEulerAngles = Vector3.zero;
            //    }

            //}

            _scenePlacer.SelectObject(prefabInScene);
            prefabInScene = null;
            spawnedPrefab = null;
        }
        // deactivate stuff
    }

    void ObjectMoved()
    {
        m_PointerEventData = new PointerEventData(m_EventSystem);
        //Set the Pointer Event Position to that of the mouse position
        m_PointerEventData.position = Input.mousePosition;

        //Create a list of Raycast Results
        List<RaycastResult> results = new List<RaycastResult>();

        //Raycast using the Graphics Raycaster and mouse click position
        m_Raycaster.Raycast(m_PointerEventData, results);
        
        if (results.Count <= 0)
        {
            if (!isOverScene)
            {
                uiImageCanvasGroup.alpha = 0f;
                isOverScene = true;

                var cursorData = _scenePlacer.CurrentCursor;
                //if (LayerManager.GetCurrentLayer() == LayerManager.EditorLayer.Cables)
                //    prefabInScene = GameObject.Instantiate(prefabToLoad, Vector3.zero, Quaternion.identity, assetContainer);
                //else
                //    prefabInScene = GameObject.Instantiate(prefabToLoad, cursorData.GroundPos, Quaternion.identity, assetContainer);

                prefabInScene = LoadableAssetManager.InstantiateEditorAsset(Asset.AssetID, cursorData.GroundPos, Quaternion.identity, assetContainer);
                var objectInfo = prefabInScene.GetComponent<ObjectInfo>();

                //temporary fix to get the resizable stopping to spawn door before updating the inspector
                //if (prefabInScene.TryGetComponent<ResizableStopping>(out var resizableStopping))
                //{
                //    resizableStopping.ResizeStopping();
                //}

                //if (!string.IsNullOrEmpty(objectInfo.AssetWindowName) && objectInfo.AssetWindowName != "")
                //{
                //    prefabInScene.name = objectInfo.AssetWindowName;
                //    if (string.IsNullOrEmpty(objectInfo.UserSuppliedName))
                //    {
                //        objectInfo.UserSuppliedName = objectInfo.AssetWindowName;
                //    }
                //}
                //else
                //{
                //    prefabInScene.name = Asset.AssetWindowName;
                //}

                if (!prefabInScene.TryGetComponent<PlacablePrefab>(out spawnedPrefab))
                {
                    spawnedPrefab = prefabInScene.AddComponent<PlacablePrefab>();
                }

                if (prefabInScene.gameObject.name == "VentilationLayerNode")
                {
                    spawnedPrefab.SetIgnoreSave(true);
                }

                if (prefabInScene.TryGetComponent<MineLayerTile>(out var mineLayerTile))
                {
                    mineLayerTile.ChangeModeToEdit(false);
                    mineLayerTile.ScaleToSettings();
                }

                prefabInScene.SetActive(true);
                _scenePlacer.SelectObject(prefabInScene);
                _scenePlacer.SwitchActiveGizmos(GizmoKind.None, temporary:true);
                //if (LayerManager.GetCurrentLayer() == LayerManager.EditorLayer.Mine)
                _scenePlacer.StartMouseDragFromOffScreen();
                //hierarchyRoot.CreateItemFromPlacedObject(prefabInScene, 0, true);

                _scenePlacer.RaiseSceneObjectListChanged();
            }
        }
        else
        {
            isOverHierarchy = false;

            foreach (RaycastResult r in results)
            {
                if (r.gameObject == _hierarchyObj) isOverHierarchy = true;
            }

            if (isOverHierarchy)
            {
                if (gateIsOverHierarchy)
                {
                    // create item for hierarchy 
                    HierarchyItem hierarchyItem = null;
                    //hierarchyRoot.CreateItemFromPrefab(prefabToLoad, out hierarchyItem);

                    //drag hierarchy item using existing drag event
                    DragDropUIBehaviour childDrag = hierarchyItem.GetComponent<DragDropUIBehaviour>();
                    dragDropUIBehaviour.StartDragOther(childDrag, m_PointerEventData);
                    gateIsOverHierarchy = false;
                }
            }
            else gateIsOverHierarchy = true;
            
            if (isOverScene)
            {
                uiImageCanvasGroup.alpha = 1f;
                isOverScene = false;
                //scenePlacer.DeselectSpecificGameObject(prefabInScene);
                //scenePlacer.DeselectObject();
                //Destroy(prefabInScene);
                _scenePlacer.DestroySelectedObject();
                // TODO pooling generic objects?
            }
        }

        if (prefabInScene && LayerManager.GetCurrentLayer() == LayerManager.EditorLayer.Cables)
        {
            if (!prefabInScene.activeSelf) 
            { 
                prefabInScene.SetActive(true); 
            }

            //prefabInScene.transform.position = Input.mousePosition;

            PlacablePrefab placeable = spawnedPrefab;

            // get placable script
            // put where hit is based on pivot
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                Vector3 placementVector = hit.point;

                //Transform placementTarget = hit.transform;
                //var mineTile = placementTarget.GetComponentInParent<MineLayerTile>();
                //if (mineTile != null) placementTarget = mineTile.transform;

                //switch (placeable.GetPlacementMode())
                //{
                //    case PlacementMode.Anchor:
                //        placementVector = hit.point - (placeable.GetAnchor().transform.position - placeable.transform.position);
                //        break;
                //    case PlacementMode.Collider:
                //    case PlacementMode.Pivot:
                //        placementVector = hit.point;
                //        break;
                //}

                placeable.transform.position = placementVector;
                placeable.transform.localRotation = Quaternion.identity;
            }
        }
        //else if(prefabInScene && objectInfo.PlacementTypeOverride == PlacementTypeOverride.SnapCurtain)
        //{
        //    spawnedPrefab = prefabInScene.GetComponent<PlacablePrefab>();
        //    var snapPlacer = scenePlacer.activeLogic as SnapCurtainPlacerLogic;
        //    if(spawnedPrefab != null && snapPlacer != null) snapPlacer.MouseDrag(spawnedPrefab);
        //}
    }

    void SetName(string assetName)
    {
        string name = assetName;
        if (name != null) GetComponentInChildren<TextMeshProUGUI>().text = name;
        else GetComponentInChildren<TextMeshProUGUI>().text = "Missing Name Info";
    }

    public string GetName()
    {
        return _assetWindowName;
    }

    //void OverridePlacementType()
    //{
    //    Debug.Log("Override placement : " + objectInfo.PlacementTypeOverride);
        
    //    if (scenePlacer != null)
    //    {
    //        scenePlacer.OverrideManipulationLogic(objectInfo.PlacementTypeOverride);
    //        PlacablePrefab placeable = GetComponent<PlacablePrefab>();
    //        if (placeable == null) { placeable = GetComponentInParent<PlacablePrefab>(); }
    //        if (placeable != null) { scenePlacer.activeLogic.selectedObject = placeable; }
    //    }
    //}
}
