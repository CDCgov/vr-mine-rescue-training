using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class UIExternalAssetLoadProgress : MonoBehaviour, INotifyBindablePropertyChanged
{
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    [CreateProperty] public string LoadMessage { get; set; }
    [CreateProperty] public float LoadProgress { get; set; }
    [CreateProperty] public DisplayStyle ShowProgressBar => LoadProgress != 1.0f ? DisplayStyle.Flex : DisplayStyle.None;

    public string ProgressBarName = "LoadProgress";
    private UIDocument _uiDocument;
    private ProgressBar _progressBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!TryGetComponent<UIDocument>(out _uiDocument))
            return;

        _progressBar = _uiDocument.rootVisualElement.Q<ProgressBar>(ProgressBarName);
        if (_progressBar == null)
            return;

        _progressBar.dataSource = this;

        ModalProgressBar.ProgressChanged += OnProgressChanged;

        UpdateProgress();
    }
    
    void OnDestroy()
    {
        ModalProgressBar.ProgressChanged -= OnProgressChanged;
    }

    void OnProgressChanged()
    {
        UpdateProgress();
    }

    void UpdateProgress()
    {
        float progress = ModalProgressBar.ProgressValue;
        string progressMessage = ModalProgressBar.ProgressMessage;

        if (progress < 0)
        {
            progress = 1.0f;
            progressMessage = "Done";
        }

        if (progressMessage == null)
            progressMessage = string.Empty;

        LoadMessage = progressMessage;
        LoadProgress = progress;

        Notify(null);
    }

    void Notify([CallerMemberName] string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
}
