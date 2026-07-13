using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine.UIElements;
using System.Collections.Generic;
using TMPro;
using System.Text;
using System.IO;

public class UIScenarioDetails : UIElementControllerBase
{
    public SystemManager SystemManager;
    public LoadableAssetManager LoadableAssetManager;

    [CreateProperty] public CustomScenarioDS SelectedScenario;
    [CreateProperty] public bool IsValidAssetSelected => SelectedScenario != null;
    [CreateProperty] public DisplayStyle ShowDefaultScreen => SelectedScenario == null ? DisplayStyle.Flex : DisplayStyle.None;

    //private VisualElement _assetEditors;

    private Button _saveFileList;
    private Button _saveToZip;
    private ListView _assetListView;

    void Reset()
    {
        ElementName = "ScenarioDetails";
    }

    protected override void Start()
    {
        base.Start();

        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
    }

    protected override void Init()
    {
        _element.dataSource = this;

        VRMineUIEvents.CustomScenarioSelected += OnCustomScenarioSelected;

        _saveFileList = _element.Q<Button>("SaveFileList");
        _saveToZip = _element.Q<Button>("SaveToZip");
        _assetListView = _element.Q<ListView>("scenario-assets-list-view");

        if (_saveFileList != null)
            _saveFileList.clicked += OnSaveFileListClicked;
        if (_saveToZip != null)
            _saveToZip.clicked += OnSaveToZipClicked;

        if (_assetListView != null)
            _assetListView.bindItem += OnListBindItem;
    }

    protected override void Cleanup()
    {
        VRMineUIEvents.CustomScenarioSelected -= OnCustomScenarioSelected;

        if (_saveFileList != null)
            _saveFileList.clicked -= OnSaveFileListClicked;
        if (_saveToZip != null)
            _saveToZip.clicked -= OnSaveToZipClicked;

        if (_assetListView != null)
            _assetListView.bindItem -= OnListBindItem;
    }

    void OnListBindItem(VisualElement element, int index)
    {
        var asset = SelectedScenario.ScenarioAssets[index];
        const string errorClass = "asset-error";
        const string warningClass = "asset-warning";

        bool assetError = false;
        bool assetWarning = false;

        if (!asset.ValidAssetID)
            assetError = true;

        if (asset.AssetReference != null && 
            asset.AssetReference.ExternalAssetMetadata != null &&
            asset.AssetReference.ExternalAssetMetadata.ImportLogMessages != null &&
            asset.AssetReference.ExternalAssetMetadata.ImportLogMessages.Count > 0)
        {
            assetWarning = true;
        }


        if (assetError)
        {
            element.AddToClassList(errorClass);
            element.RemoveFromClassList(warningClass);
        }
        else if (assetWarning)
        {
            element.AddToClassList(warningClass);
            element.RemoveFromClassList(errorClass);
        }
        else
        {
            element.RemoveFromClassList(errorClass);
            element.RemoveFromClassList(warningClass);
        }
    }

    private void OnSaveFileListClicked()
    {
        if (SelectedScenario == null)
            return;

        var date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var filename = Path.Combine(SystemManager.SystemConfig.RootDataFolder, $"{Path.GetFileNameWithoutExtension(SelectedScenario.FilePath)}_{date}.txt");
        SelectedScenario.SaveFileList(filename);
    }
    
    private void OnSaveToZipClicked()
    {
        if (SelectedScenario == null)
            return;

        var date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var filename = Path.Combine(SystemManager.SystemConfig.RootDataFolder, $"{Path.GetFileNameWithoutExtension(SelectedScenario.FilePath)}_{date}.zip");
        SelectedScenario.SaveFilesToZip(filename);
    }


    private void OnCustomScenarioSelected(CustomScenarioDS obj)
    {
        obj.LoadScenarioAssetDetails(LoadableAssetManager);

        SelectedScenario = obj;
        //_element.dataSource = obj;

        //AddAllEditors();

        Notify(null);

    }

}
