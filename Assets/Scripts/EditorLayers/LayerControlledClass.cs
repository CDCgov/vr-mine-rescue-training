using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NIOSH_EditorLayers
{
    public abstract class LayerControlledClass : MonoBehaviour
    {
        // Start is called before the first frame update
        protected void Start()
        {
            if (LayerManager.Instance)
            {
                LayerManager.Instance.layerChanged += OnLayerChanged;
            }
        }

        protected void OnDestroy()
        {
            if (LayerManager.Instance)
            {
                LayerManager.Instance.layerChanged -= OnLayerChanged;
            }
        }

        protected void CheckInitalLayer()
        {
            OnLayerChanged(LayerManager.GetCurrentLayer());
        }

        protected abstract void OnLayerChanged(LayerManager.EditorLayer newLayer);
    }
}