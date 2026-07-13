using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine.UIElements;
using System.Collections.Generic;
using TMPro;

public class UIExternalAssetDetails : UIElementControllerBase
{    
    [CreateProperty] public ExternalAssetDS SelectedAsset;
    [CreateProperty] public bool IsValidAssetSelected => SelectedAsset != null;
    [CreateProperty] public DisplayStyle ShowDefaultScreen => SelectedAsset == null ? DisplayStyle.Flex : DisplayStyle.None;

    private VisualElement _assetEditors;
        
    void Reset()
    {
        ElementName = "ExternalAssetDetails";
    }
    protected override void Init()
    {
        _element.dataSource = this;

        _assetEditors = _element.Q<VisualElement>("AssetEditors");

        AddAllEditors();

        VRMineUIEvents.ExternalAssetSelected += OnExternalAssetSelected;
    }

    protected override void Cleanup()
    {
        VRMineUIEvents.ExternalAssetSelected -= OnExternalAssetSelected;
    }

    private void OnExternalAssetSelected(ExternalAssetDS obj)
    {
        SelectedAsset = obj;
        //_element.dataSource = obj;

        AddAllEditors();

        Notify(null);
    }

    private void AddAllEditors()
    {
        if (SelectedAsset == null || SelectedAsset.LoadableAsset == null || SelectedAsset.LoadableAsset.ExternalAssetMetadata == null)
            return;

        _assetEditors.contentContainer.Clear();

        var dataSource = SelectedAsset.LoadableAsset.ExternalAssetMetadata;
        AddEditor(dataSource, "Asset ID", nameof(ExternalAssetMetadata.AssetID), true);
        AddEditor(dataSource, "Asset Name", nameof(ExternalAssetMetadata.AssetName), false);
    }

    private void AddEditor(object dataSource, string labelText, string fieldName, bool readOnly = true)
    {
        var container = new VisualElement() { name = "EditorContainer" };
        container.AddToClassList("field-editor__container");
        container.dataSource = dataSource;

        var label = new Label() { text = labelText };
        label.AddToClassList("field-editor__label");

        var editor = new TextField();
        editor.AddToClassList("field-editor__editor");
        editor.SetBinding("value", new DataBinding
        {
            dataSourcePath = new PropertyPath(fieldName),
        });

        if (readOnly)
        {
            editor.isReadOnly = true;
        }

        container.Add(label);
        container.Add(editor);

        _assetEditors.Add(container);

    }
    
}
