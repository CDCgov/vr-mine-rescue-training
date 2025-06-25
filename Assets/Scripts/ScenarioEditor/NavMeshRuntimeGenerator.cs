using System;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using static UnityEngine.Rendering.HableCurve;
using System.Linq;

[RequireComponent(typeof(NavMeshSurface))]
public class NavMeshRuntimeGenerator : MonoBehaviour
{
    public const int MaxNavMeshVerts = 3000000;

    public Action NavMeshBuilt;
    public bool NavMeshReady { get
        {
            return _navMeshReady;
        } 
    }

    private NavMeshSurface _surface;
    private bool _navMeshReady = false;

    private List<MeshFilter> _filters = new List<MeshFilter>();
    private List<LODGroup> _lodGroups = new List<LODGroup>();
    private List<Collider> _colliders = new List<Collider>();

    private void Start()
    {
        _surface = GetComponent<NavMeshSurface>();
        if (_surface == null)
        {
            Debug.LogError($"No NavMeshSurface on {gameObject.name}");
            this.enabled = false;
            Destroy(this);
        }
    }

    //private void OnEnable()
    //{
    //    ScenarioSaveLoad.Instance.onLoadComplete += GenerateRuntimeMesh;
    //}

    //private void OnDisable()
    //{
    //    if (ScenarioSaveLoad.Instance != null)
    //        ScenarioSaveLoad.Instance.onLoadComplete -= GenerateRuntimeMesh;
    //}

    //private void OnDestroy()
    //{
    //    if (ScenarioSaveLoad.Instance != null)
    //        ScenarioSaveLoad.Instance.onLoadComplete -= GenerateRuntimeMesh;
    //}

    public void StartNavMeshGeneration()
    {
        StartCoroutine(GenerateRuntimeMesh());
    }
    
    private bool AddNavMeshToLODs(GameObject obj)
    {
        _lodGroups.Clear();
        obj.GetComponentsInChildren<LODGroup>(_lodGroups);

        if (_lodGroups.Count <= 0)
            return false;

        foreach (var lodGroup in _lodGroups)
        {
            var lods = lodGroup.GetLODs();
            if (lods == null || lods.Length <= 0)
                continue;

            //var lod = lods[lods.Length - 1];
            var lod = lods[0];
            if (lod.renderers == null || lod.renderers.Length <= 0)
                continue;

            foreach (var rend in lod.renderers)
            {
                if (rend.TryGetComponent<MeshFilter>(out var filter))
                    AddNavMeshModifier(filter);
                else
                    AddNavMeshModifier(rend.gameObject, true);
            }
        }

        _lodGroups.Clear();

        return true;
    }

    private bool AddNavMeshToColliders(GameObject obj)
    {
        _colliders.Clear();
        obj.GetComponentsInChildren<Collider>(_colliders);

        if (_colliders.Count <= 0)
            return false;

        foreach (var collider in _colliders)
        {
            if (collider.isTrigger)
                continue;

            bool includeInMesh = true;
            var meshCollider = collider as MeshCollider;
            
            if (meshCollider != null)
            {
                if (meshCollider.sharedMesh.vertexCount > MaxNavMeshVerts)
                {
                    Debug.LogWarning($"NavMesh ignoring mesh on {gameObject.name} vertices: {meshCollider.sharedMesh.vertexCount}");
                    includeInMesh = false;
                }
            }

            AddNavMeshModifier(collider.gameObject, includeInMesh, false);
        }

        _colliders.Clear();
        return true;
    }

    private void AddNavMeshToAll(GameObject obj)
    {
        _filters.Clear();
        obj.GetComponentsInChildren<MeshFilter>(_filters);

        foreach (var filter in _filters)
        {
            AddNavMeshModifier(filter);
        }

        _filters.Clear();
    }

    private void AddNavMeshModifier(MeshFilter filter)
    {
        bool includeInMesh = true;
        if (filter.sharedMesh.vertexCount > MaxNavMeshVerts)
        {
            Debug.LogWarning($"NavMesh ignoring mesh on {gameObject.name} vertices: {filter.sharedMesh.vertexCount}");
            includeInMesh = false;
        }

        AddNavMeshModifier(filter.gameObject, includeInMesh);
    }

    private void AddNavMeshModifier(GameObject obj, bool includeInMesh, bool overrideExisting = true)
    {
        NavMeshModifier mod;
        if (!obj.TryGetComponent<NavMeshModifier>(out mod))
        {
            mod = obj.gameObject.AddComponent<NavMeshModifier>();
        }
        else if (!overrideExisting)
        {
            //existing modifier exists
            return;
        }

        mod.overrideArea = false;
        mod.applyToChildren = false;

        mod.ignoreFromBuild = !includeInMesh;
    }

    public IEnumerator GenerateRuntimeMesh()
    {
        //_surface = FindObjectOfType<NavMeshSurface>();
        //if (_surface == null)
        //{
        //    _surface = gameObject.AddComponent<NavMeshSurface>();
        //    _surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        //}

        _navMeshReady = false;

        var mineSegments = FindObjectsOfType<MineSegment>();
        foreach (var segment in mineSegments)
        {
            //NavMeshModifier mod;
            //if (!segment.TryGetComponent<NavMeshModifier>(out mod))
            //{
            //    mod = segment.gameObject.AddComponent<NavMeshModifier>();
            //}

            //mod.overrideArea = false;
            //mod.applyToChildren = true;
            //mod.ignoreFromBuild = false;

            //BuildNavMesh(mod);

            AddNavMeshModifier(segment.gameObject, false);

            if (segment.BuildNavMesh)
            {
                if (!AddNavMeshToColliders(segment.gameObject))
                    AddNavMeshToAll(segment.gameObject);
            }
        }

        _surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        _surface.collectObjects = CollectObjects.MarkedWithModifier;
        //Debug.Log($"Navmesh Build Start time! {Time.time}");
        _surface.buildHeightMesh = true;
        _surface.overrideVoxelSize = true;
        _surface.voxelSize = 0.05f;

        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        //var startTime = Time.time;
        Debug.Log($"Starting nav mesh build frame:{Time.frameCount}");

        //_surface.BuildNavMesh();
        yield return BuildNavMesh();

        //var elapsed = Time.time - startTime;
        sw.Stop();
        Debug.Log($"Finished nav mesh build frame:{Time.frameCount} {sw.ElapsedMilliseconds}ms");

        yield return null;

        //Debug.Log($"Navmesh Build End time! {Time.time}");
        _navMeshReady = true;
        NavMeshBuilt?.Invoke();
    }

    private IEnumerator BuildNavMesh()
    {
        if (_surface.navMeshData != null)
        {
            _surface.RemoveData();
        }

        //_surface.AddData();
        _surface.navMeshData = new NavMeshData();
        _surface.navMeshData.position = Vector3.zero;
        _surface.navMeshData.rotation = Quaternion.identity;

        var asyncOp = _surface.UpdateNavMesh(_surface.navMeshData);
        while (!asyncOp.isDone)
            yield return null;

        _surface.AddData();

    }

    private void BuildNavMesh(NavMeshModifier modifier)
    {
        var surface = modifier.gameObject.AddComponent<NavMeshSurface>();

        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.collectObjects = CollectObjects.Children;
        surface.buildHeightMesh = true;
        surface.overrideVoxelSize = true;
        surface.voxelSize = 0.05f;

        surface.BuildNavMesh();
    }

}
