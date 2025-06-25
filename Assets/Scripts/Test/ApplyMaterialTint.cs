using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyMaterialTint : MonoBehaviour
{
    public Color TintColor;

    // Start is called before the first frame update
    void Start()
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetColor("_Color", TintColor);

        var renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
        {
            r.SetPropertyBlock(mpb);
            for (int i = 0; i < r.materials.Length; i++)
            {
                
            }
        }    
    }

}
