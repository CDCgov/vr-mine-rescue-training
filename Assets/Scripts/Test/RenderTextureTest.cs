using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureTest : MonoBehaviour
{
    public RenderTexture RenderTexture;
    public MeshRenderer TargetMesh;
    public int TexSize = 256;

    public bool ManualCameraRender = false;
    public bool UseCommandBuffers = false;

    private Camera _camera;    

    // Start is called before the first frame update
    void Start()
    {
        if (RenderTexture == null)
        {
            //var rt = new RenderTexture(TexSize, TexSize, 16, RenderTextureFormat.ARGBHalf);
            var rt = new RenderTexture(TexSize, TexSize, 16, RenderTextureFormat.Default);
            //rt.volumeDepth = 1;
            rt.Create();
            rt.name = $"{gameObject.name}_RT";
            rt.vrUsage = VRTextureUsage.None;
            RenderTexture = rt;
        }

        _camera = GetComponent<Camera>();

        _camera.targetTexture = RenderTexture;

        if (TargetMesh != null)
        {
            TargetMesh.material.SetTexture("_BaseColorMap", RenderTexture);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ManualCameraRender && _camera != null)
        {
            _camera.Render();
        }
    }
}
