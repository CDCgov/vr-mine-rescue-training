using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NIOSH_MineCreation;

public class MineLayerTileManager : SceneManagerBase
{
    
    private struct ConnectionPointData
    {

    }

    public Transform AssetContainer;

    private List<MineLayerTile> _mineTiles;
    private int _queryMask;
    private int _simParentMask;
    private Collider[] _queryResults;
    private MineLayerTile[] _nearbyTiles;

    private System.Diagnostics.Stopwatch _stopwatch;

    //private static GameObject _defaultManagerObj = null;
    //private static MineLayerTileManager _defaultManager = null;

    public static MineLayerTileManager GetDefault()
    {
        //if (_defaultManagerObj != null && _defaultManager != null)
        //    return _defaultManager;

        //_defaultManager = FindObjectOfType<MineLayerTileManager>();
        //_defaultManagerObj = _defaultManager.gameObject;

        //return _defaultManager;
        return Util.GetDefaultManager<MineLayerTileManager>(null, "MineLayerTileManager", false);
    }

    private void Awake()
    {
        _mineTiles = new List<MineLayerTile>(100);
        _queryMask = LayerMask.GetMask("MineSegments");
        _simParentMask = LayerMask.GetMask("Ignore Raycast");
        _queryResults = new Collider[200];
        _nearbyTiles = new MineLayerTile[40];

        _stopwatch = new System.Diagnostics.Stopwatch();
    }

    public void RebuildTileConnections()
    {
        _stopwatch.Reset();
        _stopwatch.Start();

        Physics.SyncTransforms();

        BuildMineTileArray();
        if (_mineTiles.Count <= 0)
            return;

        foreach (var tile in _mineTiles)
        {
            tile.ClearConnections();
        }

        foreach (var tile in _mineTiles)
        {
            CheckTileConnections(tile, _mineTiles);
        }
        _stopwatch.Stop();

        if (ScenarioSaveLoad.Instance != null)
            ScenarioSaveLoad.Instance.MineBounds = ComputeMineBounds();

        Debug.Log($"Tile connection rebuild took {_stopwatch.ElapsedMilliseconds}ms");
    }

    private Bounds ComputeMineBounds()
    {
        if (_mineTiles == null || _mineTiles.Count <= 0)
            return new Bounds();

        var collider = _mineTiles[0].GetComponent<BoxCollider>();
        if (collider == null)
            return new Bounds();

        Bounds bounds = collider.bounds;

        for (int i = 1; i < _mineTiles.Count; i++)
        {
            collider = _mineTiles[i].GetComponent<BoxCollider>();
            if (collider == null)
                continue;

            bounds.Encapsulate(collider.bounds);
        }

        return bounds;

    }

    public int GetPlacedTileCount()
    {
        return _mineTiles.Count;
    }

    public void ClearTileConnections(MineLayerTile tile)
    {
        if (tile == null || tile.Connections == null)
            return;

        for (int i = 0; i < tile.Connections.Length; i++)
        {
            if (tile.Connections[i] != null)
            {
                tile.Connections[i].RemoveConnectionTo(tile);
                tile.Connections[i] = null;
            }
        }
    }

    public void CheckTileConnections(MineLayerTile tile, List<MineLayerTile> tiles)
    {        
        for (int i = 0; i < tiles.Count; i++)
        {
            var nearby = tiles[i];
            CheckTileConnection(tile, nearby);
        }
    }

    public void CheckTileConnections(MineLayerTile tile)
    {
        var count = GetNearbyTiles(tile, _nearbyTiles);

        for (int i = 0; i < count; i++)
        {
            var nearby = _nearbyTiles[i];

            CheckTileConnection(tile, nearby);
        }
    }

