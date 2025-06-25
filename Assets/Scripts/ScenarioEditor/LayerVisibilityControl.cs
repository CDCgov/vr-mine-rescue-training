using NIOSH_EditorLayers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerVisibilityControl : LayerControlledClass
{
    public LayerManager.EditorLayer VisibleLayer;
    public List<Renderer> Renderers;
    
    
    new void Start()
    {
        base.Start();

        var currentLayer = LayerManager.GetCurrentLayer();
        UpdateVisibility(currentLayer);
    }

    protected override void OnLayerChanged(LayerManager.EditorLayer newLayer)
    {
        UpdateVisibility(newLayer);
    }

    void UpdateVisibility(LayerManager.EditorLayer currentLayer)
    {
        if (Renderers == null || Renderers.Count <= 0)
            return;

        bool visible = currentLayer == VisibleLayer;

        foreach (var rend in Renderers)
        {
            rend.enabled = visible;
        }
    }
}
