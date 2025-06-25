using UnityEngine;
using System.Collections.Generic;

public class ProcCableTest : MonoBehaviour
{
    public List<Vector3> Path;

    private Mesh _mesh;

    // Use this for initialization
    void Start()
    {
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        if (mr == null)
        {
            mr = gameObject.AddComponent<MeshRenderer>();
        }

        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        if (mf == null)
        {
            mf = gameObject.AddComponent<MeshFilter>();
        }

        _mesh = mf.mesh;
        if (_mesh == null)
        {
            _mesh = new Mesh();
            mf.sharedMesh = _mesh;
        }

    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < Path.Count - 1; i++)
        {
            Vector3 p1 = transform.TransformPoint(Path[i]);
            Vector3 p2 = transform.TransformPoint(Path[i + 1]);
            Gizmos.DrawLine(p1, p2);
        }	
    }

    // Update is called once per frame
    void Update()
    {
        Vector3[] vertices = _mesh.vertices;
        int[] triangles = _mesh.triangles;
        Vector2[] uv = _mesh.uv;

        ProcGeometry.GenerateTube(Path, 0.06f, ref vertices, ref triangles, ref uv);

        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.uv = uv;

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }
}
