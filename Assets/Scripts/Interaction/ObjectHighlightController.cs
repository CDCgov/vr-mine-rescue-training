using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectHighlightColor
{
    HighlightOff            =  0,
    UnavailableHighlight    =  5,
    SelectHighlight         = 10,
    ActivateHighlight       = 20,
    ErrorHighlight          = 30,
}

public class ObjectHighlightController : MonoBehaviour
{

    private struct HighlightRendererData
    {
        public Renderer Renderer;
        public MaterialPropertyBlock PropertyBlock;
    }

    private struct HighlightRequestData
    {
        public Component RequestingComponent;
        public Color Color;
        public int Priority;
    }

    public List<Renderer> HighlightRenderers;

    //public bool UseGlowSurrogate = false;
    //public MeshRenderer SurrogateMesh;
    ////public Renderer ModelRenderer;
    
    //private Color[] startingColors;
    //private Texture[] emissiveTextures;

    //public Renderer MeshRenderer;
    //private float[] _emissiveLevels;
    //private Color _glowColor = new Color(0, 0.1f, 0, 1 / 255);
    //private Color _rejectColor = new Color(0.1f, 0, 0, (1f / 255f));
    //private List<Color> _cachedColors;

    //private List<MaterialPropertyBlock> _materialBlocks;

    private List<HighlightRendererData> _rendererData;
    private List<HighlightRequestData> _highlightRequests;

    public static void ShowHighlight(GameObject targetObject, Component requestingObject, ObjectHighlightColor color)
    {
        ObjectHighlightController highlight;
        if (!targetObject.TryGetComponent<ObjectHighlightController>(out highlight))
        {
            highlight = targetObject.AddComponent<ObjectHighlightController>();
        }

        highlight.ShowHighlight(requestingObject, color);
    }

    public static void ClearHighlight(GameObject targetObject, Component requestingObject)
    {
        ShowHighlight(targetObject, requestingObject, ObjectHighlightColor.HighlightOff);
    }

    //public static void ShowHighlight(GameObject targetObject, GameObject requestingObject, Color highlightColor, bool enableHighlight)
    //{
    //    ObjectHighlightController highlight;
    //    if (!targetObject.TryGetComponent<ObjectHighlightController>(out highlight))
    //    {
    //        highlight = targetObject.AddComponent<ObjectHighlightController>();
    //    }

    //    highlight.ShowHighlight(requestingObject, highlightColor, enableHighlight);
    //}

    public void ShowHighlight(Component requestingObject, ObjectHighlightColor color)
    {
        switch (color)
        {
            case ObjectHighlightColor.HighlightOff:
                ShowHighlight(requestingObject, Color.white, false, 0);
                break;
            case ObjectHighlightColor.SelectHighlight:
                ShowHighlight(requestingObject, new Color(0, 0.5f, 0), true, (int)color);
                break;
            case ObjectHighlightColor.ActivateHighlight:
                ShowHighlight(requestingObject, new Color(0, 0.2f, 0.5f), true, (int)color);
                break;
            case ObjectHighlightColor.UnavailableHighlight:
                ShowHighlight(requestingObject, new Color(0.5f, 0.1f, 0.5f), true, (int)color);
                break;
            case ObjectHighlightColor.ErrorHighlight:
                ShowHighlight(requestingObject, new Color(0.5f, 0.1f, 0), true, (int)color);
                break;
        }
    }

    public void ClearHighlight(Component requestingObject)
    {
        ShowHighlight(requestingObject, Color.white, false, 0);
    }

    public void ShowHighlight(Component requestingComponent, Color highlightColor, bool enableHighlight, int priority)
    {
        if (_highlightRequests == null)
            _highlightRequests = new List<HighlightRequestData>();

        //remove any existing highlight request for this object
        for (int i = _highlightRequests.Count - 1; i >= 0; i--)
        {
            var req = _highlightRequests[i];
            if (req.RequestingComponent == null || req.RequestingComponent.gameObject == null ||
                req.RequestingComponent == requestingComponent)
                _highlightRequests.RemoveAt(i);
        }

        if (enableHighlight)
        {
            _highlightRequests.Add(new HighlightRequestData
            {
                RequestingComponent = requestingComponent,
                Color = highlightColor,
                Priority = priority,
            });
        }


        if (_highlightRequests.Count <= 0)
            ClearHighlight();
        else
        {
            int colorPriority = int.MinValue;
            Color color = Color.white;

            //take last color with highest priority
            for (int i = 0; i < _highlightRequests.Count; i++)
            {
                if (_highlightRequests[i].Priority >= colorPriority)
                {
                    colorPriority = _highlightRequests[i].Priority;
                    color = _highlightRequests[i].Color;
                }
            }

            ShowHighlight(color);
        }
    }

    private void ShowHighlight(Color color)
    {
        if (_rendererData == null)
            InitializeRendererData();

        foreach (var data in _rendererData)
        {
            data.PropertyBlock.SetColor("_EmissiveColor", color);
            data.PropertyBlock.SetTexture("_EmissionMap", Texture2D.whiteTexture);
            data.PropertyBlock.SetTexture("_EmissiveColorMap", Texture2D.whiteTexture);
            data.Renderer.SetPropertyBlock(data.PropertyBlock);
        }
    }

