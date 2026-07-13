using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine.UIElements;
using System.Collections.Generic;

public abstract class UIElementControllerBase : MonoBehaviour, INotifyBindablePropertyChanged
{
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    public string ElementName;

    protected UIDocument _uiDocument;
    protected VisualElement _element;

    private bool _startCalled = false;

    protected virtual void Start()
    {
        _startCalled = true;
        BaseInit();
    }

    private void BaseInit()
    {
        if (!TryGetComponent<UIDocument>(out _uiDocument))
            return;

        _element = _uiDocument.rootVisualElement.Q<VisualElement>(ElementName);

        Init();
    }

    protected abstract void Init();
    protected abstract void Cleanup();

    private void OnEnable()
    {
        //OnEnable is called before start - delay initial Init until Start is called
        if (!_startCalled)
            return;

        BaseInit();
    }

    private void OnDisable()
    {
        Cleanup();
    }

    protected void Notify([CallerMemberName] string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
}
