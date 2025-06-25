using NIOSH_EditorLayers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamstopEditorVisiblityHandler : LayerControlledClass
{
    public GameObject ShowObject;
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        if (TeamstopsVisibility.GetVisbility() && NIOSH_EditorLayers.LayerManager.GetCurrentLayer() == NIOSH_EditorLayers.LayerManager.EditorLayer.SceneControls)
        {
            SetVisbility(true);
        }
        else
        {
            SetVisbility(false);
        }
    }

    public void SetVisbility(bool isVisible)
    {
        if (ShowObject == null)
            return;

        if (isVisible)
        {
            ShowObject.SetActive(true);
        }
        else
        {
            ShowObject.SetActive(false);
        }
    }

    protected override void OnLayerChanged(LayerManager.EditorLayer newLayer)
    {
        if (newLayer == LayerManager.EditorLayer.SceneControls)
            SetVisbility(true);
        else
            SetVisbility(false);
    }
}
