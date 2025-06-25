using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DrawMeshNormals : MonoBehaviour
{

    public Color NormalColor = Color.red;
    public float NormalLength = 0.1f;

    MeshRenderer _mr;
    MeshFilter _mf;

    // Use this for initialization
    void Start()
    {
        _mr = GetComponent<MeshRenderer>();
        _mf = GetComponent<MeshFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        Mesh m = _mf.sharedMesh;

        if (m.vertices != null & m.normals != null & m.vertices.Length == m.normals.Length)
        {
            for (int i = 0; i < m.vertices.Length; i++)
            {
                Vector3 p = m.vertices[i];
                Vector3 n = m.normals[i];

                p = transform.TransformPoint(p);
                n = transform.TransformDirection(n);

                Debug.DrawLine(p, p + n * NormalLength, NormalColor);
            }
        }
    }
}
