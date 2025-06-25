using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TagColor
{
    Red,
    Green,
    White,
    Yellow,
    FlourescentYellow
}
public class LifelineTag : LifelineItem {

    public Material TagColorMat;
    public Color TagColor;
    public int TagColorIndex; // color index for scriptable : Indexed Color Data
    [HideInInspector]public TagColor T_Color; ///depreciated
    public Material[] LifelineMaterial;
    public GameObject Geometry;




    //private void Reset()
    //{
    //    Geometry.GetComponent<Renderer>().materials[1] = new Material(Shader.Find("Standard"));
    //    Geometry.GetComponent<Renderer>().materials[1].mainTexture = TagColorMat.mainTexture;
    //    UpdateTagColor(TagColor);
    //    //UpdateTagColor(TagColor);
    //}

    public void Start()
    {

        SetMaterialColor();
        
    }

    public void SetMaterialColor()
    {
        if (Geometry != null) 
        { 
            var renderer = Geometry.GetComponent<Renderer>(); 
        
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            //mpb.SetColor("_EmissiveColor", Color.black);
            mpb.SetColor("_BaseColor", TagColor);

            renderer.SetPropertyBlock(mpb, 1);
        }
        else
        {
            var renderer = GetComponentInChildren<Renderer>();
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            //mpb.SetColor("_EmissiveColor", Color.black);
            mpb.SetColor("_BaseColor", TagColor);

            renderer.SetPropertyBlock(mpb, 1);
        }
    }

    public void UpdateTagColor(Color color)
    {
        //Geometry.GetComponent<Renderer>().sharedMaterials[1].color = color;		
        //Geometry.GetComponent<Renderer>().sharedMaterials[1].SetColor("_EmissionColor", Color.black);
        TagColor = color;
        SetMaterialColor();
    }

    public void UpdateTagColor(Color color, int index)
    {
        TagColor = color;
        TagColorIndex = index;
        SetMaterialColor();
    }

    private void OnValidate()
    {
        //Geometry.GetComponent<Renderer>().sharedMaterials[1] = new Material(Shader.Find("Standard"));
        //Geometry.GetComponent<Renderer>().sharedMaterials[1].mainTexture = TagColorMat.mainTexture;
        
        UpdateTagColor(TagColor, TagColorIndex);
    }
}
