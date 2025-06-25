using UnityEngine;
using System.Collections.Generic;
using g3;

public static class ProcGeometry
{
    private static Vector3[] tubeOffsetVectors;
    private const int numTubeVertices = 6;

    public static void GenerateTube(List<Vector3> path, float radius, ref Vector3[] vertices, ref int[] triangles, ref Vector2[] uv)
    {
        //initialize offset vectors if necessary
        if (tubeOffsetVectors == null)
        {
            tubeOffsetVectors = new Vector3[numTubeVertices];
            Vector3 vStart = Vector3.up;
            tubeOffsetVectors[0] = vStart;

            for (int i = 1; i < numTubeVertices; i++)
            {
                Quaternion rot = Quaternion.AngleAxis((360.0f / (float)numTubeVertices) * i, Vector3.forward);
                tubeOffsetVectors[i] = rot * vStart;
            }
        }

        //determine the vertex & triangle count and initialize arrays
        int totalVertexCount = path.Count * numTubeVertices;
        if (vertices.Length != totalVertexCount)
            vertices = new Vector3[totalVertexCount];
        if (uv.Length != totalVertexCount)
            uv = new Vector2[totalVertexCount];

        int totalTriangleCount = (path.Count - 1) * numTubeVertices * 2 * 3;
        if (triangles.Length != totalTriangleCount)
            triangles = new int[totalTriangleCount];

        //first compute vertex positions
        Vector3 vdir = path[1] - path[0]; //initial direction of the tube
        Quaternion dir = Quaternion.LookRotation(vdir, Vector3.up);
        Quaternion nextDir, midDir;
        Vector3 nextVDir;
        Vector3 up = Vector3.up;

        float dist = 0;

        //add first vertex set
        ComputeTubeVertices(path[0], dist, radius, dir, 0, ref vertices, ref uv);
        dist += vdir.magnitude;
        nextDir = dir;

        for (int i = 1; i < path.Count - 1; i++)
        {
            nextVDir = path[i + 1] - path[i];
            if (nextVDir.magnitude > 0.0001f)
            {
                up = dir * Vector3.up;
                nextDir = Quaternion.LookRotation(nextVDir, up);
            }

            midDir = Quaternion.Slerp(dir, nextDir, 0.5f);
            ComputeTubeVertices(path[i], dist, radius, midDir, i * numTubeVertices, ref vertices, ref uv);
            dist += nextVDir.magnitude;

            dir = nextDir;
        }

        //add final set
        ComputeTubeVertices(path[path.Count - 1], dist, radius, dir, (path.Count - 1) * numTubeVertices, ref vertices, ref uv);

        //create triangle list
        for (int i = 0; i < path.Count - 1; i++)
        {
            for (int j = 0; j < numTubeVertices; j++)
            {
                int nextJ;
                if (j == numTubeVertices - 1)
                    nextJ = 0;
                else
                    nextJ = j + 1;

                int triStart = i * numTubeVertices * 2 * 3;

                triangles[triStart + j * 6 + 0] = (i + 1) * numTubeVertices + nextJ;
                triangles[triStart + j * 6 + 1] = (i + 1) * numTubeVertices + j;
                triangles[triStart + j * 6 + 2] = i * numTubeVertices + j;

                triangles[triStart + j * 6 + 3] = i * numTubeVertices + nextJ;
                triangles[triStart + j * 6 + 4] = (i + 1) * numTubeVertices + nextJ;
                triangles[triStart + j * 6 + 5] = i * numTubeVertices + j;
            }
        }
    }

