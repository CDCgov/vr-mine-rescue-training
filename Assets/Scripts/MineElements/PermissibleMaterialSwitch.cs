using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PermissibleMaterialSwitch : MonoBehaviour
{
    public MeshRenderer RendererToSwap;
    public List<Material> PermissibleMaterials;
    public List<Material> NonPermissibleMaterials;

    public void PermissibleSwap(bool isPermissible)
    {
        if (isPermissible)
        {
            RendererToSwap.SetMaterials(PermissibleMaterials);
        }
        else
        {
            RendererToSwap.SetMaterials(NonPermissibleMaterials);
        }
    }
}