    private unsafe bool CheckTileConnection(MineLayerTile a, MineLayerTile b)
    {
        if (a == null || b == null)
            return false;

        if (a.SegmentGeometry == null || a.SegmentGeometry.SegmentConnections == null)
        {
            Debug.LogError($"MineLayerTileManager: Missing SegmentGeometry or SegmentConnections on {a.name}");
            return false;
        }
        if (b.SegmentGeometry == null || b.SegmentGeometry.SegmentConnections == null)
        {
            Debug.LogError($"MineLayerTileManager: Missing SegmentGeometry or SegmentConnections on {b.name}");
            return false;
        }

        if (a.gameObject == b.gameObject)
            return false;

        var a_con = a.SegmentGeometry.SegmentConnections;
        var b_con = b.SegmentGeometry.SegmentConnections;

        var a_pos = stackalloc Vector3[a_con.Length];
        var b_pos = stackalloc Vector3[b_con.Length];

        GetWorldSpaceConnections(a.transform, a_con, a_pos);
        GetWorldSpaceConnections(b.transform, b_con, b_pos);

        for (int a_i = 0; a_i < a_con.Length; a_i++)
        {
            for (int b_i = 0; b_i < b_con.Length; b_i++)
            {
                //var dist = (a_pos[a_i] - b_pos[b_i]).sqrMagnitude;
                var dist = Vector3.Distance(a_pos[a_i], b_pos[b_i]);
                if (dist < 0.5f)
                {
                    ConnectTiles(a, a_i, b, b_i);
                    return true;
                }
            }
        }

        return false;
    }

    private void ConnectTiles(MineLayerTile a, int a_index, MineLayerTile b, int b_index)
    {
        //Debug.Log($"Connecting {a.name}:{a_index} to {b.name}:{b_index}");
        a.Connections[a_index] = b;
        b.Connections[b_index] = a;
    }

    private unsafe void GetWorldSpaceConnections(Transform t, SegmentConnection[] con, Vector3* pos)
    {        
        for (int i = 0; i < con.Length; i++)
            pos[i] = con[i].GetWorldSpaceCentroid(t);
    }

    private void BuildMineTileArray()
    {
        _mineTiles.Clear();

        if (AssetContainer == null)
            return;

        foreach (Transform xform in AssetContainer.transform)
        {
            if (xform == null)
                continue;

            if (xform.gameObject.TryGetComponent<MineLayerTile>(out var mineTile))
            {
                _mineTiles.Add(mineTile);
            }
        }

        Debug.Log($"MineLayerTileManager: Found {_mineTiles.Count} mine tiles");
    }

    public MineLayerTile GetClosestTile(Vector3 pos)
    {
        return GetClosest<MineLayerTile>(pos, _queryMask);
    }

    public SimulatedParent GetClosestSimParent(Vector3 pos)
    {
        return GetClosest<SimulatedParent>(pos, _simParentMask);
    }

    private T GetClosest<T>(Vector3 pos, int mask)
    {
        T closest = default(T);
        float minDist = float.MaxValue;

        SearchNearby(pos, 10, mask, _queryResults, out int count);

        for (int i = 0; i < count; i++)
        {
            var tile = _queryResults[i];

            if (!tile.TryGetComponent<T>(out var comp))
                continue;

            if (!tile.TryGetComponent<Collider>(out var collider))
                continue;

            if (collider.bounds.Contains(pos))
            {
                minDist = 0;
                closest = comp;
                break;
            }

            var closestPoint = collider.ClosestPointOnBounds(pos);
            float dist = Vector3.Distance(pos, closestPoint);

            if (dist < minDist)
            {
                minDist = dist;
                closest = comp;
            }
        }

        return closest;
    }

    public void SearchNearby(Vector3 pos, float radius, int mask, Collider[] results, out int numResults)
    {
        numResults = Physics.OverlapSphereNonAlloc(pos, radius, results,
            mask, QueryTriggerInteraction.Collide);

        if (numResults == results.Length)
            Debug.LogError($"MineLayerTileManager::SearchNearby: Met or exceeded max number of results (radius: {radius:F2})");
    }

    public void SearchForNearbyTiles(Vector3 pos, float radius, Collider[] results, out int numResults)
    {
        SearchNearby(pos, radius, _queryMask, results, out numResults);
    }

    public void SearchForNearbySimParents(Vector3 pos, float radius, Collider[] results, out int numResults)
    {
        SearchNearby(pos, radius, _simParentMask, results, out numResults);
    }


    public int GetNearbyTiles(MineLayerTile tile, MineLayerTile[] results)
    {
        if (results == null || _queryResults == null)
            return -1;

        tile.SearchForNearbyTiles(tile.transform.position, _queryResults, out int count);

        int index = 0;

        for (int i = 0; i < count; i++)
        {
            var nearby = _queryResults[i];
            if (nearby == null)
                continue;

            if (nearby.TryGetComponent<MineLayerTile>(out var nearbyTile))
            {
                results[index] = nearbyTile;
                index++;
                if (index >= results.Length)
                {
                    Debug.LogError("GetNearbyTiles: Met or exceeded results buffer size");
                    break;
                }
            }
        }

        return index;

    }


    
}
