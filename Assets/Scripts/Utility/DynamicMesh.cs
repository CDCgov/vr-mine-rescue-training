using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DynamicMesh
{
    public Mesh GeneratedMesh;

    private List<Vector3> _vertices;
    private List<int> _triangles;

    public DynamicMesh()
    {
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
    }

    public void UpdateMesh()
    {
        if (GeneratedMesh == null)
        {
            GeneratedMesh = new Mesh();
            GeneratedMesh.MarkDynamic();
        }

        GeneratedMesh.Clear();
        GeneratedMesh.vertices = _vertices.ToArray();
        GeneratedMesh.triangles = _triangles.ToArray();

        GeneratedMesh.RecalculateBounds();
        GeneratedMesh.RecalculateNormals();
    }

    public void Clear()
    {
        _vertices.Clear();
        _triangles.Clear();
    }

    public void AddMarker(Vector3 pos, float size)
    {
        if (_vertices.Count > 64000)
            return;

        int voffset = _vertices.Count;
        int toffset = _triangles.Count;

        _vertices.Add(pos + new Vector3(-size, -size, -size));
        _vertices.Add(pos + new Vector3(-size, -size, size));
        _vertices.Add(pos + new Vector3(size, -size, size));
        _vertices.Add(pos + new Vector3(size, -size, -size));
        _vertices.Add(pos + new Vector3(0, size, 0));

        _triangles.Add(voffset + 2);
        _triangles.Add(voffset + 1);
        _triangles.Add(voffset + 0);

        _triangles.Add(voffset + 0);
        _triangles.Add(voffset + 3);
        _triangles.Add(voffset + 2);

        _triangles.Add(voffset + 0);
        _triangles.Add(voffset + 4);
        _triangles.Add(voffset + 3);

        _triangles.Add(voffset + 3);
        _triangles.Add(voffset + 4);
        _triangles.Add(voffset + 2);

        _triangles.Add(voffset + 2);
        _triangles.Add(voffset + 4);
        _triangles.Add(voffset + 1);

        _triangles.Add(voffset + 1);
        _triangles.Add(voffset + 4);
        _triangles.Add(voffset + 0);

    }
}
