using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine.UIElements;
using System.Collections.Generic;

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
        if (!TryGetComponent<UIDocument>(out _uiDocument))
            return;

        _assetListView = _uiDocument.rootVisualElement.Q<ListView>(AssetListName);
        if (_assetListView == null)
            return;

        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);

        ModalProgressBar.ProgressChanged += OnProgressChanged;

        UpdateList();
    }

    void OnDestroy()
    {
        ModalProgressBar.ProgressChanged -= OnProgressChanged;
    }

    void OnProgressChanged()
    {
        UpdateList();
    }

    void UpdateList()
    {
        List<ExternalAssetDS> assetList = new List<ExternalAssetDS>();
        foreach (var loadable in LoadableAssetManager.GetAllLoadableAssets())
        {
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
