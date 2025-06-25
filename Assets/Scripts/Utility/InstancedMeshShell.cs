using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancedMeshShell
{
    private class InstanceBuffer
    {
        public Vector4[] positions;
        public Matrix4x4[] matrices;
        public Matrix4x4[] localMats;
        public int count;

        public InstanceBuffer()
        {
            positions = new Vector4[MAX_INSTANCE_COUNT];
            matrices = new Matrix4x4[MAX_INSTANCE_COUNT];
            localMats = new Matrix4x4[MAX_INSTANCE_COUNT];
            count = 0;

            for (int i = 0; i < MAX_INSTANCE_COUNT; i++)
            {
                matrices[i] = Matrix4x4.identity;
            }
        }

        public void AddPosition(Vector3 pos)
        {
            positions[count] = pos;
            count++;
        }
    }

    public const int MAX_INSTANCE_COUNT = 1023;

    private Mesh _markerMesh;
    private List<InstanceBuffer> _buffers;

    public InstancedMeshShell()
    {
        _buffers = new List<InstanceBuffer>();
    }

    public void Clear()
    {
        for (int i = 0; i < _buffers.Count; i++)
        {
            _buffers[i].count = 0;
        }
    }

    public void AddMarker(Vector3 pos)
    {
        InstanceBuffer buffer = FindFreeBuffer();
        buffer.AddPosition(pos);
    }

    public void DrawShell(Vector3 worldSpaceCenter, Matrix4x4 localToWorld, Material mat, MaterialPropertyBlock mpb, int layer = 0)
    {
        if (_markerMesh == null)
            BuildMarkerMesh();

        //update the bounding box of the mesh to the position of the shell, since we are passing identity matrices to the shader
        //to boost performance
        Bounds b = _markerMesh.bounds;
        b.center = worldSpaceCenter;
        b.extents = new Vector3(25, 25, 25);
        _markerMesh.bounds = b;

        for (int i = 0; i < _buffers.Count; i++)
        {
            if (_buffers[i].count > 0)
            {
                DrawBuffer(_buffers[i], localToWorld, mat, mpb, layer);
            }
        }
    }

    private void DrawBuffer(InstanceBuffer buffer, Matrix4x4 localToWorld, Material mat, MaterialPropertyBlock mpb, int layer = 0)
    {
        Matrix4x4 posMat = Matrix4x4.identity;		

        

        mpb.SetMatrix("_TransformMat", localToWorld);
        /*

        //update the list of transform matrices
        for (int i = 0; i < buffer.count; i++)
        {
            //posMat = Matrix4x4.TRS(buffer.positions[i], Quaternion.identity, Vector3.one);
            //buffer.matrices[i] = localToWorld * posMat;

            
            Vector3 pos = buffer.positions[i];
            //posMat.m03 = pos.x;
            //posMat.m13 = pos.y;
            //posMat.m23 = pos.z;
            //buffer.localMats[i] = posMat;

            //posMat.m03 = pos.x;
            //posMat.m13 = pos.y;
            //posMat.m23 = pos.z;
            //posMat.SetColumn(3, new Vector4(pos.x, pos.y, pos.z, 1));
            //buffer.localMats[i] = posMat;

            //buffer.localMats[i].SetColumn(3, new Vector4(pos.x, pos.y, pos.z, 1));
            //buffer.localMats[i].m03 = pos.x;
            //buffer.localMats[i].m13 = pos.y;
            //buffer.localMats[i].m23 = pos.z;

            //buffer.matrices[i] = localToWorld * posMat;
            buffer.matrices[i] = localToWorld;
            //buffer.matrices[i] = posMat;
            
            
            
            //Vector3 pos = buffer.positions[i];
            //buffer.matrices[i] = localToWorld;
            //buffer.matrices[i].m03 += pos.x;
            //buffer.matrices[i].m13 += pos.y;
            //buffer.matrices[i].m23 += pos.z;
            
            //if (!Application.isPlaying)
            //{
            //	//Graphics.DrawMesh(_markerMesh, buffer.matrices[i], mat, 0, null, 0, mpb);
            //	//Graphics.DrawMeshNow(_markerMesh, buffer.matrices[i]);			

            //	//posMat = Matrix4x4.TRS(buffer.positions[i], Quaternion.identity, Vector3.one);
            //	//buffer.matrices[i] = localToWorld * posMat;	

            //	Gizmos.matrix = buffer.matrices[i];
            //	Gizmos.DrawCube(Vector3.zero, new Vector3(0.02f, 0.02f, 0.02f));
            //}
        } */

        //mpb = new MaterialPropertyBlock();
        //mpb.SetMatrix("_TransformMat", localToWorld);
        //mpb.SetColor("_Color", Color.magenta);

        //mpb.SetMatrixArray("_LocalTransform", buffer.localMats);
        mpb.SetVectorArray("_Position", buffer.positions);

        //if (Application.isPlaying)
        
        Graphics.DrawMeshInstanced(_markerMesh, 0, mat, buffer.matrices, buffer.count, mpb);
    }

    private InstanceBuffer FindFreeBuffer()
    {
        for (int i = 0; i < _buffers.Count; i++)
        {
            if (_buffers[i].count < MAX_INSTANCE_COUNT)
                return _buffers[i];
        }

        return AllocateNewBuffer();
    }

    private InstanceBuffer AllocateNewBuffer()
    {
        InstanceBuffer buffer = new InstanceBuffer();
        _buffers.Add(buffer);
        return buffer;
    }

    private void BuildMarkerMesh()
    {
        DynamicMesh dm = new DynamicMesh();
        dm.AddMarker(Vector3.zero, 0.02f);
        dm.UpdateMesh();

        dm.GeneratedMesh.bounds = new Bounds(Vector3.zero, new Vector3(100,100,100));
        
        _markerMesh = dm.GeneratedMesh;
    }



}
