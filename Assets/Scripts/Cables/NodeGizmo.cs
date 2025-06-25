using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeGizmo : MonoBehaviour
{
    //public int index;
    public HangingGeometry.CableNode CableNode;
    public Renderer NodeVisual;

    private MaterialPropertyBlock _mpb;

    public void SetColor(Color color)
    {
        if (NodeVisual == null)
            return;

        if (_mpb == null)
            _mpb = new MaterialPropertyBlock();

        _mpb.SetColor("_UnlitColor", color);

        NodeVisual.SetPropertyBlock(_mpb);
    }
}
