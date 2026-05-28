using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class ExternalAssetDS : INotifyBindablePropertyChanged
{
    public LoadableAsset LoadableAsset;

    [CreateProperty] public string AssetCategory 
    { 
        get
        {
            return LoadableAsset.EditorLayer.ToString();
        } 
    }

    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    void Notify([CallerMemberName] string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
}
