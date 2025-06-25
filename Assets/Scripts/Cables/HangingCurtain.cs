using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangingCurtain : HangingGeometry
{
    private ClothSkinningCoefficient[] _clothCoeffs;

    protected override void GenerateMeshFromSmoothedPoints()
    {
        if (GeneratedMesh == null)
            GeneratedMesh = new Mesh();

        if (GeneratedMesh.name == null || GeneratedMesh.name.Length < 5)
        {
            System.Guid id = System.Guid.NewGuid();
            GeneratedMesh.name = "CurtainMesh_" + id.ToString();
        }

        Vector3[] vertices = GeneratedMesh.vertices;
        int[] triangles = GeneratedMesh.triangles;
        Vector2[] uv = GeneratedMesh.uv;

        List<Vector3> heights = new List<Vector3>(_smoothedPoints.Count);
        for (int i = 0; i < _smoothedPoints.Count; i++)
        {
            Vector3 worldPos = transform.TransformPoint(_smoothedPoints[i]);
            float dist = DistToGround(worldPos);
            heights.Add(new Vector3(0, -dist, 0));
        }		

        //ProcGeometry.GenerateTube(_smoothedPoints, CableDiameter / 2.0f, ref vertices, ref triangles, ref uv);
        ProcGeometry.GenerateCurtain(_smoothedPoints, heights, 5, ref vertices, ref triangles, ref uv, ref _clothCoeffs);

        GeneratedMesh.Clear();
        GeneratedMesh.vertices = vertices;
        GeneratedMesh.triangles = triangles;
        GeneratedMesh.uv = uv;

        GeneratedMesh.RecalculateNormals();
        GeneratedMesh.RecalculateBounds();		

        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null)
            mf.sharedMesh = GeneratedMesh;

        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
        if (smr != null)
        {
            smr.sharedMesh = GeneratedMesh;
        }

        Cloth cloth = GetComponent<Cloth>();
        if (cloth != null && _clothCoeffs.Length == vertices.Length)
        {
            cloth.coefficients = _clothCoeffs;
        }
    }
}
