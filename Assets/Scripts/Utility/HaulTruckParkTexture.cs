using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaulTruckParkTexture : MonoBehaviour
{
    public Texture DriveTexture;
    public Texture ParkTexture;
    public Material ParkMaterial;
    public Material DriveMaterial;
    public Renderer CabRenderer;

    private Material _DashMat;
    private bool ChangeTexture = false;

    private void Start()
    {
        _DashMat = new Material(CabRenderer.materials[1]);
        CabRenderer.materials[1] = _DashMat;
    }


    private void OnEnable()
    {
        Debug.Log("On enable ran: " + CabRenderer.materials[1].name);
        Debug.Log("New texture? " + ParkTexture.name);
        //CabRenderer.materials[1].shader = Shader.Find("HDRP/Lit");
        //CabRenderer.materials[1].EnableKeyword("_BaseColor");
        //CabRenderer.materials[1].SetTexture("_BaseColor", ParkTexture);
        Material[] materials = CabRenderer.materials;
        materials[1] = new Material(ParkMaterial);

        CabRenderer.materials = materials;
    }
    private void OnDisable()
    {
        Material[] materials = CabRenderer.materials;
        materials[1] = new Material(DriveMaterial);

        CabRenderer.materials = materials;
    }

}
