using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DrawToTextureTest2 : MonoBehaviour
{
    public List<MeshRenderer> RenderList;

    public Renderer TargetRenderer;

    // Start is called before the first frame update
    void Start()
    {
        var cam = GetComponent<Camera>();

        var tex = TargetRenderer.material.mainTexture;
        //var rt = new RenderTexture(tex.width, tex.height, 24, RenderTextureFormat.RGB565);
        //var rt = new RenderTexture(TexSize, TexSize, 16, RenderTextureFormat.ARGBHalf);        
        var rt = new RenderTexture(tex.width, tex.height, 24, RenderTextureFormat.Default);

        cam.targetTexture = rt;

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetTexture("_BaseColorMap", rt);

        TargetRenderer.SetPropertyBlock(mpb);

        UpdateTexture();
    }

    private void UpdateTexture()
    {
        var cam = GetComponent<Camera>();

        foreach (var rend in RenderList)
        {
            rend.gameObject.SetActive(true);
        }

        CommandBuffer cb = new CommandBuffer();
        foreach (var rend in RenderList)
        {
            if (rend is MeshRenderer)
            {
                //cb.DrawRenderer(rend, ((MeshRenderer)rend).material, 0, 4);
                cb.DrawRenderer(rend, ((MeshRenderer)rend).material);
            }
        }
        cam.AddCommandBuffer(CameraEvent.AfterEverything, cb);

        cam.Render();

        cam.RemoveAllCommandBuffers();
        cb.Release();

        foreach (var rend in RenderList)
        {
            rend.gameObject.SetActive(false);
        }
    }

    private void Update()
    {

        UpdateTexture();
    }
}
