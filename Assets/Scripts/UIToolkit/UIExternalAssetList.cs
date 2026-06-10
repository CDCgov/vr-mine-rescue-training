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
    public ExternalAssetManager ExternalAssetManager;
    public string AssetListName = "ExternalAssetList";

    public VectorImage ErrorImage;
    public VectorImage WarningImage;

    private UIDocument _uiDocument;
    private ListView _assetListView;

    private List<ExternalAssetDS> _assetList = new List<ExternalAssetDS>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);
        if (ExternalAssetManager == null)
            ExternalAssetManager = ExternalAssetManager.GetDefault(gameObject);

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

    void OnProgressChanged()
    {
        if ( ModalProgressBar.ProgressValue >= 1 || ModalProgressBar.ProgressValue < 0)
        {
            UpdateList();
        }        
    }

    void UpdateList()
    {

        _assetList.Clear();

        foreach (var fileData in ExternalAssetManager.GetExternalAssetFileData())
        {
            //if (fileData.Metadata == null)
            //    continue;

            _assetList.Add(new ExternalAssetDS
            {
                LoadableAsset = fileData.LoadableAsset,
                Metadata = fileData.Metadata,
                FileData = fileData,
            });
        }

        _assetListView.bindItem = OnListBindItem;
        _assetListView.itemsSource = _assetList;
        _assetListView.RefreshItems();
    }

    void OnListBindItem(VisualElement element, int index)
    {
        var asset = _assetList[index];
        const string loadError = "asset-load-error";
        const string loadWarning = "asset-load-warning";

        var decorator = element.Q<Image>("Decorator");

        if (asset.Metadata == null || asset.LoadableAsset == null || (asset.FileData.LoadErrors != null && asset.FileData.LoadErrors.Count > 0))
        {
            element.AddToClassList(loadError);
            element.RemoveFromClassList(loadWarning);

            decorator.vectorImage = ErrorImage;
        }
        else if (asset.Metadata.ImportLogMessages != null && asset.Metadata.ImportLogMessages.Count > 0)
        {
            element.AddToClassList(loadWarning);
            element.RemoveFromClassList(loadError);

            decorator.vectorImage = WarningImage;
        }
        else
        {
            element.RemoveFromClassList(loadError);
            element.RemoveFromClassList(loadWarning);

            decorator.vectorImage = null;
        }

        if (asset.LoadableAsset.Icon != null)
        {
            var image = element.Q<Image>("AssetIcon");
            image.image = asset.LoadableAsset.Icon.texture;
            
        }

        
        
    }

    void Notify([CallerMemberName] string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
}
