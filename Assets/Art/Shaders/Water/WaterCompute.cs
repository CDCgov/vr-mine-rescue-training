using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCompute : MonoBehaviour
{
    public ComputeShader WaterComputeShader;

    public RenderTexture WaterOutputTexture;

    private int _waterKernel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        WaterOutputTexture = new RenderTexture(512, 512, 8, RenderTextureFormat.ARGB32);
        WaterOutputTexture.enableRandomWrite = true;

        _waterKernel = WaterComputeShader.FindKernel("WaterSurfaceCompute");
        WaterComputeShader.SetTexture(_waterKernel, "Result", WaterOutputTexture);
    }

    private void OnDisable()
    {
        WaterOutputTexture.Release();
        WaterOutputTexture = null;
    }

    // Update is called once per frame
    void Update()
    {
        WaterComputeShader.Dispatch(_waterKernel, 64, 64, 1);
    }
}
