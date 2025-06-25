using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NIOSH_EditorLayers;


public class EnvironmentTabsController : TabsController
{

    public GameObject TabsGroup;
    private PlacableAssetLoader _loader;


    protected override void Start()
    {
        base.Start();
        LayerManager.Instance.layerChanged += ToggleTabs;
        _loader = Placer.GetDefault().gameObject.GetComponent<PlacableAssetLoader>();
        
    }

    protected override void ChangeTab(int newTab)
    {
        if (_loader == null)
            _loader = Placer.GetDefault().gameObject.GetComponent<PlacableAssetLoader>();

        base.ChangeTab(newTab);
        switch (newTab)
        {
            case 0:
                ActivateAllTab();
                break;
            case 1:
                ActivateTilesTab();
                break;
            case 2:
                ActivateScansTab();
                break;
        }
    }
    public void ActivateAllTab()
    {
        Debug.Log("ActivateAllTab()");
        _loader.LoadPlaceableAssetsFromCategory(LayerManager.EditorLayer.Mine);
    }
    public void ActivateTilesTab()
    {
        Debug.Log("ActivateTilesTab()");
        _loader.LoadPlaceableAssetsFromCategory(LayerManager.EditorLayer.Mine, LoadableAssetCategories.MineTile);
    }

    public void ActivateScansTab()
    {
        Debug.Log("ActivateScansTab()");
        _loader.LoadPlaceableAssetsFromCategory(LayerManager.EditorLayer.Mine, LoadableAssetCategories.ScannedEnvironment);
    }
    /// <summary>
    /// Toggle tabs group on or off based on layer
    /// </summary>
    public void ToggleTabs(LayerManager.EditorLayer newLayer)
    {
        TabsGroup.SetActive(newLayer == LayerManager.EditorLayer.Mine);
    }
}

