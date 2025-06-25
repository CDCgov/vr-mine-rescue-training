using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawToTextureTest : MonoBehaviour
{
    public GameObject CameraPrefab;
    public Texture GeneratedTexture;    

    void Start()
    {
        var renderer = GetComponent<Renderer>();

        GeneratedTexture = DrawToTexture.Draw((Texture2D)renderer.material.mainTexture, CameraPrefab);

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetTexture("_BaseColorMap", GeneratedTexture);
        //mpb.SetColor("_BaseColor", GeneratedTexture);

        renderer.SetPropertyBlock(mpb);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
