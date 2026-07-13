using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;


public class UIScenarioList : MonoBehaviour
{
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    public SystemManager SystemManager;
    public LoadableAssetManager LoadableAssetManager;
    public ExternalAssetManager ExternalAssetManager;
    public string ScenarioListName = "ScenarioList";

    public VectorImage ErrorImage;
    public VectorImage WarningImage;

    private UIDocument _uiDocument;
    private ListView _scenarioListView;

    private List<CustomScenarioDS> _scenarioList = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
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

        _scenarioListView = _uiDocument.rootVisualElement.Q<ListView>(ScenarioListName);
        if (_scenarioListView == null)
            return;

        ModalProgressBar.ProgressChanged += OnProgressChanged;

        _scenarioListView.selectedIndicesChanged += OnListViewSelectionChanged;

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
        var obj = _scenarioListView.selectedItem;
        if (obj == null)
            return;

        var scenario = obj as CustomScenarioDS;
        if (scenario == null)
            return;

        VRMineUIEvents.RaiseCustomScenarioSelected(scenario);
    }

    void OnProgressChanged()
    {
        if (ModalProgressBar.ProgressValue >= 1 || ModalProgressBar.ProgressValue < 0)
        {
            UpdateList();
        }
    }

    void UpdateList()
    {

        _scenarioList.Clear();

        CustomScenarioUtil.GetCustomScenarios(_scenarioList, SystemManager.SystemConfig.ScenariosFolder);

        _scenarioListView.bindItem = OnListBindItem;
        _scenarioListView.itemsSource = _scenarioList;
        _scenarioListView.RefreshItems();
    }

    void OnListBindItem(VisualElement element, int index)
    {
        //var asset = _assetList[index];
        //const string loadError = "asset-load-error";
        //const string loadWarning = "asset-load-warning";

        //var decorator = element.Q<Image>("Decorator");

        //if (asset.Metadata == null || asset.LoadableAsset == null || (asset.FileData.LoadErrors != null && asset.FileData.LoadErrors.Count > 0))
        //{
        //    element.AddToClassList(loadError);
        //    element.RemoveFromClassList(loadWarning);

        //    decorator.vectorImage = ErrorImage;
        //}
        //else if (asset.Metadata.ImportLogMessages != null && asset.Metadata.ImportLogMessages.Count > 0)
        //{
        //    element.AddToClassList(loadWarning);
        //    element.RemoveFromClassList(loadError);

        //    decorator.vectorImage = WarningImage;
        //}
        //else
        //{
        //    element.RemoveFromClassList(loadError);
        //    element.RemoveFromClassList(loadWarning);

        //    decorator.vectorImage = null;
        //}

        //if (asset.LoadableAsset.Icon != null)
        //{
        //    var image = element.Q<Image>("AssetIcon");
        //    image.image = asset.LoadableAsset.Icon.texture;

        //}



    }

    void Notify([CallerMemberName] string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
}