    private void ClearHighlight()
    {
        if (_rendererData == null)
            InitializeRendererData();

        foreach (var data in _rendererData)
        {
            data.Renderer.SetPropertyBlock(null);
        }
    }

    private void InitializeRendererData()
    {
        if (HighlightRenderers == null || HighlightRenderers.Count <= 0)
        {
            HighlightRenderers = new List<Renderer>();
            transform.GetComponentsInChildren<Renderer>(HighlightRenderers);

            for (int i = HighlightRenderers.Count - 1; i >= 0; i--)
            {
                var rend = HighlightRenderers[i];
                //if (!rend.TryGetComponent<MeshFilter>(out var filter))
                //    HighlightRenderers.RemoveAt(i);

                if (rend.TryGetComponent<TMPro.TMP_Text>(out var _))
                    HighlightRenderers.RemoveAt(i);
            }
        }

        _rendererData = new List<HighlightRendererData>(HighlightRenderers.Count);
        for (int i = 0; i < HighlightRenderers.Count; i++)
        {
            var rend = HighlightRenderers[i];
            var mpb = new MaterialPropertyBlock();

            _rendererData.Add(new HighlightRendererData
            {
                Renderer = rend,
                PropertyBlock = mpb,
            });
        }

    }


    //private void Start()
    //{
    //    _cachedColors = new List<Color>();
    //    _materialBlocks = new List<MaterialPropertyBlock>();
    //    if (MeshRenderer == null)
    //    {
    //        MeshRenderer = GetComponentInChildren<MeshRenderer>();
    //    }
        
    //    if (MeshRenderer != null)
    //    {
    //        for (int i = 0; i < MeshRenderer.materials.Length; i++)
    //        {
    //            MaterialPropertyBlock block = new MaterialPropertyBlock();
    //            MeshRenderer.GetPropertyBlock(block, i);
    //            _materialBlocks.Add(block);
    //            if (MeshRenderer.materials[i].HasProperty("_EmissiveColor"))
    //            {
    //                if (MeshRenderer.materials[i].GetColor("_EmissiveColor") != null)
    //                {
    //                    _cachedColors.Add(MeshRenderer.materials[i].GetColor("_EmissiveColor"));
    //                }
    //                else
    //                {
    //                    _cachedColors.Add(new Color(0, 0, 0, 0));
    //                }
    //            }
    //        }
    //    }
    //}

    //public void GlowOn(bool standardGlow = true)
    //{
    //    if(MeshRenderer == null)
    //    {
    //        return;
    //    }

    //    if (UseGlowSurrogate)
    //    {
    //        SurrogateMesh.enabled = true;
    //    }
    //    if (standardGlow)
    //    {
    //        for (int i = 0; i < MeshRenderer.materials.Length; i++)
    //        {
    //            if (MeshRenderer.materials[i].HasProperty("_EmissiveColor"))
    //            {
    //                MeshRenderer.materials[i].SetColor("_EmissiveColor", _glowColor);
    //            }
    //        }
    //    }
    //    else
    //    {
    //        MaterialPropertyBlock redBlock = new MaterialPropertyBlock();
    //        redBlock.SetColor("_EmissiveColor", _rejectColor);
    //        for(int i = 0; i < MeshRenderer.materials.Length; i++)
    //        {
    //            MeshRenderer.SetPropertyBlock(redBlock, i);
    //        }
    //    }
    //    //Renderer ren = GetComponent<Renderer>();
    //    //for (int i = 0; i < ren.materials.Length; i++)
    //    //{
    //    //    if (emissiveTextures[i] != null)
    //    //    {
    //    //        ren.materials[i].SetTexture("_EmissionMap", null);
    //    //    }
    //    //    Color color = new Color(0, 0.1f, 0);
    //    //    //color.g += 0.1f;

    //    //    ren.materials[i].SetColor("_EmissionColor", color);
    //    //}
        
    //}
    //public void GlowOff(bool standardGlow = true)
    //{
    //    if(MeshRenderer == null)
    //    {
    //        return;
    //    }

    //    if (UseGlowSurrogate)
    //    {
    //        SurrogateMesh.enabled = false;
    //        return;
    //    }

    //    if (_cachedColors == null || MeshRenderer.materials == null || _cachedColors.Count != MeshRenderer.materials.Length)
    //        return;


    //    if (standardGlow)
    //    {
    //        for (int i = 0; i < MeshRenderer.materials.Length; i++)
    //        {
    //            MeshRenderer.materials[i].SetColor("_EmissiveColor", _cachedColors[i]);
    //        }
    //    }
    //    else
    //    {
    //        for (int i = 0; i < MeshRenderer.materials.Length; i++)
    //        {
    //            if(i < _materialBlocks.Count)
    //            {
    //                MeshRenderer.SetPropertyBlock(_materialBlocks[i], i);
    //            }
    //        }
    //    }
    //    //    Color color;
    //    //    Renderer ren = GetComponent<Renderer>();
    //    //    if(startingColors == null)
    //    //    {
    //    //        return;
    //    //    }
    //    //    for (int i = 0; i < ren.materials.Length; i++)
    //    //    {
    //    //        color = startingColors[i];
    //    //        if (emissiveTextures[i] != null)
    //    //        {
    //    //            ren.material.SetTexture("_EmissionMap", emissiveTextures[i]);
    //    //        }
    //    //        ren.material.SetColor("_EmissionColor", color);
    //    //    }
    //}
}