    public static void GenerateTube(LinkedList<Vector3> path, float radius, ref Vector3[] vertices, ref int[] triangles, ref Vector2[] uv)
    {
        int i;
        if (path.Count < 2)
            return;

        //initialize offset vectors if necessary
        if (tubeOffsetVectors == null)
        {
            tubeOffsetVectors = new Vector3[numTubeVertices];
            Vector3 vStart = Vector3.up;
            tubeOffsetVectors[0] = vStart;

            for (i = 1; i < numTubeVertices; i++)
            {
                Quaternion rot = Quaternion.AngleAxis((360.0f / (float)numTubeVertices) * i, Vector3.forward);
                tubeOffsetVectors[i] = rot * vStart;
            }
        }

        //determine the vertex & triangle count and initialize arrays
        int totalVertexCount = path.Count * numTubeVertices;
        if (vertices.Length != totalVertexCount)
            vertices = new Vector3[totalVertexCount];
        if (uv.Length != totalVertexCount)
            uv = new Vector2[totalVertexCount];

        int totalTriangleCount = (path.Count - 1) * numTubeVertices * 2 * 3;
        if (triangles.Length != totalTriangleCount)
            triangles = new int[totalTriangleCount];

        //first compute vertex positions
        Vector3 vdir = path.First.Next.Value - path.First.Value; //initial direction of the tube
        Quaternion dir = Quaternion.LookRotation(vdir, Vector3.up);
        Quaternion nextDir, midDir;
        Vector3 nextVDir;
        Vector3 up = Vector3.up;

        float dist = 0;

        //add first vertex set
        ComputeTubeVertices(path.First.Value, dist, radius, dir, 0, ref vertices, ref uv);
        dist += vdir.magnitude;
        nextDir = dir;

        LinkedListNode<Vector3> node = path.First.Next;
        i = 1;
        while (node != null && node.Next != null)
        {
            nextVDir = node.Next.Value - node.Value;
            if (nextVDir.magnitude > 0.0001f)
            {
                up = dir * Vector3.up;
                nextDir = Quaternion.LookRotation(nextVDir, up);
            }

            midDir = Quaternion.Slerp(dir, nextDir, 0.5f);
            ComputeTubeVertices(node.Value, dist, radius, midDir, i * numTubeVertices, ref vertices, ref uv);
            dist += nextVDir.magnitude;

            dir = nextDir;

            node = node.Next;
            i++;
        }

        //add final set
        ComputeTubeVertices(path.Last.Value, dist, radius, dir, (path.Count - 1) * numTubeVertices, ref vertices, ref uv);

        //create triangle list
        for (i = 0; i < path.Count - 1; i++)
        {
            for (int j = 0; j < numTubeVertices; j++)
            {
                int nextJ;
                if (j == numTubeVertices - 1)
                    nextJ = 0;
                else
                    nextJ = j + 1;

                int triStart = i * numTubeVertices * 2 * 3;

                triangles[triStart + j * 6 + 0] = (i + 1) * numTubeVertices + nextJ;
                triangles[triStart + j * 6 + 1] = (i + 1) * numTubeVertices + j;
                triangles[triStart + j * 6 + 2] = i * numTubeVertices + j;

                triangles[triStart + j * 6 + 3] = i * numTubeVertices + nextJ;
                triangles[triStart + j * 6 + 4] = (i + 1) * numTubeVertices + nextJ;
                triangles[triStart + j * 6 + 5] = i * numTubeVertices + j;
            }
        }
    }

    private static void ComputeTubeVertices(Vector3 pos, float dist, float radius, Quaternion dir, int startIndex, ref Vector3[] vertices, ref Vector2[] uv)
    {
        Vector2 uvcoord;
        uvcoord.x = dist;

        for (int i = 0; i < numTubeVertices; i++)
        {
            //add the offset vector rotated to be perpendicular to the direction of the tube
            vertices[i + startIndex] = pos + (dir * tubeOffsetVectors[i] * radius);

            uvcoord.y = (float)i / (float)(numTubeVertices);

            uv[i + startIndex] = uvcoord;
        }
    }


