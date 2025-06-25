using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinerColorChanger : MonoBehaviour
{
    [System.Serializable]
    public struct RenderMatInfo
    {
        public Renderer Renderer;
        public int MaterialIndex;
    };

    public Renderer[] ColorChangeRenderers;
    public RenderMatInfo[] ColorChangeMats;

    public Color MinerColor;

    private MineMapSymbolRenderer _symbol;

    private void Awake()
    {
        _symbol = GetComponent<MineMapSymbolRenderer>();
        if (_symbol != null)
        {
            _symbol.Color = MinerColor;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateMiner();
    }

    public void UpdateMiner()
    {

        if (ColorChangeMats != null && ColorChangeMats.Length > 0)
        {
            foreach (var rendMatInfo in ColorChangeMats)
            {
                if (rendMatInfo.Renderer == null)
                    continue;

                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                mpb.SetColor("_BaseColor", MinerColor);
                rendMatInfo.Renderer.SetPropertyBlock(mpb, rendMatInfo.MaterialIndex);
            }
        }

        if (ColorChangeRenderers != null)
        {
            foreach (var rend in ColorChangeRenderers)
            {
                /*foreach (var mat in rend.materials)
                {
                    mat.color = MinerColor;
                    mat.SetColor("_BaseColor", MinerColor);
                    
                }*/

                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                mpb.SetColor("_BaseColor", MinerColor);
                rend.SetPropertyBlock(mpb);
            }
        }


        if (_symbol != null)
        {
            _symbol.UpdateColor(MinerColor);
            //_symbol.Color = MinerColor;
            //if (_symbol.MineMapSymbolManager != null)
            //{
            //    _symbol.MineMapSymbolManager.UpdateSymbolColor(_symbol.Symbol);
            //}
        }

    }

}
