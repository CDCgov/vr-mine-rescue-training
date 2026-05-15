using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class ExternalAssetsEditorDS : MonoBehaviour, IDataSourceViewHashProvider, INotifyBindablePropertyChanged
{
    public ExternalAssetManager ExternalAssetManager;
    public LoadableAssetManager LoadableAssetManager;

    public UIDocument Target;

    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    [CreateProperty]
    public float LoadProgress { get; set; }
    [CreateProperty]
    public string LoadMessage { get; set; }

    [CreateProperty]
    public float TestValue
    {
        get => _testValue;        
        set
        {
            _testValue = value;
            Notify();
        }
    }

    private float _testValue;
    private long _viewHashCode = 0;
    private ListView _loadableList;

    void Start()
    {
        if (ExternalAssetManager == null)
            ExternalAssetManager = ExternalAssetManager.GetDefault(gameObject);
        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);

        ModalProgressBar.ProgressChanged += OnProgressChanged;

        if (Target == null)
            return;

        Target.rootVisualElement.dataSource = this;

        _loadableList = Target.rootVisualElement.Q<ListView>("LoadableList");
        
    }

    private void OnProgressChanged()
    {
        float progress = ModalProgressBar.ProgressValue;
        string progressMessage = ModalProgressBar.ProgressMessage;
        
        if (progress < 0)
        {
            progress = 1.0f;
            progressMessage = "Done";

            _loadableList.itemsSource = LoadableAssetManager.GetAllLoadableAssets().ToList();
            _loadableList.RefreshItems();
        }

        if (progressMessage == null)
            progressMessage = string.Empty;

        LoadMessage = progressMessage;
        LoadProgress = progress;

        Notify("LoadMessage");
        Notify("LoadProgress");

    }

    void OnDestroy()
    {
        ModalProgressBar.ProgressChanged -= OnProgressChanged;
    }

    public long GetViewHashCode()
    {
        return _viewHashCode;
    }

    void Notify([CallerMemberName] string property = "")
    {
        _viewHashCode++;
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }

    void Update()
    {
        TestValue = Time.realtimeSinceStartup;
    }
}
