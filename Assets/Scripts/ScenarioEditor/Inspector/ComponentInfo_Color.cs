using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ComponentInfo_Color : ModularComponentInfo, ISaveableComponent
{
    [Tooltip("The name of the component as it appears in the inspector, also used to reference this information so the name should be unique for the prefab. This should be assigned in both the editor and scenario prefab")]
    public string componentName = "Color";
    public Color TargetColor = Color.white;
    public Renderer RendererToColor;
    public int MaterialIndexToColor = 0;
    public DecalProjector DecalProjectorToColor;

    private Material _cachedMaterialRenderer;
    private Material _cachedMaterialDecal;

    public void LoadInfo(SavedComponent component)
    {
        ColorUtility.TryParseHtmlString(component.GetParamValueAsStringByName("RGBValue"), out TargetColor);
        
        UpdateColor();
    }

    public string[] SaveInfo()
    {
        string RGBValue = ColorUtility.ToHtmlStringRGBA(TargetColor);
        return new string[] { "RGBValue|#" + RGBValue };
    }

    public string SaveName()
    {
        return componentName;
    }

    public void UpdateColor()
    {
        if (RendererToColor != null)
        {
            Material mat = new Material(RendererToColor.materials[MaterialIndexToColor]);
            mat.color = TargetColor;
            //mat.SetColor("_EmissiveColor", col);
            if(_cachedMaterialRenderer != null)
            {
                Destroy(_cachedMaterialRenderer);
            }
            RendererToColor.materials[MaterialIndexToColor] = mat;
            _cachedMaterialRenderer = mat;
        }
        if(DecalProjectorToColor != null)
        {
            Material mat = new Material(DecalProjectorToColor.material);
            mat.color = TargetColor;
            if(_cachedMaterialDecal != null)
            {
                Destroy(_cachedMaterialDecal);
            }
            DecalProjectorToColor.material = mat;
            _cachedMaterialDecal = mat;
        }
    }
}
