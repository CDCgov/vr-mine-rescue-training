using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using g3;
using Unity.Mathematics;

public class TileGeometryCut : MonoBehaviour, IMeshCut
{
    public Mesh CutMesh;

    private Vector3 _cutPosition;
    private Vector3 _cutScale;
    private Quaternion _cutRotation;

    //private Transform _target;

    private DMesh3 _dmCutMesh;
    //private Vector3[] _cutMeshVertices; //used for more accurate AABB bounds checking

    //private DMesh3 _dmCutMeshTransformed;
    //private List<Mesh> _originalMeshes;

    private Collider[] _colliders;
    private List<MeshFilter> _meshFilters;
    private int _layerMask;

    private List<MeshCut> _activeCutTargets;

    private System.Diagnostics.Stopwatch _stopwatch;

    public bool GetMeshCutInfo(out MeshCut.MeshCutInfo cutInfo)
    {
        if (_dmCutMesh == null)
        {
            cutInfo = default;
            return false;
        }

        cutInfo = new MeshCut.MeshCutInfo
        {
            CutMesh = _dmCutMesh,
            LocalToWorldMatrix = transform.localToWorldMatrix,
        };

        return true;
    }

    private void Awake()
    {
        _stopwatch = new System.Diagnostics.Stopwatch();

        _layerMask = LayerMask.GetMask("Floor");
        _colliders = new Collider[25];

        //_originalMeshes = new List<Mesh>();
        _meshFilters = new List<MeshFilter>();

        _activeCutTargets = new List<MeshCut>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _dmCutMesh = ProcGeometry.ConvertToDMesh(CutMesh);
        //_cutMeshVertices = CutMesh.vertices;

        UpdateCut();
    }

