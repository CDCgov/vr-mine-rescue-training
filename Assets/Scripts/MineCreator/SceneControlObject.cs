using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NIOSH_EditorLayers;
using UnityEngine.UI;

public class SceneControlObject : LayerControlledClass
{
    public Renderer[] Renderers;
    public Image[] Images;
    protected override void OnLayerChanged(LayerManager.EditorLayer newLayer)
    {       
        foreach (var renderer in Renderers)
        {
            renderer.enabled = (newLayer == LayerManager.EditorLayer.SceneControls);
        }
        foreach (var image in Images)
        {
            image.enabled = (newLayer == LayerManager.EditorLayer.SceneControls);
        }
    }
    
    void Awake()
    {
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "BAH_ScenarioEditor")
        {
            return;
        }
        if (Renderers == null || Renderers.Length == 0)
        {
            Renderers = GetComponentsInChildren<Renderer>();
        }

        if(Images == null || Images.Length == 0)
        {
            Images = GetComponentsInChildren<Image>();
        }
        foreach (var renderer in Renderers)
        {
            renderer.enabled = (LayerManager.GetCurrentLayer() == LayerManager.EditorLayer.SceneControls);
        }

        foreach (var image in Images)
        {
            image.enabled = (LayerManager.GetCurrentLayer() == LayerManager.EditorLayer.SceneControls);
        }
    }
}
