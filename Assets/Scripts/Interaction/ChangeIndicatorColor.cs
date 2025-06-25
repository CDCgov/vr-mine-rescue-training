using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeIndicatorColor : MonoBehaviour
{
    public Material BaseMaterial;
    public Material HighlightedMaterial;
    public Renderer Renderer;

    public void Highlight()
    {
        Renderer.material = HighlightedMaterial;
    }

    public void Restore()
    {
        Renderer.material = BaseMaterial;
    }
}