    private void OnEnable()
    {
        InvokeRepeating(nameof(CheckIfUpdateNeeded), 2.5f, 2.5f);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    // Update is called once per frame
    void CheckIfUpdateNeeded()
    {
        if (_cutPosition != transform.position || 
            _cutRotation != transform.rotation ||
            _cutScale != transform.lossyScale)
            UpdateCut();
    }

    private void UpdateCut()
    {
        _cutPosition = transform.position;
        _cutRotation = transform.rotation;
        _cutScale = transform.lossyScale;

        ClearActiveTargets();
        FindTargets();
    }

    private void ClearActiveTargets()
    {
        if (_activeCutTargets == null || _activeCutTargets.Count <= 0)
            return;

        foreach (var meshCut in _activeCutTargets)
        {
            meshCut.RemoveCutSource(gameObject);
        }

        _activeCutTargets.Clear();
    }


    private void FindTargets()
    {
        //var bounds = CutMesh.bounds;
        //var pos = transform.TransformPoint(bounds.center);
        //int numColliders = Physics.OverlapBoxNonAlloc(pos, bounds.extents, _colliders);


        //var bounds = GeometryUtility.CalculateBounds(_cutMeshVertices, transform.localToWorldMatrix);

        var floorLayer = LayerMask.NameToLayer("Floor");

        var bounds = CutMesh.bounds;
        var pos = transform.TransformPoint(bounds.center);
        var extents = Vector3.Scale(bounds.extents, transform.lossyScale);

        int numColliders = Physics.OverlapBoxNonAlloc(pos, extents, _colliders, transform.rotation, _layerMask);


        for (int i = 0; i < numColliders; i++)
        {
            var target = _colliders[i].gameObject;

            if (target.layer != floorLayer && !target.TryGetComponent<LODGroup>(out var lodGroup))
                continue;

            var tileCut = target.GetComponentInParent<TileGeometryCut>();
            if (tileCut != null)
                continue;

            target.TryGetComponent<MeshCollider>(out var meshCollider);

            _meshFilters.Clear();
            target.GetComponentsInChildren<MeshFilter>(_meshFilters);

            foreach (var filter in _meshFilters)
            {
                if (filter.gameObject.layer != floorLayer)
                    continue;

                MeshCut cut = null;
                if (!filter.TryGetComponent<MeshCut>(out cut))
                    cut = filter.gameObject.AddComponent<MeshCut>();

                Debug.Log($"Adding tile geometry cut from {gameObject.name} to {filter.name}");
                cut.AddCutSource(gameObject);
                _activeCutTargets.Add(cut);

                if (meshCollider != null && meshCollider.sharedMesh == filter.sharedMesh)
                {
                    Debug.Log($"TileGeometryCut: updating mesh collider for {meshCollider.name} with mesh {filter.name}");
                    cut.AssociatedMeshCollider = meshCollider;
                }
            }

        }
    }


    /*
    public void UpdateCut()
    {
        if (_dmCutMesh == null)
            return;

        _cutPosition = transform.position;
        _stopwatch.Reset();
        _stopwatch.Start();

        RestoreOriginalMeshes();

        _target = FindTarget();
        if (_target == null)
            return;

        _originalMeshes.Clear();
        _meshFilters.Clear();

        _target.GetComponentsInChildren<MeshFilter>(_meshFilters);        
        

        for (int i = 0; i < _meshFilters.Count; i++)
        {
            var filter = _meshFilters[i];

            _originalMeshes.Add(filter.sharedMesh);

            CutTargetMesh(filter);
        }

        _stopwatch.Stop();
        Debug.Log($"TileGeometryCut: Took {_stopwatch.ElapsedMilliseconds}ms to update cut");
    }

    private void CutTargetMesh(MeshFilter filter)
    {
        if (_dmCutMesh == null)
            return;

        if (!filter.sharedMesh.isReadable)
        {
            Debug.LogWarning($"Couldn't cut mesh {filter.name} - mesh not readable");
            return;
        }

        var dm = ProcGeometry.ConvertToDMesh(filter.sharedMesh, true);

        _dmCutMeshTransformed = new DMesh3(_dmCutMesh, false, false, false, false);

        //var localPos = filter.transform.InverseTransformPoint(transform.position);
        //MeshTransforms.Translate(_dmCutMeshTransformed, localPos);

        Matrix4x4 xform = filter.transform.worldToLocalMatrix * transform.localToWorldMatrix;
        ProcGeometry.ApplyTransform(_dmCutMeshTransformed, xform);

        var originalVertexCount = dm.VertexCount;

        ProcGeometry.MeshCut(dm, _dmCutMeshTransformed);

        Debug.Log($"Cut mesh {filter.name} from {originalVertexCount} to {dm.VertexCount} vertices");

        filter.sharedMesh = ProcGeometry.ConvertToUnityMesh(dm);
        //filter.sharedMesh.RecalculateNormals();
        filter.sharedMesh.RecalculateTangents();
    }

    void RestoreOriginalMeshes()
    {
        if (_meshFilters == null || _originalMeshes == null)
            return;

        for (int i = 0; i < _meshFilters.Count; i++)
        {
            if (_meshFilters[i] == null || _originalMeshes[i] == null)
                continue;

            _meshFilters[i].sharedMesh = _originalMeshes[i];
        }

        _meshFilters.Clear();
        _originalMeshes.Clear();
    }

    private Transform FindTarget()
    {
        var bounds = CutMesh.bounds;

        var pos = transform.TransformPoint(bounds.center);

        int numColliders = Physics.OverlapBoxNonAlloc(pos, bounds.extents, _colliders);

        float minDist = float.MaxValue;
        Transform target = null;

        for (int i = 0; i < numColliders; i++)
        {
            var dist = Vector3.Distance(transform.position, _colliders[i].transform.position);
            if (dist < minDist)
            {
                if (!_colliders[i].TryGetComponent<LODGroup>(out var lodGroup))
                    continue;

                minDist = dist;
                target = _colliders[i].transform;
            }
        }

        return target;
    }
    */

}
