using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class UIExternalAssetList : MonoBehaviour, INotifyBindablePropertyChanged
{
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    public LoadableAssetManager LoadableAssetManager;
    public string AssetListName = "ExternalAssetList";

    private UIDocument _uiDocument;
    private ListView _assetListView;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);

        Init();
    }

    void Init()
    {
        if (LoadableAssetManager == null)
            return;

        if (!TryGetComponent<UIDocument>(out _uiDocument))
            return;

        _assetListView = _uiDocument.rootVisualElement.Q<ListView>(AssetListName);
        if (_assetListView == null)
            return;

        ModalProgressBar.ProgressChanged += OnProgressChanged;

        _assetListView.selectedIndicesChanged += OnListViewSelectionChanged;

        UpdateList();
    }

    void OnEnable()
    {
        Init();
    }

    void OnDisable()
    {
        ModalProgressBar.ProgressChanged -= OnProgressChanged;
    }

    private void OnListViewSelectionChanged(IEnumerable<int> indices)
    {
        var obj = _assetListView.selectedItem;
        if (obj == null)
            return;

        var asset = obj as ExternalAssetDS;
        if (asset == null)
            return;

        ExternalAssetEditorEvents.RaiseExternalAssetSelected(asset);
    }

    void OnDestroy()
    {
        
    }

    void OnProgressChanged()
    {
        if ( ModalProgressBar.ProgressValue >= 1 || ModalProgressBar.ProgressValue < 0)
        {
            UpdateList();
        }        
    }

    void UpdateList()
    {
        List<ExternalAssetDS> assetList = new List<ExternalAssetDS>();
        foreach (var loadable in LoadableAssetManager.GetAllLoadableAssets())
        {
            if (loadable.ExternalAssetMetadata == null)
                continue;

            assetList.Add(new ExternalAssetDS
            {
                LoadableAsset = loadable,
            });
        }

        _assetListView.itemsSource = assetList;
        _assetListView.RefreshItems();
    }

    void Notify([CallerMemberName] string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
}