    public static void GeneratePlane(Vector3 topLeft, Vector3 bottomRight, int numSegX, int numSegY, ref Vector3[] vertices, ref int[] triangles, ref Vector2[] uv)
    {
        int numVertsX = numSegX + 1;
        int numVertsY = numSegY + 1;

        int vertexCount = numVertsX * numVertsY;
        if (vertices == null || vertices.Length != vertexCount)
            vertices = new Vector3[vertexCount];

        if (uv == null || uv.Length != vertexCount)
            uv = new Vector2[vertexCount];

        Vector3 dir = bottomRight - topLeft;
        float height = dir.y;
        dir.y = 0;


        for (int i = 0; i < numVertsX; i++)
        {
            for (int j = 0; j < numVertsY; j++)
            {
                float xMult = (float)i / (float)numSegX;
                float yMult = (float)j / (float)numSegY;

                Vector3 pos = topLeft + dir * xMult;
                pos.y = topLeft.y + height * yMult;
                int index = i * numVertsY + j;

                vertices[index] = pos;
                uv[index] = new Vector2(xMult, yMult);
            }


        }

        int numTris = numSegX * numSegY * 2;
        if (triangles == null || triangles.Length != numTris)
            triangles = new int[numTris * 3];

        for (int i = 0; i < numSegX; i++)
        {
            for (int j = 0; j < numSegY; j++)
            {
                int c = i * (numSegX * 2) * 3 + j * 2 * 3; //first corner index in the triangle array
                int v1 = i * numVertsY + j;
                int v2 = i * numVertsY + j + 1;
                int v3 = (i + 1) * numVertsY + j + 1;
                int v4 = (i + 1) * numVertsY + j;

                triangles[c + 0] = v1;
                triangles[c + 1] = v2;
                triangles[c + 2] = v3;

                triangles[c + 3] = v1;
                triangles[c + 4] = v3;
                triangles[c + 5] = v4;
            }
        }
    }

    public static void GenerateCurtain(List<Vector3> path, List<Vector3> heights, int numSegments, ref Vector3[] vertices, ref int[] triangles, ref Vector2[] uv, ref ClothSkinningCoefficient[] coeffs)
    {
        if (path == null || heights == null || path.Count != heights.Count)
            return;

        //numSegments + 1 vertices per entry in the path array
        int vertexCount = (numSegments + 1) * path.Count;
        if (vertices == null || vertices.Length != vertexCount)
            vertices = new Vector3[vertexCount];

        if (uv == null || uv.Length != vertexCount)
            uv = new Vector2[vertexCount];

        if (coeffs == null || coeffs.Length != vertexCount)
            coeffs = new ClothSkinningCoefficient[vertexCount];


        ClothSkinningCoefficient fixedCoeff = new ClothSkinningCoefficient();
        ClothSkinningCoefficient limitedCoeff = new ClothSkinningCoefficient();
        ClothSkinningCoefficient normalCoeff = new ClothSkinningCoefficient();

        float collisionSphereDist = 0.2f;
        fixedCoeff.collisionSphereDistance = collisionSphereDist;
        normalCoeff.collisionSphereDistance = collisionSphereDist;
        limitedCoeff.collisionSphereDistance = collisionSphereDist;

        fixedCoeff.maxDistance = 0;
        normalCoeff.maxDistance = 4.0f;
        limitedCoeff.maxDistance = 0.3f;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 top = path[i];
            Vector3 height = heights[i];



            for (int j = 0; j < (numSegments + 1); j++)
            {
                Vector3 pos = top + height * ((float)j / (float)numSegments);
                int index = i * (numSegments + 1) + j;
                vertices[index] = pos;
                uv[index] = new Vector2((float)i / 16.0f, (float)j / (float)numSegments);

                if (j == 0)
                    coeffs[index] = fixedCoeff;
                else
                    coeffs[index] = normalCoeff;
            }


        }

        //numSegments * 2 triangles per column, path.Count - 1 columns
        int numTris = (numSegments * 2) * (path.Count - 1);
        if (triangles == null || triangles.Length != numTris)
            triangles = new int[numTris * 3];

