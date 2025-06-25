using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NIOSH_EditorLayers;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlacableAssetLoader : LayerControlledClass
{
    public LoadableAssetManager LoadableAssetManager;

    public LayerManager.EditorLayer defaultLayer = LayerManager.EditorLayer.Object;
    public GameObject uiObject;
    public GameObject uiRowObject;
    public GameObject uiContentObj;
    public float maxAssetsInRow;

    AssetLoader loader;

    private Placer _placer;
    private EventSystem _eventSystem;
    private GraphicRaycaster _graphicRaycaster;
    private GameObject _hierarchyObj;
    private List<LoadableAsset> _sortedAssets;


    void Awake()
    {
        loader = FindObjectOfType<AssetLoader>();
        _sortedAssets = new List<LoadableAsset>(100);
    }

    public void LoadPlaceableAssetsFromCategory(LayerManager.EditorLayer layer)
    {
        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);

        StopAllCoroutines();
        ClearContent();

        //StartCoroutine(loader.GetAssetsByCategory(layer, CreateUIAssets));

        CreateUIAssets(layer);
    }

    public void LoadPlaceableAssetsFromCategory(LayerManager.EditorLayer layer, LoadableAssetCategories category)
    {
        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);

        StopAllCoroutines();
        ClearContent();

        CreateUIAssets(layer, category);
    }

    void CreateUIAssets(LayerManager.EditorLayer layer)
    {
        CreateUIAssets(layer, LoadableAssetCategories.All);
        //if (LoadableAssetManager == null)
        //    return;

        //foreach(LoadableAsset asset in LoadableAssetManager.GetLoadableAssetsByLayer(layer))
        //{
        //    if (!asset.ShowInAssetWindow)
        //        continue;

        //    string tooltipText = asset.GetTooltip();
        //    if (string.IsNullOrEmpty(tooltipText))
        //    {
        //        tooltipText = asset.GetAssetWindowName();
        //    }
            
        //    CreateUIAsset(asset, asset.GetIcon(), asset.GetAssetWindowName(), tooltipText);
        //}
    }

    void CreateUIAssets(LayerManager.EditorLayer layer, LoadableAssetCategories category)
    {
        if (LoadableAssetManager == null)
            return;

        if (_placer == null)
            _placer = FindObjectOfType<Placer>();
        if (_eventSystem == null)
            _eventSystem = FindObjectOfType<EventSystem>();
        if (_graphicRaycaster == null)
            _graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
        if (_hierarchyObj == null)
            _hierarchyObj = GameObject.Find("HierarchyWindow");

        //_placer = FindObjectOfType<Placer>();
        //_eventSystem = FindObjectOfType<EventSystem>();
        //_graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
        //_hierarchyObj = GameObject.Find("HierarchyWindow");

        if (_sortedAssets == null)
            _sortedAssets = new List<LoadableAsset>();

        _sortedAssets.Clear();
        foreach (LoadableAsset asset in LoadableAssetManager.GetLoadableAssetsByLayer(layer, category))
        {
            _sortedAssets.Add(asset);
        }
        _sortedAssets.Sort((a, b) =>
        {
            return string.Compare(a.AssetWindowName, b.AssetWindowName);
        });

        //foreach (LoadableAsset asset in LoadableAssetManager.GetLoadableAssetsByLayer(layer, category))
        foreach (LoadableAsset asset in _sortedAssets)
        {
            if (!asset.ShowInAssetWindow)
                continue;

            string tooltipText = asset.GetTooltip();
            if (string.IsNullOrEmpty(tooltipText))
            {
                tooltipText = asset.GetAssetWindowName();
            }

            CreateUIAsset(asset, asset.GetIcon(), asset.GetAssetWindowName(), tooltipText);
        }
    }

    void CreateUIAsset(LoadableAsset asset, Sprite icon, string name, string tooltip)
    {
        GameObject go = Instantiate(uiObject, uiContentObj.transform);
        AssetUIObject uiObj = go.GetComponent<AssetUIObject>();

        uiObj.LoadableAssetManager = LoadableAssetManager;
        uiObj.Initialize(asset, icon, name, tooltip, _placer, _eventSystem, _graphicRaycaster, _hierarchyObj);
    }

    public void ClearContent()
    {
        for (int i = uiContentObj.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(uiContentObj.transform.GetChild(i).gameObject);
        }
    }

    protected override void OnLayerChanged(LayerManager.EditorLayer newLayer)
    {
        switch (newLayer)
        {
            case LayerManager.EditorLayer.Mine:
                Debug.Log("LOADING MINE TILES");
                LoadPlaceableAssetsFromCategory(LayerManager.EditorLayer.Mine);
                break;

            case LayerManager.EditorLayer.Object:
                Debug.Log("LOADING OBJECTS");
                LoadPlaceableAssetsFromCategory(LayerManager.EditorLayer.Object);
                break;

            case LayerManager.EditorLayer.SceneControls:
                LoadPlaceableAssetsFromCategory(LayerManager.EditorLayer.SceneControls);
                break;

            case LayerManager.EditorLayer.Ventilation:
                LoadPlaceableAssetsFromCategory(LayerManager.EditorLayer.Ventilation);
                break;

            case LayerManager.EditorLayer.VentilationBlockers:
                LoadPlaceableAssetsFromCategory(LayerManager.EditorLayer.VentilationBlockers);
                break;

            case LayerManager.EditorLayer.Cables:
                LoadPlaceableAssetsFromCategory(LayerManager.EditorLayer.Cables);
                break;


            default:
                Debug.LogWarning("Editor layer \"" + newLayer.ToString() + "\" not recognized!");
                break;
        }
    }
}
