using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMaterialColor : MonoBehaviour
{
    public Color Color;

    // Use this for initialization
    void Start()
    {
        var mr = GetComponent<MeshRenderer>();
        mr.material.color = Color;
    }
}