        for (int i = 1; i < path.Count; i++)
        {
            int c1_top = (i - 1) * (numSegments + 1); //top vertex in column 1
            int c2_top = c1_top + (numSegments + 1); //top vertex in column 2

            for (int j = 0; j < numSegments; j++)
            {
                int c = (i - 1) * (numSegments * 2) * 3 + j * 2 * 3; //first corner index in the triangle array

                int c1 = c1_top + j;
                int c2 = c2_top + j;

                triangles[c + 0] = c1;
                triangles[c + 1] = c2;
                triangles[c + 2] = c2 + 1;

                triangles[c + 3] = c1;
                triangles[c + 4] = c2 + 1;
                triangles[c + 5] = c1 + 1;
            }
        }
    }

    public static DMesh3 ConvertToDMesh(Mesh m, bool includeUV = false)
    {
        var dm = g3UnityUtils.UnityMeshToDMesh(m, includeUV);
        dm.EnableVertexUVs(new Vector2f(0, 0));

        return dm;
    }

    public static Mesh ConvertToUnityMesh(DMesh3 dm, bool compactMesh = true)
    {
        if (compactMesh)
            dm = new DMesh3(dm, true, true, false, true);

        return g3UnityUtils.DMeshToUnityMesh(dm);
    }

    public static Mesh BoxCutXY(Mesh m, Bounds cutBounds, float metersPerTile)
    {
        var dm = ConvertToDMesh(m);

        BoxCutXY(dm, cutBounds);              
        PlanarUnwrapXY(dm, metersPerTile);

        return ConvertToUnityMesh(dm);
    }

    public static void BoxCutXY(DMesh3 mesh, Bounds cutBounds)
    {
        TrivialBox3Generator box3 = new TrivialBox3Generator();
        box3.Box = new Box3d(cutBounds.center, cutBounds.extents);
        box3.NoSharedVertices = false;
        var boxMesh = box3.Generate().MakeDMesh();

        MeshMeshCut cut = new MeshMeshCut();
        cut.CutMesh = boxMesh;
        cut.Target = mesh;
        cut.VertexSnapTol = 0.00001;
        cut.Compute();
        cut.RemoveContained();
    }

    public static void MeshCut(DMesh3 mesh, Mesh cutMesh)
    {
        var dmCutMesh = ConvertToDMesh(cutMesh);
        MeshCut(mesh, dmCutMesh);
    }

    public static void MeshCut(DMesh3 mesh, DMesh3 cutMesh)
    {
        MeshMeshCut cut = new MeshMeshCut();
        cut.CutMesh = cutMesh;
        cut.Target = mesh;
        cut.VertexSnapTol = 0.00001;
        cut.Compute();
        cut.RemoveContained();
    }

    public static void ApplyScale(DMesh3 mesh, Vector3 scale, Vector3 origin)
    {
        MeshTransforms.Scale(mesh, scale, origin);
    }

    public static void ApplyTransform(DMesh3 mesh, Matrix4x4 mat)
    {
        foreach (int vid in mesh.VertexIndices())
        {
            Vector3 v = (Vector3)mesh.GetVertex(vid);

            v = mat.MultiplyPoint(v);

            mesh.SetVertex(vid, v);
        }
    }

    public static void BoxCutXY(Mesh m, Bounds cutBounds, List<Vector3> vertexBuffer, List<int> indexBuffer,
        List<Vector3> newVertexBuffer, List<int> newIndexBuffer, List<Vector2> uvBuffer, float tilesPerMeter)
    {
        if (vertexBuffer == null)
            vertexBuffer = new List<Vector3>();
        if (indexBuffer == null)
            indexBuffer = new List<int>();
        if (newIndexBuffer == null)
            newIndexBuffer = new List<int>();
        if (newVertexBuffer == null)
            newVertexBuffer = new List<Vector3>();
        if (uvBuffer == null)
            uvBuffer = new List<Vector2>();

        if (vertexBuffer.Count <= 0)
            m.GetVertices(vertexBuffer);
        if (indexBuffer.Count <= 0)
            m.GetIndices(indexBuffer, 0);
        if (uvBuffer.Count != vertexBuffer.Count)
            m.GetUVs(0, uvBuffer);
    }

    public static void FastBoxCutXY(Mesh m, Bounds cutBounds, List<Vector3> vertexBuffer, List<int> indexBuffer,
        List<Vector3> newVertexBuffer, List<int> newIndexBuffer, List<Vector2> uvBuffer, float tilesPerMeter)
    {
        HashSet<int> displacedVerts = new HashSet<int>();

        if (vertexBuffer == null)
            vertexBuffer = new List<Vector3>();
        if (indexBuffer == null)
            indexBuffer = new List<int>();
        if (newIndexBuffer == null)
            newIndexBuffer = new List<int>();
        if (newVertexBuffer == null)
            newVertexBuffer = new List<Vector3>();
        if (uvBuffer == null)
            uvBuffer = new List<Vector2>();


        if (vertexBuffer.Count <= 0)
            m.GetVertices(vertexBuffer);

        if (indexBuffer.Count <= 0)
            m.GetIndices(indexBuffer, 0);

        if (uvBuffer.Count != vertexBuffer.Count)
            m.GetUVs(0, uvBuffer);


        newIndexBuffer.Clear();
        newVertexBuffer.Clear();

        var center = cutBounds.center;
        var extent = cutBounds.extents;

        for (int i = 0; i < vertexBuffer.Count; i++)
        {
            var v = vertexBuffer[i];

            if (cutBounds.Contains(v))
            {
                if (v.x > center.x)
                    v.x = center.x + extent.x;
                else
                    v.x = center.x - extent.x;

                //if (v.y > center.y)
                //    v.y = center.y + extent.y;
                //else
                //    v.y = center.y - extent.y;

                displacedVerts.Add(i);
            }

            //vertexBuffer[i] = v;
            newVertexBuffer.Add(v);
        }

        for (int i = 0; i <= indexBuffer.Count - 3; i += 3)
        {
            if (displacedVerts.Contains(indexBuffer[i]) &&
                displacedVerts.Contains(indexBuffer[i + 1]) &&
                displacedVerts.Contains(indexBuffer[i + 2]))
            {
                continue;
            }

            newIndexBuffer.Add(indexBuffer[i]);
            newIndexBuffer.Add(indexBuffer[i + 1]);
            newIndexBuffer.Add(indexBuffer[i + 2]);
        }

        PlanarUnwrapXY(newVertexBuffer, uvBuffer, tilesPerMeter);

        m.SetVertices(newVertexBuffer);
        m.SetIndices(newIndexBuffer, MeshTopology.Triangles, 0, false);
        m.SetUVs(0, uvBuffer);
    }

    public static void PlanarUnwrapXY(List<Vector3> vertices, List<Vector2> uvCoords, float tilesPerMeter)
    {
        if (vertices == null || uvCoords == null)
            return;

        int numVertices = Mathf.Min(vertices.Count, uvCoords.Count);
        for (int i = 0; i < numVertices; i++)
        {
            var v = vertices[i];
            Vector2 uv = new Vector2();
            uv.x = v.x / tilesPerMeter;
            uv.y = v.y / tilesPerMeter;

            uvCoords[i] = uv;
        }
    }

    public static void PlanarUnwrapXY(DMesh3 mesh, float metersPerTile)
    {
        //if (mesh == null || mesh.VerticesBuffer == null || mesh.UVBuffer == null || 
        //    mesh.VerticesBuffer.Length != mesh.UVBuffer.Length)
        //{
        //    Debug.LogError("Error unwrapping DMesh3");
        //    return;
        //}

        //var uv = mesh.UVBuffer;
        //var verts = mesh.VerticesBuffer;
        //int numVertices = Mathf.Min(uv.Length, verts.Length);

        foreach (var vi in mesh.VertexIndices())
        {
            var v = mesh.GetVertex(vi);

            Vector2f uv = new Vector2f();
            uv.x = (float)v.x / metersPerTile;
            uv.y = (float)v.y / metersPerTile;

            mesh.SetVertexUV(vi, uv);
        }
    }

    public static Mesh CutMesh(Mesh m, Mesh cut)
    {
        DMesh3 dm = g3UnityUtils.UnityMeshToDMesh(m);
        DMesh3 dmcut = g3UnityUtils.UnityMeshToDMesh(cut);

        MeshMeshCut meshcut = new MeshMeshCut();
        meshcut.CutMesh = dmcut;
        meshcut.Target = dm;

        meshcut.Compute();
        meshcut.RemoveContained();

        dm = new DMesh3(dm, true, true, false, true);

        return g3UnityUtils.DMeshToUnityMesh(dm);
    }

    public static void asdf(DMesh3 mesh)
    {
        NewVertexInfo v;

        //mesh.AppendVertex()
    }

}
