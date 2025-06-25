using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using g3;

[RequireComponent(typeof(MeshFilter))]
public class MeshCut : MonoBehaviour
{
    public struct MeshCutInfo
    {
        public DMesh3 CutMesh;
        public Matrix4x4 LocalToWorldMatrix;
    }

    public MeshCollider AssociatedMeshCollider;

    private Mesh _originalMesh;
    private DMesh3 _originalDmesh3;
    private DMesh3 _processedMesh;
    private MeshFilter _meshFilter;

    private Collider[] _colliders;
    private int _layerMask;

    private List<GameObject> _cutSources;
    private List<MeshCutInfo> _meshCutInfo;

    private bool _cutUpdateNeeded = true;
    private CancellationTokenSource _cancelSource;
    private Task _updateTask = null;

    private System.Diagnostics.Stopwatch _stopwatch;

    private static int _lastMeshUpdateCompletionFrame = -1;

    public void AddCutSource(GameObject source)
    {
        _cutSources.Add(source);
        _cutUpdateNeeded = true;
    }

    public void RemoveCutSource(GameObject source)
    {
        _cutSources.Remove(source);
        _cutUpdateNeeded = true;
    }

    public void FlagForCutUpdate()
    {
        _cutUpdateNeeded = true;
    }

    private void Awake()
    {
        _stopwatch = new System.Diagnostics.Stopwatch();
        _cutSources = new List<GameObject>();
        _meshCutInfo = new List<MeshCutInfo>();
        _cancelSource = new CancellationTokenSource();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!TryGetComponent<MeshFilter>(out _meshFilter))
        {
            Debug.LogError($"Couldn't find MeshFilter for MeshCut on {gameObject.name}");
            this.enabled = false;
            return;
        }

        _originalMesh = _meshFilter.sharedMesh;
        _originalDmesh3 = null;

        if (!_originalMesh.isReadable)
        {
            Debug.LogError($"Unable to cut mesh for {gameObject.name} - mesh not readable");
            this.enabled = false;
            return;
        }

        _cutUpdateNeeded = true;
    }

    private void OnDestroy()
    {
        _cancelSource.Cancel();
        _cancelSource.Dispose();
    }

    private void LateUpdate()
    {
        if (_updateTask != null && _lastMeshUpdateCompletionFrame != Time.frameCount)
        {
            if (_updateTask.IsCompletedSuccessfully)
            {
                _lastMeshUpdateCompletionFrame = Time.frameCount;
                CompleteMeshUpdate();
                _updateTask.Dispose();
                _updateTask = null;
            }
            else if (_updateTask.IsCompleted || _updateTask.IsCanceled || _updateTask.IsFaulted)
            {
                Debug.Log($"Mesh cut operation failed: {_updateTask}");
                _updateTask.Dispose();
                _updateTask = null;
            }
            else
            {
                return;
            }
        }

        if (_cutUpdateNeeded && _updateTask == null)
        {
            StartMeshUpdate();
        }
    }

    private void StartMeshUpdate()
    {
        _meshCutInfo.Clear();

        //RestoreOriginalMesh();

        if (_originalDmesh3 == null)
        {
            _originalDmesh3 = ProcGeometry.ConvertToDMesh(_originalMesh, true);
        }

        for (int i = _cutSources.Count - 1; i >= 0; i--)
        {
            var source = _cutSources[i];
            if (source == null)
            {
                _cutSources.RemoveAt(i);
                continue;
            }

            if (!source.TryGetComponent<IMeshCut>(out var meshCut))
            {
                _cutSources.RemoveAt(i);
                continue;
            }

            if (!meshCut.GetMeshCutInfo(out var cutInfo))
            {
                continue;
            }

            _meshCutInfo.Add(cutInfo);
        }

        //clear updated needed flag
        _cutUpdateNeeded = false;

        if (_meshCutInfo.Count <= 0)
        {
            RestoreOriginalMesh();
            return;
        }

        Debug.Log($"Starting mesh cut update for {gameObject.name} with {_meshCutInfo.Count} cut(s)");
        _stopwatch.Reset();
        _stopwatch.Start();

        var worldToLocalMatrix = transform.worldToLocalMatrix;

        _updateTask = Task.Run(() =>
        {
            ProcessMeshUpdate(_cancelSource.Token, worldToLocalMatrix);
        }, _cancelSource.Token);
    }

    /// <summary>
    /// Apply cut operations to the original mesh. Runs on worker thread.
    /// </summary>
    /// <param name="cancelToken"></param>
    private void ProcessMeshUpdate(CancellationToken cancelToken, Matrix4x4 worldToLocalMatrix)
    {
        _processedMesh = new DMesh3(_originalDmesh3, true, true, false, true);

        foreach (var cutInfo in _meshCutInfo)
        {
            var cutMeshTransformed = new DMesh3(cutInfo.CutMesh, false, false, false, false);
            
            //Matrix4x4 xform = filter.transform.worldToLocalMatrix * transform.localToWorldMatrix;
            Matrix4x4 xform = worldToLocalMatrix * cutInfo.LocalToWorldMatrix;

            ProcGeometry.ApplyTransform(cutMeshTransformed, xform);
            ProcGeometry.MeshCut(_processedMesh, cutMeshTransformed);
        }

        //compact mesh
        _processedMesh = new DMesh3(_processedMesh, true, true, false, true);
    }

    private void CompleteMeshUpdate()
    {
        if (_processedMesh == null || _meshFilter == null)
            return;        

        var mesh = ProcGeometry.ConvertToUnityMesh(_processedMesh, compactMesh: false);
        mesh.RecalculateTangents();

        _meshFilter.sharedMesh = mesh;

        if (AssociatedMeshCollider != null)
        {
            AssociatedMeshCollider.sharedMesh = mesh;
        }

        _stopwatch.Stop();
        Debug.Log($"Mesh cut update for {gameObject.name} completed in {_stopwatch.ElapsedMilliseconds}ms on frame {Time.frameCount}");
    }

    void RestoreOriginalMesh()
    {
        if (_meshFilter == null || _originalMesh == null)
            return;

        _meshFilter.sharedMesh = _originalMesh;

        if (AssociatedMeshCollider != null)
        {
            AssociatedMeshCollider.sharedMesh = _originalMesh;
        }
    }

}
