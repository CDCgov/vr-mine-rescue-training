using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryObjectInfo : MonoBehaviour
{
    public List<MeshCollider> MeshColliders;
    public List<Collider> OtherColliders;

    private void Awake()
    {
        MeshColliders = new List<MeshCollider>();
        OtherColliders = new List<Collider>();
    }

    public void AddMeshCollider(MeshCollider collider)
    {
        if (MeshColliders == null)
            MeshColliders = new List<MeshCollider>();

        MeshColliders.Add(collider);
    }

}
